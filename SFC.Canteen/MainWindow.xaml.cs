using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Effects;
using SFC.Canteen.ViewModels;
using SFC.Canteen.Views;

namespace SFC.Canteen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if(awooo.Context==null)
                awooo.Context = SynchronizationContext.Current;
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            RfidScanner.UnHook();
            base.OnClosing(e);
            
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            RfidScanner.Hook(this);
            base.OnSourceInitialized(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (MainViewModel.Instance.CurrentUser == null)
            {
                ShowLogin();
            }
            else
            {
                Grid.Effect = null;
            }
        }

        private bool _loginShown = false;
        public void ShowLogin()
        {
            if (_loginShown) return;
            _loginShown = true;
            Grid.Effect = new BlurEffect()
            {
                Radius = 10,
                RenderingBias = RenderingBias.Performance,
                KernelType = KernelType.Gaussian
            };
            var login = new LoginView();
            var res = false;

            while (true)
            {
                try
                {
                    res = login.ShowDialog() ?? false;
                    break;
                }
                catch (Exception e)
                {
                    //
                }
            }

            if (res)
            {
                MainViewModel.Instance.Login(login.Username.Text, login.Password.Password);
            }
            else
            {
                Close();
                Application.Current.Shutdown(0);
                return;
            }
            _loginShown = false;

            if (MainViewModel.Instance.CurrentUser == null)
                ShowLogin();
            else
                Grid.Effect = null;
        }

        private void MinimizeClicked(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeClicked(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MainWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
