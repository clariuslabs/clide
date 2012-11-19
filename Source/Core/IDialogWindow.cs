#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
namespace Clide
{
    using System.Windows;
    
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