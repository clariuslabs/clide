using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clide
{
	internal class NullErrorsManager : IErrorsManager
	{
		public IErrorItem AddError(string message, Action<IErrorItem> handler)
		{
			return null;
		}

		public void ShowErrors()
		{
		}

		public void ClearErrors()
		{
		}
	}
}
