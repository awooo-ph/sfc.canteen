using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using SFC.Canteen.Models;

namespace SFC.Canteen.ViewModels
{
    class CustomerFinderViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _SearchKeyword;

        public string SearchKeyword
        {
            get => _SearchKeyword;
            set
            {
                if(value == _SearchKeyword)
                    return;
                _SearchKeyword = value;
                OnPropertyChanged(nameof(SearchKeyword));
                Result.Filter = Filter;
            }
        }

        private ListCollectionView _result;

        public ListCollectionView Result
        {
            get
            {
                if (_result != null) return _result;
                _result = new ListCollectionView(Customer.Cache);
                _result.Filter = Filter;
                return _result;
            }
        }

        private bool Filter(object o)
        {
            var customer = o as Customer;
            if (customer == null) return false;
            return customer.RFID.Contains(SearchKeyword) || customer.Fullname.ToLower().Contains(SearchKeyword.ToLower());
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
