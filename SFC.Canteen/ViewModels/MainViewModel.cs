﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using MaterialDesignThemes.Wpf;
using SFC.Canteen.Models;
using SFC.Canteen.Properties;
using SFC.Canteen.Views;
using Xceed.Words.NET;

namespace SFC.Canteen.ViewModels
{
    class MainViewModel : MarkupExtension, INotifyPropertyChanged
    {
        public const int STUDENTS = 0, EMPLOYEES = 1, PRODUCTS=2, POS=3,REPORTS=4;

        public MainViewModel()
        {
            if(_instance!=null) throw new Exception("awooo");
            _instance = this;
            Messenger.Default.AddListener<string>(Messages.Scan, code =>
            {
                if (SelectedTab != POS) return;
                if (PosViewModel.IsTransactionStarted) return;
                Customer customer = null;
                if (code.ToLower() == Settings.Default.VisitorCard.ToLower())
                    customer = Customer.Visitor;
                else
                    customer = Customer.Cache.FirstOrDefault(x => x.RFID.ToLower() == code.ToLower());
                
                if (customer == null) return;
                PosViewModel.Customer = customer;
                PosViewModel.IsTransactionStarted = true;
            });
            
            Messenger.Default.AddListener<string>(Messages.SMS, msg =>
            {
                awooo.Context.Post(d =>
                {
                    Status = msg;
                    _statusUpdated = DateTime.Now;

                    Task.Factory.StartNew(async () =>
                    {
                        while ((DateTime.Now - _statusUpdated).TotalMilliseconds < 7777)
                            await TaskEx.Delay(111);
                        awooo.Context.Post(dd =>
                        {
                            Status = null;
                        },null);
                    });
                },null);
                
            });
        }

        private DateTime _statusUpdated = DateTime.Now;
        
        private string _Status;

        public string Status
        {
            get => _Status;
            set
            {
                if(value == _Status)
                    return;
                _Status = value;
                OnPropertyChanged(nameof(Status));
            }
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
            if ((o as Customer).RFID?.ToLower().Contains(StudentsKeyword?.ToLower()??"")??false) return true;
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
            if (!(o is Customer e)) return false;
            if (e.RFID?.ToLower().Contains(EmployeesKeyword?.ToLower() ?? "")??false) return true;
            return string.IsNullOrEmpty(EmployeesKeyword) || 
                   ((o as Customer)?.Fullname.Contains(EmployeesKeyword)??false);
        }

        private ICommand _newStudentCommand;

        public ICommand NewStudentCommand => _newStudentCommand ?? (_newStudentCommand = new DelegateCommand(d =>
        {
            var stud = new Customer()
            {
                IsStudent = true,
                Picture = ImageProcessor.Generate()
            };
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
        },d=>CurrentUser?.StudentsAdmin??false));

        private ICommand _newEmployeeCommand;

        public ICommand NewEmployeeCommand => _newEmployeeCommand ?? (_newEmployeeCommand = new DelegateCommand(d =>
        {
            var stud = new Customer()
            {
                IsStudent = false,
                Picture = ImageProcessor.Generate()
            };
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

        },d=>CurrentUser?.EmployeesAdmin??false));

        private ICommand _ShowAboutCommand;

        public ICommand ShowAboutCommand => _ShowAboutCommand ?? (_ShowAboutCommand = new DelegateCommand(d =>
        {
            var about = new About();
            about.ShowDialog();
        }));

        private ICommand _topupCommand;

        public ICommand TopupCommand => _topupCommand ?? (_topupCommand = new DelegateCommand<Customer>(c =>
        {
            var topVm = new TopupViewModel(c);
            if ((new TopUp() {DataContext = topVm}.ShowDialog() ?? false) && topVm.Credits>0 && topVm.Customer!=null)
            {
                var sale = Sale.Create(CurrentUser.Id, topVm.Customer.Id, topVm.Credits, true);
                sale.Save();
                topVm.Customer.Update(nameof(topVm.Customer.Credits), topVm.Customer.Credits+topVm.Credits);
                CustomerLog.Add(topVm.Customer.Id, CurrentUser.Id, $"Deposited Php {topVm.Credits:#,##0.00}.");

                var sms = Settings.Default.TopupMessage
                    .Replace("[AMOUNT]", sale.Amount.ToString("#,##0.00"))
                    .Replace("[CREDITS]", c.Credits.ToString("#,##0.00"))
                    .Replace("[REF]", sale.Id.ToString());

                SMS.Send(sms, c.ContactNumber);
            }
        },d=>
        {
            if (d == null) return false;

            if (d.IsStudent) return CurrentUser?.StudentsAdmin ?? false;
            if (!d.IsStudent) return CurrentUser?.EmployeesAdmin ?? false;
            
            return true;
        }));

