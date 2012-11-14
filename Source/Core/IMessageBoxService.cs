using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Clide
{
	public interface IMessageBoxService
	{
		void Show(string message, string title = "Visual Studio", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.OK);
		MessageBoxResult Prompt(string message, string title = "Visual Studio", MessageBoxButton button = MessageBoxButton.OKCancel, MessageBoxImage icon = MessageBoxImage.Question, MessageBoxResult defaultResult = MessageBoxResult.OK);
        string InputBox(string message, string title = "Visual Studio");
    }
}
