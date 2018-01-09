﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using SFC.Canteen.Models;

namespace SFC.Canteen.ViewModels
{
    class PosViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _Code;

        public string Code
        {
            get => _Code;
            set
            {
                if(value == _Code)
                    return;
                _Code = value;
                OnPropertyChanged(nameof(Code));
                AddProduct = Product.Cache.FirstOrDefault(x => x.Code == value);
            }
        }

        private double _Quantity = 1;

        public double Quantity
        {
            get => _Quantity;
            set
            {
                if(value == _Quantity)
                    return;
                _Quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }
        
        private Product _AddProduct;

        public Product AddProduct
        {
            get => _AddProduct;
            set
            {
                if(value == _AddProduct) return;
                _AddProduct = value;
                OnPropertyChanged(nameof(AddProduct));
            }
        }

        private ObservableCollection<SaleItem> _items;
        public ObservableCollection<SaleItem> Items => _items ?? (_items = new ObservableCollection<SaleItem>());

        private Customer _Customer;

        public Customer Customer
        {
            get => _Customer;
            set
            {
                if(value == _Customer)
                    return;
                _Customer = value;
                OnPropertyChanged(nameof(Customer));
                OnPropertyChanged(nameof(IsTransactionStarted));
            }
        }
        
        public double TotalAmount => Items.Sum(x=>x.Amount);

        public double Change => Customer?.Credits - TotalAmount ?? 0;
        
        private bool _IsTransactionStarted;

        public bool IsTransactionStarted
        {
            get => Customer!=null;
            set
            {
                if(value == _IsTransactionStarted)
                    return;
                _IsTransactionStarted = value;
                OnPropertyChanged(nameof(IsTransactionStarted));
            }
        }
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PosViewModel(Customer customer)
        {
            Customer = customer;
        }

        private string _SearchKeyword = "";

        public string SearchKeyword
        {
            get => _SearchKeyword;
            set
            {
                if (value == _SearchKeyword)
                    return;
                _SearchKeyword = value;
                OnPropertyChanged(nameof(SearchKeyword));
                Result.Filter = Filter;
                Result.MoveCurrentToFirst();
            }
        }

        private ListCollectionView _result;

        public ListCollectionView Result
        {
            get
            {
                if (_result != null)
                    return _result;
                _result = new ListCollectionView(Customer.Cache);
                _result.Filter = Filter;
                return _result;
            }
        }

        private bool Filter(object o)
        {
            var customer = o as Customer;
            if (customer == null)
                return false;
            return customer.RFID.Contains(SearchKeyword) ||
                   customer.Fullname.ToLower().Contains(SearchKeyword?.ToLower());
        }

        private ICommand _selectCustomerCommand;

        public ICommand SelectCustomerCommand =>
            _selectCustomerCommand ?? (_selectCustomerCommand = new DelegateCommand(
                d =>
                {
                    Customer = Result.CurrentItem as Customer;
                    IsTransactionStarted = true;
                },d=>Result.CurrentItem!=null));

        private ICommand _cancelCommand;

        public ICommand CancelCommand => _cancelCommand ?? (_cancelCommand = new DelegateCommand(d =>
        {
            Customer = null;
            Items.Clear();
        }));

        private ICommand _addProductCommand;

        public ICommand AddProductCommand => _addProductCommand ?? (_addProductCommand = new DelegateCommand(d =>
        {
            var item = Items.FirstOrDefault(x => x.ProductId == AddProduct.Id);
            if (item == null)
            {
                item = new SaleItem();
                item.ProductId = AddProduct.Id;
                Items.Add(item);
            }
            item.Quantity+=Quantity;
            item.Amount = item.Product.Price * item.Quantity;
            Quantity = 1;
            Code = "";
            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(Change));
        },d=>Customer!=null && AddProduct!=null && AddProduct.Quantity>=Quantity && AddProduct.Price*Quantity+TotalAmount<=Customer.Credits));

        private ICommand _checkoutCommand;

        public ICommand CheckoutCommand => _checkoutCommand ?? (_checkoutCommand = new DelegateCommand(d =>
        {
            if (Customer.IsStudent && Customer.Credits < TotalAmount)
            {
                MessageBox.Show($"{Customer.Fullname} does not have enough credits.");
                return;
            }
            
            var sale = new Sale()
            {
                CustomerId = Customer.Id,
                Time = DateTime.Now,
                Amount = TotalAmount
            };
            sale.Save();

            foreach (var saleItem in Items)
            {
                saleItem.SaleId = sale.Id;
                saleItem.Product.Update(nameof(Product.Quantity),saleItem.Product.Quantity - saleItem.Quantity);
                saleItem.Save();
            }

            Customer.Update(nameof(Customer.Credits),Customer.Credits-TotalAmount);
            Items.Clear();
            Customer = null;
            OnPropertyChanged(nameof(Change));
            OnPropertyChanged(nameof(TotalAmount));
        },d=>Items.Count>0));
    }
}
