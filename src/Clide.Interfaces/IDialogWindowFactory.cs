namespace Clide
{
    /// <summary>
    /// Provides dialog windows creation functionality, properly setting the 
    /// window owner and invoking the creation on the proper UI thread.
    /// </summary>
    public interface IDialogWindowFactory : IFluentInterface
    {
        /// <summary>
        /// Creates a <see cref="IDialogWindow"/> dialog as child of the main Visual Studio window.
        /// </summary>
        /// <typeparam name="TView">The type of the window to create.</typeparam>
        /// <returns>
        /// The created <see cref="IDialogWindow"/> dialog.
        /// </returns>
        TView CreateDialog<TView>() where TView : IDialogWindow, new();
    }
}
