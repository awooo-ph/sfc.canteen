using System.Threading;
using System.Windows;

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
            awooo.Context = SynchronizationContext.Current;
        }
    }
}
