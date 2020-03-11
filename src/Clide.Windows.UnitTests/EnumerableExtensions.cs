using System;
using System.Collections.Generic;

namespace Clide
{
    public static class EnumerableExtensions
    {
        public static IObservable<T> ToObservable<T>(this IEnumerable<T> source)
            => new EnumerableObservable<T>(source);

        class EnumerableObservable<T> : IObservable<T>
        {
            IEnumerable<T> source;

            public EnumerableObservable(IEnumerable<T> source) => this.source = source;

            public IDisposable Subscribe(IObserver<T> observer)
            {
                foreach (var item in source)
                {
                    observer.OnNext(item);
                }
                observer.OnCompleted();
                return Disposable.Empty;
            }
        }
    }
}
