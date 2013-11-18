using Clide;
using System.Windows;
using System.Windows.Controls;

namespace IntegrationPackage
{
    /// <summary>
    /// Interaction logic for MyControl.xaml
    /// </summary>
    public partial class MyControl : UserControl
    {
        public MyControl()
        {
            InitializeComponent();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(string.Format(
                "Clide Version for package {0}: {1}", this.GetType(), typeof(IDevEnv).GetType().Assembly.GetName().Version));
        }
    }
}