        private ICommand _payFullCommand;

        public ICommand PayFullCommand => _payFullCommand ?? (_payFullCommand = new DelegateCommand<Customer>(d =>
        {
            CustomerLog.Add(d.Id,CurrentUser.Id, $"Full payment of {Math.Abs(d.Credits):#,##0.00}.");

            var sms = Settings.Default.FullPaymentMessage
                .Replace("[AMOUNT]", Math.Abs(d.Credits).ToString("#,##0.00"));
            
            SMS.Send(sms, d.ContactNumber);
            
           // var sale = Sale.Create(CurrentUser.Id, d.Id,Math.Abs(d.Credits), true);
           // sale.Save();
            d.Update(nameof(Customer.Credits), 0.0);
        },d=>d!=null && d.Utangan));

        private ICommand _showCustomerCommand;

        public ICommand ShowCustomerCommand => _showCustomerCommand ?? (_showCustomerCommand = new DelegateCommand<Customer>(cust =>
        {
            if (cust.IsStudent)
            {
                SelectedTab = STUDENTS;
                Students.MoveCurrentTo(cust);
            }
            else
            {
                SelectedTab = EMPLOYEES;
                Employees.MoveCurrentTo(cust);
            }
           
        }));

        private ICommand _showProductCommand;

        public ICommand ShowProductCommand => _showProductCommand ?? (_showProductCommand = new DelegateCommand<Product>(d =>
        {
            SelectedTab = PRODUCTS;
            Products.MoveCurrentTo(d);
        }));

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
            if (string.IsNullOrEmpty(ProductsKeyword)) return true;

            if (!(o is Product p)) return false;
            
