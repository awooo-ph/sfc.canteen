using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using SFC.Canteen.Models;
using Xceed.Words.NET;

namespace SFC.Canteen.ViewModels
{
    class ReportsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
           
        }
        
        private ReportsViewModel() { }
        private static ReportsViewModel _instance;
        public static ReportsViewModel Instance => _instance ?? (_instance = new ReportsViewModel());

        private ListCollectionView _report;
        private ObservableCollection<Sale> _sales = new ObservableCollection<Sale>();
        public ListCollectionView Report => _report ?? (_report = new ListCollectionView(_sales));

        private void RefreshFilter()
        {
            
            var items = Sale.Where(IncludeSales, IncludeTopup, DateStart, DateEnd);
            var list = _sales.ToList();
         
                foreach (var sale in list)
                {
                    if (items.All(x => x.Id != sale.Id))
                        _sales.Remove(sale);
                }
           
                list = _sales.ToList();
                foreach (var item in items)
                {
                    if (list.Any(x => x.Id == item.Id)) continue;
                     _sales.Add(item);
                }
                
           // Report.Refresh();
            // OnPropertyChanged(nameof(Report));
        }

        private bool FilterSale(object o)
        {
            var sale = o as Models.Sale;
            if (sale == null) return false;
            if (IncludeSales && !IncludeTopup && sale.Topup) return false;
            if (IncludeTopup && !IncludeSales && !sale.Topup) return false;

            if (sale.Time.Date < DateStart.Date) return false;
            if (sale.Time.Date > DateEnd.Date) return false;
            return true;
        }


        private bool _IncludeTopup;

        public bool IncludeTopup
        {
            get => _IncludeTopup;
            set
            {
                if(value == _IncludeTopup)
                    return;
                _IncludeTopup = value;
                OnPropertyChanged(nameof(IncludeTopup));
                RefreshFilter();
            }
        }

        private bool _IncludeSales;

        public bool IncludeSales
        {
            get => _IncludeSales;
            set
            {
                if(value == _IncludeSales)
                    return;
                _IncludeSales = value;
                OnPropertyChanged(nameof(IncludeSales));
                RefreshFilter();
            }
        }

        private DateTime _DateStart = DateTime.Now.AddDays(-1);

        public DateTime DateStart
        {
            get => _DateStart;
            set
            {
                if(value == _DateStart)
                    return;
                _DateStart = value;
                OnPropertyChanged(nameof(DateStart));
                RefreshFilter();
            }
        }

        private DateTime _DateEnd = DateTime.Now;

        public DateTime DateEnd
        {
            get => _DateEnd;
            set
            {
                if(value == _DateEnd)
                    return;
                _DateEnd = value;
                OnPropertyChanged(nameof(DateEnd));
                RefreshFilter();
            }
        }

        private ICommand _toggleDetailsCommand;

        public ICommand ToggleDetailsCommand => _toggleDetailsCommand ?? (_toggleDetailsCommand = new DelegateCommand(
                                                    d =>
                                                    {
                                                        ShowDetails = !ShowDetails;
                                                    }));

        private ICommand _toggleSummaryCommand;

        public ICommand ToggleSummaryCommand => _toggleSummaryCommand ?? (_toggleSummaryCommand = new DelegateCommand(
                                                    d =>
                                                    {
                                                        ShowSummary = !ShowSummary;
                                                    }));

        private bool _ShowDetails;

        public bool ShowDetails
        {
            get => _ShowDetails;
            set
            {
                if(value == _ShowDetails)
                    return;
                _ShowDetails = value;
                OnPropertyChanged(nameof(ShowDetails));
                RefreshSummary();
            }
        }

        private bool _ShowSummary;

        public bool ShowSummary
        {
            get => _ShowSummary;
            set
            {
                if(value == _ShowSummary)
                    return;
                _ShowSummary = value;
                OnPropertyChanged(nameof(ShowSummary));
                RefreshSummary();
            }
        }

        private int _DetailsRowSpan;

        public int DetailsRowSpan
        {
            get => _DetailsRowSpan;
            set
            {
                if(value == _DetailsRowSpan)
                    return;
                _DetailsRowSpan = value;
                OnPropertyChanged(nameof(DetailsRowSpan));
                RefreshSummary();
            }
        }

        private int _SummaryRow;

        public int SummaryRow
        {
            get => _SummaryRow;
            set
            {
                if(value == _SummaryRow)
                    return;
                _SummaryRow = value;
                OnPropertyChanged(nameof(SummaryRow));
                RefreshSummary();
            }
        }

        private ICommand _printCommand;
        public ICommand PrintCommand => _printCommand??(_printCommand = new DelegateCommand(d =>
        {
            if (!Directory.Exists("Temp"))
                Directory.CreateDirectory("Temp");

            var temp = Path.Combine("Temp", $"Sales Report [{DateTime.Now:yy-MMM-dd}].docx");
            using (var doc = DocX.Load($@"Templates\SalesReport.docx"))
            {
                var tbl = doc.Tables.First();
                var total = 0.0;
                foreach (Models.Sale item in Report)
                {
                    total += item.Amount;
                    var r = tbl.InsertRow();
                    var top = item.Topup ? "T" : "S";
                    var p = r.Cells[0].Paragraphs.First().Append($"{top}{item.Id:0000}");
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.center;

                    p = r.Cells[1].Paragraphs.First().Append(item.Time.ToString("g"));
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.left;

                    p = r.Cells[2].Paragraphs.First().Append(item.Customer.Fullname);
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.left;

                    p = r.Cells[3].Paragraphs.First().Append(item.Amount.ToString("#,##0.00"));
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.right;
                }
                
                doc.ReplaceText("[TOTAL_AMOUNT]",total.ToString("#,##0.00"));
                
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
            MainViewModel.Print(temp);
        }));
        
        

        private void RefreshSummary()
        {
            if (ShowDetails && ShowSummary)
            {
                SummaryRow = 1;
                DetailsRowSpan = 1;
            } else if (ShowDetails && !ShowSummary)
            {
                DetailsRowSpan = 2;
            } else if (!ShowDetails && ShowSummary)
            {
                SummaryRow = 0;
            }
        }
    }
}
