using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clide
{
	/// <summary>
	/// Provides members to interact with the Error List window
	/// </summary>
	public interface IErrorsManager
	{
		/// <summary>
		/// Clear the errors
		/// </summary>
		void ClearErrors();

		/// <summary>
		/// Shows the errors
		/// </summary>
		void ShowErrors();

		/// <summary>
		/// Adds an error to the error list
		/// </summary>
		IErrorItem AddError(string text, Action<IErrorItem> handler);
	}
}
