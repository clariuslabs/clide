namespace Clide
{
	public interface IStatusBar : IFluentInterface
	{
		void Clear();

		void ShowMessage(string message);

		void ShowProgress(string message, int complete, int total);
	}
}