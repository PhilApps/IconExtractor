using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PhilUtils.WPF;

namespace IconExtractor
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Model.Folder = "C:\\Windows\\System32";
        }

        private IconExtractorModel Model => (IconExtractorModel)DataContext;

        private async void Test_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var coll = await Model.GetIconsAsync(CancellationToken.None, null);
                foreach (var icon in coll.Values.SelectMany(c => c))
                {
                    Image img = new Image();
                    img.BeginInit();
                    img.Source = icon;
                    img.EndInit();
                    _testPanel.Children.Add(img);
                }
                this.MsgBoxInfo("OK!");
            }
            catch (Exception exc)
            {
                this.MsgBoxError(exc.Message);
            }
        }
    }
}