            if (p.Code?.ToLower().Contains(ProductsKeyword.ToLower()) ?? false) return true;
            return p.Description?.ToLower().Contains(ProductsKeyword.ToLower()) ?? false;
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
            
        },d=>CurrentUser?.ProductsAdmin??false));

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
                    if (qty == 1)
                    {
                        ProductLog.Add(product.Id, "New item added.",CurrentUser.Id);
                    } else
                    ProductLog.Add(product.Id, $"{qty:#,##0.##} stocks were added.",CurrentUser.Id);
                }
                break;
            }
        },d=>d!=null && (CurrentUser?.ProductsAdmin??false)));

        private ICommand _StartSalesCommand;

        public ICommand StartSalesCommand => _StartSalesCommand ?? (_StartSalesCommand = new DelegateCommand<Customer>(c =>
        {
            if (c == null)
            {
                c = (Customer) (SelectedTab ==STUDENTS ? Students.CurrentItem : Employees.CurrentItem);
            }
            if (c == null) return;
            
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
            if (d == null)
            {
                if (SelectedTab != PRODUCTS) return;
                d = Products.CurrentItem as Product;
            }
            PosViewModel.AddProduct = d;
            PosViewModel.Quantity = 1;
            if(PosViewModel.AddProductCommand.CanExecute(null))
                PosViewModel.AddProductCommand.Execute(null);
            MessageBox.Show($"1 {d.Description} has been added to cart.");
        },d=>
        {
            if (!PosViewModel.IsTransactionStarted) return false;
            if (!(CurrentUser?.SalesAdmin ?? false)) return false;
            return PosViewModel.AddProductCommand.CanExecute(Products.CurrentItem);
        }));

        private ICommand _printCommand;

        public ICommand PrintCommand => _printCommand ?? (_printCommand = new DelegateCommand(d =>
        {
            if (SelectedTab == STUDENTS)
            {
                PrintCustomers(Customer.Cache.Where(x => x.IsStudent).ToList());
            } 
            else if (SelectedTab == EMPLOYEES)
            {
                PrintCustomers(Customer.Cache.Where(x => !x.IsStudent).ToList());
            }
            else if (SelectedTab == PRODUCTS)
            {
                PrintProducts();
            }
        }, d => SelectedTab != POS));

        private void PrintProducts()
        {
            if (!Directory.Exists("Temp"))
                Directory.CreateDirectory("Temp");
            
            var temp = Path.Combine("Temp", $"Products [{DateTime.Now:d-MMM-yyyy}].docx");
            using (var doc = DocX.Load($@"Templates\Products.docx"))
            {
                var tbl = doc.Tables.First();
                var items = Product.Cache.ToList();
                var total = 0.0;
                var sold = 0.0;
                var qty = 0.0;
                foreach (var item in items)
                {
                    var r = tbl.InsertRow();
                    var p = r.Cells[0].Paragraphs.First().Append(item.Code);
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.center;

                    p = r.Cells[1].Paragraphs.First().Append(item.Description);
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.left;

                    p = r.Cells[2].Paragraphs.First().Append(item.Price.ToString("#,##0.00"));
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.right;

                    qty += item.Quantity;
                    p = r.Cells[3].Paragraphs.First().Append(item.Quantity.ToString("#,##0.##"));
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.right;

                    var s1 = SaleItem.Cache
                        .Where(x => x.ProductId == item.Id)
                        .Sum(x => x.Quantity);
                    p = r.Cells[4].Paragraphs.First().Append(
                        s1.ToString("#,##0.##")
                    );
                    sold += s1;
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.right;

                    var s = SaleItem.Cache
                        .Where(x => x.ProductId == item.Id)
                        .Sum(x => x.Amount);
                    total += s;
                    p = r.Cells[5].Paragraphs.First().Append(s.ToString("#,##0.00"));
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.right;
                    
                    
                }

                var r2 = tbl.InsertRow();
                
                r2.MergeCells(0,2);

                var p2 = r2.Cells[0].Paragraphs.First().Append("TOTAL");
                p2.LineSpacingAfter = 0;
                p2.Alignment = Alignment.center;

                p2 = r2.Cells[1].Paragraphs.First().Append(qty.ToString("#,##0.##"));
                p2.LineSpacingAfter = 0;
                p2.Alignment = Alignment.right;

                p2 = r2.Cells[2].Paragraphs.First().Append(sold.ToString("#,##0.##"));
                p2.LineSpacingAfter = 0;
                p2.Alignment = Alignment.right;

                p2 = r2.Cells[3].Paragraphs.First().Append(total.ToString("#,##0.00"));
                p2.LineSpacingAfter = 0;
                p2.Alignment = Alignment.right;

                var border = new Xceed.Words.NET.Border(BorderStyle.Tcbs_single, BorderSize.one, 0,
                    System.Drawing.Color.Black);
                tbl.SetBorder(TableBorderType.Bottom, border);
                tbl.SetBorder(TableBorderType.Left, border);
                tbl.SetBorder(TableBorderType.Right, border);
                tbl.SetBorder(TableBorderType.Top, border);
                tbl.SetBorder(TableBorderType.InsideV, border);
                tbl.SetBorder(TableBorderType.InsideH, border);
                File.Delete(temp);
                doc.SaveAs(temp);
            }
            Print(temp);
        }

        private void PrintCustomers(List<Customer> customers)
        {
            if(!Directory.Exists("Temp"))
                Directory.CreateDirectory("Temp");
            
            var students = customers.FirstOrDefault()?.IsStudent ?? false ? "Students" : "Employees";
            
            var temp = Path.Combine("Temp", $"{students} [{DateTime.Now:d-MMM-yyyy}].docx");
            using(var doc = DocX.Load($@"Templates\{students}.docx"))
            {
                var tbl = doc.Tables.First();

                foreach(var item in customers)
                {
                    var r = tbl.InsertRow();
                    var p = r.Cells[0].Paragraphs.First().Append(item.RFID);
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.center;

                    p = r.Cells[1].Paragraphs.First().Append(item.Fullname);
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.left;

                    r.Cells[2].Paragraphs.First()
                        .Append(item.Course)
                        .LineSpacingAfter = 0;

                    p = r.Cells[3].Paragraphs.First().Append(item.Credits.ToString("#,##0.00"));
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.right;
                }
                var border = new Xceed.Words.NET.Border(BorderStyle.Tcbs_single, BorderSize.one, 0, System.Drawing.Color.Black);
                tbl.SetBorder(TableBorderType.Bottom, border);
                tbl.SetBorder(TableBorderType.Left, border);
                tbl.SetBorder(TableBorderType.Right, border);
                tbl.SetBorder(TableBorderType.Top, border);
                tbl.SetBorder(TableBorderType.InsideV, border);
                tbl.SetBorder(TableBorderType.InsideH, border);
                File.Delete(temp);
                doc.SaveAs(temp);
            }
            Print(temp);
        }
        
        internal static void Print(string path)
        {
            var info = new ProcessStartInfo(path);
            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = true;
            info.Verb = "PrintTo";
            Process.Start(info);
        }

        private ICommand _printProductLogCommand;

        public ICommand PrintProductLogCommand =>
            _printProductLogCommand ?? (_printProductLogCommand = new DelegateCommand<Product>(
                d =>
                {
                    var summary = new LogSummary()
                    {
                        R_NAME = "Stocks",
                        R_VALUE = d.Quantity.ToString("0.00"),
                        SUM_1 = "Quantity Sold",
                        SUM_2 = "Total Sales",
                        SUM_3 = "Transactions",
                        VALUE_1 = SaleItem.Cache.Where(x => x.ProductId == d.Id).Sum(x => x.Quantity).ToString("0"),
                        VALUE_2 = SaleItem.Cache.Where(x => x.ProductId == d.Id).Sum(x => x.Amount).ToString("0.00"),
                        VALUE_3 = SaleItem.Cache.Count(x => x.ProductId == d.Id).ToString("0")
                    };
                    
                    PrintLog(d.Code, d.Description, d.Logs.Cast<ILog>(),summary);
                },d=> d!=null && d.Logs.Count>0));

        private ICommand _printLogCommand;

        public ICommand PrintLogCommand => _printLogCommand ?? (_printLogCommand = new DelegateCommand<Customer>(d =>
        {
            
            var summary = new LogSummary()
            {
                R_NAME = "Credits",
                R_VALUE = d.Credits.ToString("0.00"),
                SUM_1 = "Total Deposits",
                SUM_2 = "Total Sales",
                SUM_3 = "Transactions",
                VALUE_1 = Sale.Cache.Where(x=>x.CustomerId == d.Id && x.Topup).Sum(x=>x.Amount).ToString("0.00"),
                VALUE_2 = Sale.Cache.Where(x => x.CustomerId == d.Id && !x.Topup).Sum(x => x.Amount).ToString("0.00"),
                VALUE_3 = Sale.Cache.Count(x=>x.CustomerId == d.Id).ToString("0")
            };
            PrintLog(d.RFID,d.Fullname,d.Logs.Cast<ILog>(),summary);
        },d=>d!=null && d?.Logs.Count>0));

        class LogSummary
        {
            public string R_NAME { get; set; } = "";
            public string R_VALUE { get; set; } = "";
            public string SUM_1 { get; set; } = "";
            public string SUM_2 { get; set; } = "";
            public string SUM_3 { get; set; } = "";
            public string VALUE_1 { get; set; } = "";
            public string VALUE_2 { get; set; } = "";
            public string VALUE_3 { get; set; } = "";
        }
        
        private void PrintLog(string id, string name, IEnumerable<ILog> logs, LogSummary summary)
        {
            if (!Directory.Exists("Temp"))
                Directory.CreateDirectory("Temp");

            var temp = Path.Combine("Temp", $"Activity Log [{id}].docx");
            using (var doc = DocX.Load($@"Templates\CustomerLog.docx"))
            {
                doc.ReplaceText("[RFID]", id);
                doc.ReplaceText("[NAME]", name);
                doc.ReplaceText("[R_NAME]",summary.R_NAME);
                doc.ReplaceText("[R_VALUE]", summary.R_VALUE);
                doc.ReplaceText("[SUM_1]", summary.SUM_1);
                doc.ReplaceText("[SUM_2]", summary.SUM_2);
                doc.ReplaceText("[SUM_3]", summary.SUM_3);
                doc.ReplaceText("[VALUE_1]", summary.VALUE_1);
                doc.ReplaceText("[VALUE_2]", summary.VALUE_2);
                doc.ReplaceText("[VALUE_3]", summary.VALUE_3);

                var tbl = doc.Tables.First();

                foreach (var item in logs)
                {
                    var r = tbl.InsertRow();
                    var p = r.Cells[0].Paragraphs.First().Append(item.Time.ToString("g"));
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.center;

                    p = r.Cells[1].Paragraphs.First().Append(item.Message);
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.left;

                    p = r.Cells[2].Paragraphs.First().Append(item.User?.Name);
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.center;

                }
                var border = new Xceed.Words.NET.Border(BorderStyle.Tcbs_single, BorderSize.one, 0,
                    System.Drawing.Color.Black);
                tbl.SetBorder(TableBorderType.Bottom, border);
                tbl.SetBorder(TableBorderType.Left, border);
                tbl.SetBorder(TableBorderType.Right, border);
                tbl.SetBorder(TableBorderType.Top, border);
                tbl.SetBorder(TableBorderType.InsideV, border);
                tbl.SetBorder(TableBorderType.InsideH, border);
                File.Delete(temp);
                doc.SaveAs(temp);
            }
            Print(temp);
        }

        private User _CurrentUser;

        public User CurrentUser
        {
            get => _CurrentUser;
            set
            {
                if(value == _CurrentUser)
                    return;
                _CurrentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }

        public void Login(string username, string password)
        {
            var user = User.Cache.FirstOrDefault(x => x.Username.ToLower() == username.ToLower());
            if (user == null && User.Cache.Count == 0)
            {
                user = new User()
                {
                    Username = username,
                    Password = password,
                    StudentsAdmin = true,
                    EmployeesAdmin = true,
                    ProductsAdmin = true,
                    UsersAdmin = true,
                    SalesAdmin = true,
                };
                user.Save();
            }
            if (user == null)
            {
                MessageBox.Show("Invalid username/password!");
                return;
            }
            if(string.IsNullOrEmpty(user.Password)) user.Update(nameof(User.Password),password);
            if (user.Password!=password)
            {
                MessageBox.Show("Invalid username/password!");
                return;
            }
            CurrentUser = user;
            Session = new Session(){UserId = user.Id};
            Session.Save();
        }

        public static Session Session { get; private set; }


        private ICommand _logoutCommand;

        public ICommand LogoutCommand => _logoutCommand ?? (_logoutCommand = new DelegateCommand(d =>
        {
            Session?.Update(nameof(Session.TimeOut),DateTime.Now);
            CurrentUser = null;
            ((MainWindow) Application.Current.MainWindow).ShowLogin();
        }));

        private ICommand _changePasswordCommand;

        public ICommand ChangePasswordCommand =>
            _changePasswordCommand ?? (_changePasswordCommand = new DelegateCommand(
                d =>
                {
                    new ChangePasswordView().ShowDialog();
                },d=>CurrentUser!=null));

        private ICommand _showUsersCommand;

        public ICommand ShowUsersCommand => _showUsersCommand ?? (_showUsersCommand = new DelegateCommand(d =>
        {
            ShowOptions(1);
        },d=>CurrentUser?.UsersAdmin??false));

        private void ShowOptions(int tab)
        {
            var dlg = new OptionsView();
            dlg.Tab.SelectedIndex = tab;
            dlg.ShowDialog();
        }

        private ICommand _optionsCommand;

        public ICommand OptionsCommand => _optionsCommand ?? (_optionsCommand = new DelegateCommand(d =>
        {
            ShowOptions(0);
        }));

        private ICommand _addUserCommand;

        public ICommand AddUserCommand => _addUserCommand ?? (_addUserCommand = new DelegateCommand(d =>
        {
            User.Cache.Add(new User(){Username = "NEW USER"});
        },d=>CurrentUser?.UsersAdmin??false));
    }
}
