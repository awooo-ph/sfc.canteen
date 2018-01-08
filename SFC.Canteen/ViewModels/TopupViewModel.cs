using System.ComponentModel;
using System.Linq;
using SFC.Canteen.Models;

namespace SFC.Canteen.ViewModels
{
    class TopupViewModel : INotifyPropertyChanged
    {
        public TopupViewModel(Customer customer)
        {
            _RFID = customer?.RFID;
            Customer = customer;
        }
        
        private string _RFID;

        public string RFID
        {
            get => _RFID;
            set
            {
                if(value == _RFID)
                    return;
                _RFID = value;
                OnPropertyChanged(nameof(RFID));
                Customer = Customer.Cache.FirstOrDefault(x => x.RFID == value);
            }
        }

        private double _Credits;

        public double Credits
        {
            get => _Credits;
            set
            {
                if(value == _Credits)
                    return;
                _Credits = value;
                OnPropertyChanged(nameof(Credits));
            }
        }

        private Customer _Customer;

        public Customer Customer
        {
            get => _Customer;
            set
            {
                if(value?.Id == _Customer?.Id)
                    return;
                _Customer = value;
                OnPropertyChanged(nameof(Customer));
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
