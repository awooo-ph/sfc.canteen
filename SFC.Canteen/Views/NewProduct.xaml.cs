using System.Windows;

namespace SFC.Canteen.Views
{
    /// <summary>
    /// Interaction logic for NewProduct.xaml
    /// </summary>
    public partial class NewProduct : Window
    {
        public NewProduct()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
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
