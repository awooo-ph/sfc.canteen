using System.Windows;

namespace SFC.Canteen.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void ExitClicked(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown(0);
        }

        private void LoginClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Username.Text) || string.IsNullOrEmpty(Password.Password)) return;
            DialogResult = true;
            Close();
        }
    }
}
