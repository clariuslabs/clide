using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clide.Solution.Implementation
{
	interface IPropertyAccessor
	{
		bool TryGetProperty(string propertyName, out object result);
		bool TrySetProperty(string propertyName, object value);
	}
}
