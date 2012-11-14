using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clide
{
	/// <summary>
	/// Provides dialog windows creation functionality, properly setting the 
	/// window owner, and optionally setting the dialog data context for
	/// data binding.
	/// </summary>
	public interface IDialogWindowFactory : IFluentInterface
	{
		/// <summary>
		/// Creates a <see cref="Window"/> dialog as child of the main Visual Studio window, 
		/// and sets its <see cref="IDialogWindow.DataContext"/> to an instance of 
		/// the given <typeparamref name="TDataContext"/> class.
		/// </summary>
		/// <param name="dynamicContextValues">Optional objects to make available for the <typeparamref name="TDataContext"/> creation via MEF.</param>
		/// <typeparam name="TView">The type of the window to create.</typeparam>
		/// <typeparam name="TDataContext">The type of the data context to create or retrieve from <see cref="IComponentModel.GetService"/>, 
		/// so it needs to be exported in the environment.</typeparam>
		/// <returns>
		/// The created <see cref="Window"/> dialog.
		/// </returns>
		TView CreateDialog<TView, TDataContext>(params object[] dynamicContextValues)
			where TView : IDialogWindow, new()
			where TDataContext : class;

		/// <summary>
		/// Creates a <see cref="Window"/> dialog as child of the main Visual Studio window.
		/// </summary>
		/// <typeparam name="TView">The type of the window to create.</typeparam>
		/// <returns>
		/// The created <see cref="Window"/> dialog.
		/// </returns>
		T CreateDialog<T>() where T : IDialogWindow, new();
	}
}
