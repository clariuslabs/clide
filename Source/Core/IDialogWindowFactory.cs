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
