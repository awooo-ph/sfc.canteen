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
using System.Windows.Navigation;
using System.Windows.Shapes;
using SFC.Canteen.Models;

namespace SFC.Canteen.Views
{
    /// <summary>
    /// Interaction logic for Employees.xaml
    /// </summary>
    public partial class Employees : UserControl
    {
        public Employees()
        {
            InitializeComponent();
        }

        private void StudentsDrop(object sender, DragEventArgs e)
        {
            var stud = ((FrameworkElement) e.OriginalSource).DataContext as Customer;

            if (e.Data.GetDataPresent(DataFormats.FileDrop) && stud != null)
            {
                var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                var file = files?.FirstOrDefault();
                if (file == null)
                    return;
                if (ImageProcessor.IsAccepted(file))
                {
                    var pic = stud.Picture;
                    stud.Update(nameof(Customer.Picture), ImageProcessor.ResizeImage(file));
                    return;
                }
            }
            e.Effects = DragDropEffects.None;
        }

        private void StudentsDragEnter(object sender, DragEventArgs e)
        {

            var stud = ((FrameworkElement) e.OriginalSource).DataContext as Customer;

            if (e.Data.GetDataPresent(DataFormats.FileDrop) && stud != null)
            {
                var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                var file = files?.FirstOrDefault();
                if (ImageProcessor.IsAccepted(file))
                {
                    e.Effects = DragDropEffects.All;
                    return;
                }
            }
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }
    }
}
