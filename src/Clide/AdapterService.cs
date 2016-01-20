using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FromTo = System.Tuple<System.Type, System.Type>;

namespace Clide
{
	/// <summary>
	/// Default implementation of the <see cref="IAdapterService"/>.
	/// </summary>
	[Export(typeof(IAdapterService))]
	[PartCreationPolicy(CreationPolicy.Shared)]
	internal class AdapterService : IAdapterService
	{
		static readonly MethodInfo AdaptExpressionGenerator = typeof(AdapterService).GetMethod("GetAdaptExpression", BindingFlags.NonPublic | BindingFlags.Static);

		ConcurrentDictionary<Type, IEnumerable<TypeInheritance>> cachedOrderedTypeHierarchies = new ConcurrentDictionary<Type, IEnumerable<TypeInheritance>>();
		ConcurrentDictionary<FromTo, Func<IAdapter, object, object>> cachedAdaptMethods = new ConcurrentDictionary<FromTo, Func<IAdapter, object, object>>();
        ConcurrentDictionary<FromTo, IAdapter> cachedFromToAdapters = new ConcurrentDictionary<FromTo, IAdapter>();

		List<AdapterInfo> allAdapters;

        /// <summary>
        /// Initializes the adapter service with the given set of adapters.
        /// </summary>
        internal AdapterService(params IAdapter[] adapters)
            : this((IEnumerable<IAdapter>)adapters)
        {
        }

		/// <summary>
		/// Initializes the adapter service with the given set of adapters.
		/// </summary>
		[ImportingConstructor]
		public AdapterService ([ImportMany] IEnumerable<IAdapter> adapters)
		{
            var genericAdapter = typeof(IAdapter<,>);
			allAdapters = adapters
                // Multiple implementations of IAdapter<TFrom, TTo> supported per adapter for convenience.
                .SelectMany(adapter => adapter
                .GetType()
                .GetInterfaces()
                // Keep only the implementations of the generic interface.
                .Where(conversion => conversion.IsGenericType && conversion.GetGenericTypeDefinition() == genericAdapter)
                .Select(conversion => new AdapterInfo
                {
                    Adapter = adapter,
                    From = conversion.GetGenericArguments()[0],
                    To = conversion.GetGenericArguments()[1]
                }))
                .ToList();

            var duplicates = this.allAdapters
                .GroupBy(info => new FromTo(info.From, info.To))
                .ToDictionary(group => group.Key, group => group.ToList())
                .Where(group => group.Value.Count > 1)
                .SelectMany(group => group.Value)
                .ToList();

            if (duplicates.Count > 0)
                throw new ArgumentException("Duplicate adapters: " + string.Join(Environment.NewLine,
                    duplicates.Select(adapter =>
                        string.Format("{0}->{1}: {2}", adapter.From, adapter.To, adapter.Adapter))));
		}

		public IAdaptable<TSource> Adapt<TSource> (TSource source) where TSource : class => new Adaptable<TSource> (this, source);

		TTarget Adapt<TSource, TTarget>(TSource source)
            where TSource : class
            where TTarget : class
		{
            // Null always adapts to null.
			if (source == null)
				return default(TTarget);

            // We first try the most specific source type, the 
            // actual implementation.
			var sourceType = source.GetType();
            var targetType = typeof(TTarget);
            // Avoid the more costly conversion if types are 
            // directly assignable.
			if (targetType.IsAssignableFrom(sourceType) || targetType.IsAssignableFrom(typeof(TSource)))
				return source as TTarget;

            var fromTo = new FromTo(sourceType, targetType);
            var adapter = cachedFromToAdapters.GetOrAdd(fromTo, FindAdapter);
            if (adapter == null)
            {
                // Try again but with the explicit TSource we were passed-in.
                fromTo = new FromTo(typeof(TSource), targetType);
                adapter = cachedFromToAdapters.GetOrAdd(fromTo, FindAdapter);
                if (adapter == null)
                    return default(TTarget);
            }

            var adaptMethod = GetAdaptMethod(fromTo, adapter);

            return adaptMethod.Invoke(adapter, source) as TTarget;
		}

        IAdapter FindAdapter(FromTo fromTo)
        {
            var fromType = fromTo.Item1;
            var toType = fromTo.Item2;
            var fromInheritance = GetInheritance(fromType);

            // Search by inheritance proximity of the target and source type
            var adapter = allAdapters
                // Filter out those that are compatible both for the source and the target.
                .Where(info => toType.IsAssignableFrom(info.To) && info.From.IsAssignableFrom(fromType))
                .Select(info => new
                {
                    Adapter = info.Adapter,
                    // Gets the distance between the requested From type to the adapter From type.
                    FromInheritance = fromInheritance.FirstOrDefault(x => x.Type == info.From),
                    // Gets the distance between the requested To type to the adapter To type.
                    ToInheritance = GetInheritance(info.To).FirstOrDefault(x => x.Type == toType)
                })
                // We first order by the most specific adapter with regards to the source type
                .OrderBy(info => info.FromInheritance.Distance)
                // Then we get the most specific (meaning most derived type, hence descending) 
                // of the target type. This means that the more generic adapters will only be 
                // used when no other more specific adapter exists. This allows implementers 
                // to only provide the most specific conversions, and have those apply to 
                // more generic adapt requests.
                .ThenByDescending(info => info.ToInheritance.Distance)
                .Select(info => info.Adapter)
                .FirstOrDefault();

            return adapter;
        }

		Func<IAdapter, object, object> GetAdaptMethod(FromTo fromTo, IAdapter adapter)
		{
			return cachedAdaptMethods.GetOrAdd(
				fromTo,
				key => ((Expression<Func<IAdapter, object, object>>)
                    AdaptExpressionGenerator.MakeGenericMethod(key.Item1, key.Item2).Invoke(null, null))
					.Compile());
		}

		static Expression<Func<IAdapter, object, object>> GetAdaptExpression<TFrom, TTo>()
		{
			return (adapter, source) => ((IAdapter<TFrom, TTo>)adapter).Adapt((TFrom)source);
		}

		IEnumerable<TypeInheritance> GetInheritance(Type sourceType)
		{
            return cachedOrderedTypeHierarchies.GetOrAdd(
                sourceType,
                type => type.GetInheritanceTree()
                    .Inheritance
                    .Traverse(TraverseKind.BreadthFirst, t => t.Inheritance)
                    .Concat(new[] { new TypeInheritance(sourceType, 0) })
                    .OrderBy(t => t.Distance)
                    .Distinct()
                    // If there are duplicates, take the farthest type
                    .GroupBy(t => t.Type)
                    .Select(group => group.OrderByDescending(h => h.Distance).First())
                    // Do a final order by distance.
                    .OrderBy(t => t.Distance)
                    .ToList());
		}

        class AdapterInfo
        {
            public IAdapter Adapter;
            public Type From;
            public Type To;
        }

        class Adaptable<TSource> : IAdaptable<TSource>
            where TSource : class
        {
            AdapterService service;
            TSource source;

            public Adaptable(AdapterService service, TSource source)
            {
                this.service = service;
                this.source = source;
            }

			public T As<T> () where T : class => service.Adapt<TSource, T> (source);
		}
	}
}
