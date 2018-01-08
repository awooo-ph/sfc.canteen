using System.Windows;

namespace SFC.Canteen.Views
{
    /// <summary>
    /// Interaction logic for TopUp.xaml
    /// </summary>
    public partial class TopUp : Window
    {
        public TopUp()
        {
            InitializeComponent();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AcceptClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
