using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFC.Canteen.Models;

namespace SFC.Canteen.Converters
{
    class ProductConverter : ConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            var prod = value as Product;
            if (prod == null) return "PRODUCT CODE";
            return prod.ToString();
        }
    }
}
