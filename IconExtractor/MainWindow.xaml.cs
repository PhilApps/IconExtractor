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
            _testPanel.Children.Clear();

            try
            {
                var dictionary = await Model.GetIconsAsync(CancellationToken.None, null);

                foreach (var elt in dictionary.Where(el => el.Value.Any()))
                {
                    Expander newExpander = new Expander();
                    WrapPanel newWrapPanel = new WrapPanel();

                    newExpander.Header = elt.Key.FullName;
                    newExpander.Content = newWrapPanel;

                    foreach (var icon in elt.Value)
                    {
                        Image img = new Image();
                        using (img.SetInitializing())
                        {
                            img.Source = icon;
                            img.Stretch = Stretch.None;
                        }
                        newWrapPanel.Children.Add(img);
                    }

                    _testPanel.Children.Add(newExpander);
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
