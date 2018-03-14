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

namespace SFC.Canteen.Views
{
    /// <summary>
    /// Interaction logic for ProductSelector.xaml
    /// </summary>
    public partial class ProductSelector : Window
    {
        public ProductSelector()
        {
            InitializeComponent();
        }

        public Product SelectedProduct { get; set; }
        
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedProduct = Box.SelectedItem as Product;
            DialogResult = true;
            Close();
        }
    }
}
