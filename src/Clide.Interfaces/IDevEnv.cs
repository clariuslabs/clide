namespace Clide
{
	public interface IDevEnv
	{
		/// <summary>
		/// Gets a value indicating whether the environment is running with 
		/// elevated permissions.
		/// </summary>
		bool IsElevated { get; }

		/// <summary>
		/// Gets the dialog window factory.
		/// </summary>
		IDialogWindowFactory DialogWindowFactory { get; }

		/// <summary>
		/// Gets the errors manager
		/// </summary>
		IErrorsManager Errors { get; }

		/// <summary>
		/// Gets the message box service.
		/// </summary>
		IMessageBoxService MessageBoxService { get; }

		/// <summary>
		/// Gets the output window manager.
		/// </summary>
		IOutputWindowManager OutputWindow { get; }

		/// <summary>
		/// Gets the service locator.
		/// </summary>
		IServiceLocator ServiceLocator { get; }

		/// <summary>
		/// Gets the status bar.
		/// </summary>
		IStatusBar StatusBar { get; }

		/// <summary>
		/// Exits Visual Studio.
		/// </summary>
		/// <param name="saveAll">Whether to save all pending changes before exiting.</param>
		void Exit(bool saveAll = true);

		/// <summary>
		/// Restarts Visual Studio.
		/// </summary>
		/// <param name="saveAll">Whether to save all pending changes before exiting.</param>
		/// <returns><see langword="true"/> if the operation succeeded; <see langword="false"/> otherwise.</returns>
		bool Restart(bool saveAll = true);
	}
}