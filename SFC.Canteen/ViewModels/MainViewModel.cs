using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using SFC.Canteen.Models;
using SFC.Canteen.Views;

namespace SFC.Canteen.ViewModels
{
    class MainViewModel : MarkupExtension, INotifyPropertyChanged
    {
        public const int STUDENTS = 0, EMPLOYEES = 1, PRODUCTS=2, POS=3;

        public MainViewModel()
        {
            if(_instance!=null) throw new Exception("awooo");
            _instance = this;
        }

        private static MainViewModel _instance;
        public static MainViewModel Instance => _instance ?? (_instance = new MainViewModel());

        private ICommand _ShowTabCommand;

        public ICommand ShowTabCommand => _ShowTabCommand ?? (_ShowTabCommand = new DelegateCommand<int>(tab =>
        {
            SelectedTab = tab;
        }));

        private int _SelectedTab;

        public int SelectedTab
        {
            get => _SelectedTab;
            set
            {
                if(value == _SelectedTab)
                    return;
                _SelectedTab = value;
                OnPropertyChanged(nameof(SelectedTab));
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        private ListCollectionView _students;

        public ListCollectionView Students
        {
            get
            {
                if (_students != null) return _students;
                _students = new ListCollectionView(Customer.Cache);
                _students.Filter = FilterStudents;
                Customer.Cache.CollectionChanged += (sender, args) =>
                {
                    _students.Filter = FilterStudents;
                };
                return _students;
            }
        }

        private string _StudentsKeyword;

        public string StudentsKeyword
        {
            get => _StudentsKeyword;
            set
            {
                if(value == _StudentsKeyword)
                    return;
                _StudentsKeyword = value;
                OnPropertyChanged(nameof(StudentsKeyword));
                Students.Filter = FilterStudents;
            }
        }

        

        private bool FilterStudents(object o)
        {
            var stud = (o as Customer)?.IsStudent??false;
            if (!stud) return false;
            return string.IsNullOrEmpty(StudentsKeyword) ||
                   ((o as Customer)?.Fullname.Contains(StudentsKeyword) ?? false);
        }

        private ListCollectionView _Employees;

        public ListCollectionView Employees
        {
            get
            {
                if (_Employees != null)
                    return _Employees;
                _Employees = new ListCollectionView(Customer.Cache);
                _Employees.Filter = FilterEmployees;
                Customer.Cache.CollectionChanged += (sender, args) =>
                {
                    _Employees.Filter = FilterEmployees;
                };
                return _Employees;
            }
        }

        private string _EmployeesKeyword;

        public string EmployeesKeyword
        {
            get => _EmployeesKeyword;
            set
            {
                if(value == _EmployeesKeyword)
                    return;
                _EmployeesKeyword = value;
                OnPropertyChanged(nameof(EmployeesKeyword));
                Employees.Filter = FilterEmployees;
            }
        }
        
        private bool FilterEmployees(object o)
        {
            var emp = !((o as Customer)?.IsStudent ?? false);
            if (!emp) return false;
            return string.IsNullOrEmpty(EmployeesKeyword) || 
                   ((o as Customer)?.Fullname.Contains(EmployeesKeyword)??false);
        }

        private ICommand _newStudentCommand;

        public ICommand NewStudentCommand => _newStudentCommand ?? (_newStudentCommand = new DelegateCommand(d =>
        {
            var stud = new Customer(){IsStudent = true};
            while (true)
            {
                
                if (new NewCustomer() {DataContext = stud, Title = "New Student"}
                        .ShowDialog() ?? false)
                {
                    if (string.IsNullOrEmpty(stud.RFID))
                    {
                        MessageBox.Show("RFID is required and must be unique.");
                        continue;
                    }
                    if (string.IsNullOrEmpty(stud.Firstname))
                    {
                        MessageBox.Show("First name is required.");
                        continue;
                    }
                    if (string.IsNullOrEmpty(stud.Lastname))
                    {
                        MessageBox.Show("Last name is required.");
                        continue;
                    }
                    if (Customer.Cache.Any(x => x.RFID == stud.RFID))
                    {
                        MessageBox.Show("RFID is already in use!");
                        continue;
                    }
                    stud.Save();
                }
                break;
            }
        }));

        private ICommand _newEmployeeCommand;

        public ICommand NewEmployeeCommand => _newEmployeeCommand ?? (_newEmployeeCommand = new DelegateCommand(d =>
        {
            var stud = new Customer() {IsStudent = false};
            while (true)
            {
                if (new NewCustomer() {DataContext = stud, Title = "New Employee"}
                        .ShowDialog() ?? false)
                {
                    if (string.IsNullOrEmpty(stud.RFID))
                    {
                        MessageBox.Show("RFID is required and must be unique.");
                        continue;
                    }
                    if (string.IsNullOrEmpty(stud.Firstname))
                    {
                        MessageBox.Show("First name is required.");
                        continue;
                    }
                    if (string.IsNullOrEmpty(stud.Lastname))
                    {
                        MessageBox.Show("Last name is required.");
                        continue;
                    }
                    if (Customer.Cache.Any(x => x.RFID == stud.RFID))
                    {
                        MessageBox.Show("RFID is already in use!");
                        continue;
                    }
                    stud.Save();
                }
                break;
            }

        }));

        private ICommand _topupCommand;

        public ICommand TopupCommand => _topupCommand ?? (_topupCommand = new DelegateCommand<Customer>(c =>
        {
            var topVm = new TopupViewModel(c);
            if ((new TopUp() {DataContext = topVm}.ShowDialog() ?? false) && topVm.Credits>0 && topVm.Customer!=null)
            {
                topVm.Customer.Update(nameof(topVm.Customer.Credits), topVm.Customer.Credits+topVm.Credits);
                CustomerLog.Add(topVm.Customer.Id,$"Added {topVm.Credits:#,##0.00} credits.");
            }
        },d=>d!=null));

        private ICommand _runExternalCommand;

        public ICommand RunExternalCommand => _runExternalCommand ?? 
                                              (_runExternalCommand = new DelegateCommand<string>(com =>Process.Start(com)));


        private ListCollectionView _Products;

        public ListCollectionView Products
        {
            get
            {
                if (_Products != null)
                    return _Products;
                _Products = new ListCollectionView(Product.Cache);
                _Products.Filter = FilterProducts;
                return _Products;
            }
        }

        private string _ProductsKeyword;

        public string ProductsKeyword
        {
            get => _ProductsKeyword;
            set
            {
                if (value == _ProductsKeyword)
                    return;
                _ProductsKeyword = value;
                OnPropertyChanged(nameof(ProductsKeyword));
                Products.Filter = FilterProducts;
            }
        }

        private bool FilterProducts(object o)
        {
            return string.IsNullOrEmpty(ProductsKeyword) ||
                   ((o as Product)?.Description.Contains(ProductsKeyword) ?? false);
        }

        private ICommand _newProductCommand;

        public ICommand NewProductCommand => _newProductCommand ?? (_newProductCommand = new DelegateCommand(d =>
        {
            var product = new Product();
            while (true)
            {
                if (new NewProduct() {DataContext = product}?.ShowDialog() ?? false)
                {
                    if (Product.Cache.Any(x => x.Code == product.Code))
                    {
                        MessageBox.Show("Product code already exists.");
                        continue;
                    }
                    if (product.Price <= 0.0)
                    {
                        MessageBox.Show("Price must be greater than zero.");
                        continue;
                    }
                    if (string.IsNullOrEmpty(product.Code))
                    {
                        MessageBox.Show("Product code is required and must be unique.");
                        continue;
                    }
                    product.Save();
                }
                break;
            }
            
        }));

        private ICommand _newStocksCOmmand;

        public ICommand AddStocksCommand => _newStocksCOmmand ?? (_newStocksCOmmand = new DelegateCommand<Product>(product =>
        {
            if (product == null) return;
            var sQty = "";
            while (true)
            {
                var dlg = new NewStocks();
                dlg.Quantity.Text = sQty;
                dlg.Quantity.SelectAll();
                dlg.Quantity.Focus();
                if (dlg.ShowDialog() ?? false)
                {
                    sQty = dlg.Quantity.Text;
                    if (!double.TryParse(sQty, out var qty))
                    {
                        MessageBox.Show("Invalid quantity.");
                        continue;
                    }
                    if (qty <= 0) return;
                    product.Update(nameof(product.Quantity), product.Quantity + qty);
                    ProductLog.Add(product.Id, $"{qty} new stocks were added.");
                }
                break;
            }
        },d=>d!=null));

        private ICommand _StartSalesCommand;

        public ICommand StartSalesCommand => _StartSalesCommand ?? (_StartSalesCommand = new DelegateCommand<Customer>(c =>
        {
            PosViewModel.Customer = c;
            SelectedTab = POS;
        },d=> !PosViewModel.IsTransactionStarted));

        private PosViewModel _PosViewModel = new PosViewModel(null);

        public PosViewModel PosViewModel
        {
            get => _PosViewModel;
            set
            {
                if(value == _PosViewModel)
                    return;
                _PosViewModel = value;
                OnPropertyChanged(nameof(PosViewModel));
            }
        }

        private ICommand _addToCartCommand;

        public ICommand AddToCartCommand => _addToCartCommand ?? (_addToCartCommand = new DelegateCommand<Product>(d =>
        {
            PosViewModel.AddProduct = d;
            PosViewModel.Quantity = 1;
            if(PosViewModel.AddProductCommand.CanExecute(null))
                PosViewModel.AddProductCommand.Execute(null);
        },d=>
        {
            if (!PosViewModel.IsTransactionStarted) return false;
            return PosViewModel.AddProductCommand.CanExecute(null);
        }));
    }
}
