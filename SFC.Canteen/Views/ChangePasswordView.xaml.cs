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
using System.Windows.Shapes;
using SFC.Canteen.Models;
using SFC.Canteen.ViewModels;

namespace SFC.Canteen.Views
{
    /// <summary>
    /// Interaction logic for ChangePasswordView.xaml
    /// </summary>
    public partial class ChangePasswordView : Window
    {
        public ChangePasswordView()
        {
            InitializeComponent();
        }

        private void ExitClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (NewPassword.Password != NewPassword2.Password)
            {
                MessageBox.Show("Password did not match");
                return;
            }
            if (string.IsNullOrEmpty(NewPassword.Password))
            {
                MessageBox.Show("Password is required.");
                return;
            }
            if (MainViewModel.Instance.CurrentUser?.Password != CurrentPassword.Password)
            {
                MessageBox.Show("Invalid password.");
                return;
            }

            MainViewModel.Instance.CurrentUser.Update(nameof(User.Password), NewPassword.Password);
            
            DialogResult = true;
            Close();
        }
    }
}
