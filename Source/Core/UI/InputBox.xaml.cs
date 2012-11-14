using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Clide
{
	/// <summary>
	/// Interaction logic for InputBox.xaml
	/// </summary>
	public partial class InputBox : Window
	{
		public InputBox()
		{
			InitializeComponent();
		}

		public string ResponseText
		{
			get { return this.ResponseTextBox.Text; }
			set { this.ResponseTextBox.Text = value; }
		}

		public string Message
		{
			get { return this.MessageText.Text; }
			set { this.MessageText.Text = value; }
		}

		public static string Show(string message, Window owner = null)
		{
			var dialog = new InputBox() { Message = message };
			dialog.Owner = owner ?? Application.Current.MainWindow;
			if (dialog.ShowDialog() == true)
			{
				return dialog.ResponseText;
			}

			return null;
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
	}
}
