using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;

namespace SFC.Canteen.ViewModels
{
    class ReportsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Report.Filter = FilterSale;
        }
        
        private ReportsViewModel() { }
        private static ReportsViewModel _instance;
        public static ReportsViewModel Instance => _instance ?? (_instance = new ReportsViewModel());

        private ListCollectionView _report;

        public ListCollectionView Report
        {
            get
            {
                if (_report != null) return _report;
                _report = new ListCollectionView(Models.Sale.Cache);
                _report.Filter = FilterSale;
                Models.Sale.Cache.CollectionChanged += (sender, args) =>
                {
                    _report.Filter = FilterSale;
                };
                return _report;
            }
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
