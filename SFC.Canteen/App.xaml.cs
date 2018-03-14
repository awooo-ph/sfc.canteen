using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using SFC.Canteen.Models;
using SFC.Canteen.Properties;
using SFC.Canteen.ViewModels;

namespace SFC.Canteen
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            awooo.IsRunning = true;
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            MainViewModel.Session?.Update(nameof(Session.TimeOut),DateTime.Now);
            Settings.Default.Save();
            awooo.IsRunning = false;
            base.OnExit(e);
        }
    }
}
