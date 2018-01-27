using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SFC.Canteen.Properties;

namespace SFC.Canteen.Views
{
    /// <summary>
    /// Interaction logic for OptionsView.xaml
    /// </summary>
    public partial class OptionsView : Window
    {
        public OptionsView()
        {
            InitializeComponent();
            Messenger.Default.AddListener(Messages.ScannerRegistered, () =>
            {
                SynchronizationContext.Current.Post(d =>
                {
                    RegButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Collapsed;
                },null);
            });
        }

        private void RegButton_OnClick(object sender, RoutedEventArgs e)
        {
            RfidScanner.RegisterScanner();
            RegButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Visible;
            ProgressBar.Visibility = Visibility.Visible;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            RfidScanner.CancelRegistration();
            ProgressBar.Visibility = Visibility.Collapsed;
            RegButton.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Collapsed;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if(RfidScanner.IsWaitingForScanner)
                RfidScanner.CancelRegistration();
            base.OnClosing(e);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
        }
    }
}
