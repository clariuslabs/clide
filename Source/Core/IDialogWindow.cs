using System.Windows;

namespace Clide
{
	/// <summary>
	/// Represents a dialog window. This interface only needs to be added to the 
	/// implementing interfaces for any <see cref="Window"/> and no additional code is 
	/// required. All members are already implemented by the base class <see cref="Window"/>.
	/// </summary>
	public interface IDialogWindow
	{
		/// <summary>
		/// Gets or sets the data context, typically a view model.
		/// </summary>
		object DataContext { get; set; }

		/// <summary>
		/// Gets or sets the dialog result.
		/// </summary>
		bool? DialogResult { get; set; }

		/// <summary>
		/// Opens a dialog and returns only when the newly opened dialog is closed.
		/// </summary>
		/// <returns>A value that signifies how a dialog was closed by the user (canceled or not).</returns>
		bool? ShowDialog();

		/// <summary>
		/// Closes the dialog.
		/// </summary>
		void Close();
	}
}