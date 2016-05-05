using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;

namespace Clide.Composition
{
	internal class ServiceProviderLocator : ServiceLocatorImplBase
	{
		static readonly IEnumerable<object> empty = new object[0];

		IServiceProvider provider;

		public ServiceProviderLocator (IServiceProvider provider)
		{
			this.provider = provider;
		}

		protected override IEnumerable<object> DoGetAllInstances (Type serviceType)
		{
			return empty;
		}

		protected override object DoGetInstance (Type serviceType, string key)
		{
			return provider.GetService (serviceType);
		}
	}
}
