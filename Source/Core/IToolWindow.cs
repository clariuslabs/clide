
namespace Clide
{
	public interface IToolWindow : IFluentInterface
	{
		bool IsOpen { get; }
		void Open();
		void Close();
	}
}
