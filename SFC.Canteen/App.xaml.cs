using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

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
            awooo.IsRunning = false;
            base.OnExit(e);
        }
    }
}
