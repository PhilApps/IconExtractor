using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace IconExtractor
{
    /// <summary>
    /// Logique d'interaction pour ProgressWindow.xaml
    /// </summary>
    sealed partial class ProgressWindow : Window
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Progress<double> _progress;
        private Task _currentTask;
        public CancellationToken CancellationToken => _cts.Token;
        private ProgressWindow()
        {
            InitializeComponent();
            _progress = new Progress<double>(Progression);
        }

        private void Progression(double progress)
        {
            if (progress < 0.0)
            {
                _prgrBar.IsIndeterminate = true;
                _prgrBar.Value = 0.0;
            }
            else
            {
                _prgrBar.IsIndeterminate = false;
                _prgrBar.Value = progress;
            }
        }

        internal static Task Start(Func<CancellationToken, IProgress<double>, Task> fctTask, Window parentWindow)
        {
            ProgressWindow wnd = new ProgressWindow();

            EventHandler handlerInit = null;
            handlerInit = async (snd, evt) =>
            {
                wnd.ContentRendered -= handlerInit;
                wnd._currentTask = fctTask(wnd._cts.Token, wnd._progress);
                try
                {
                    await wnd._currentTask;
                    wnd.DialogResult = !wnd._currentTask.IsCanceled;
                }
                catch { wnd.DialogResult = false; }
            };
            wnd.ContentRendered += handlerInit;
            if (parentWindow != null) wnd.Owner = parentWindow;
            wnd.ShowDialog();

            return wnd._currentTask;
        }

        internal static Task<T> Start<T>(Func<CancellationToken, IProgress<double>, Task<T>> fctTask, Window parentWindow)
        {
            ProgressWindow wnd = new ProgressWindow();

            EventHandler handlerInit = null;
            handlerInit = async (snd, evt) =>
            {
                wnd.ContentRendered -= handlerInit;
                wnd._currentTask = fctTask(wnd._cts.Token, wnd._progress);
                try
                {
                    await wnd._currentTask;
                    wnd.DialogResult = !wnd._currentTask.IsCanceled;
                }
                catch { wnd.DialogResult = false; }
            };
            wnd.ContentRendered += handlerInit;
            if (parentWindow != null) wnd.Owner = parentWindow;
            wnd.ShowDialog();

            return (Task<T>)wnd._currentTask;
        }

        private void _btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _cts.Cancel();
            _btnCancel.IsEnabled = false;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = !_currentTask.IsCompleted;
        }
    }
}
