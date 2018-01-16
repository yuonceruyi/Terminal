using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace YuanTu.Consts.UserControls
{
    /// <summary>
    ///     DatePicker.xaml 的交互逻辑
    /// </summary>
    public partial class DatePicker : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<object> _days;
        private ObservableCollection<object> _months;

        private ObservableCollection<object> _years;

        public DatePicker()
        {
            InitializeComponent();
            var today = DateTime.Today;
            Years = new ObservableCollection<object>(Enumerable.Range(today.Year - 200 + 1, 200).Reverse().OfType<object>());
            Months = new ObservableCollection<object>(Enumerable.Range(1, 12).OfType<object>());
            Days = new ObservableCollection<object>(Enumerable.Range(1, DaysInMonth(today)).OfType<object>());
        }

        public ObservableCollection<object> Years
        {
            get { return _years; }
            protected set
            {
                _years = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<object> Months
        {
            get { return _months; }
            protected set
            {
                _months = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<object> Days
        {
            get { return _days; }
            protected set
            {
                _days = value;
                OnPropertyChanged();
            }
        }

        protected int Year
        {
            get { return (int)YearPicker.SelectedItem; }
            set { YearPicker.SelectedItem = value; }
        }

        protected int Month
        {
            get { return (int)MonthPicker.SelectedItem; }
            set { MonthPicker.SelectedItem = value; }
        }

        protected int Day
        {
            get { return (int)DayPicker.SelectedItem; }
            set { DayPicker.SelectedItem = value; }
        }

        private int DaysInMonth(DateTime dateTime)
        {
            return (dateTime.AddMonths(1) - dateTime).Days;
        }

        private void YearPicker_OnSelectedItemChanged(object sender, ValueChangedEventArgs e)
        {
            OnSelectedDateChanged();
        }

        private void MonthPicker_OnSelectedItemChanged(object sender, ValueChangedEventArgs e)
        {
            var daysInMonth = DaysInMonth(new DateTime(Year, Month, 1));
            while (Days.Count > daysInMonth)
                Days.RemoveAt(Days.Count - 1);
            while (Days.Count < daysInMonth)
                Days.Add(Days.Count + 1);
            if (DayPicker.SelectedItem != null && Day > daysInMonth)
                Day = daysInMonth;
            OnSelectedDateChanged();
        }

        private void DayPicker_OnSelectedItemChanged(object sender, ValueChangedEventArgs e)
        {
            OnSelectedDateChanged();
        }

        private void OnSelectedDateChanged()
        {
            if (_preventReentry)
                return;
            if (YearPicker.SelectedItem == null || MonthPicker.SelectedItem == null || DayPicker.SelectedItem == null)
                return;
            SelectedDate = new DateTime(Year, Month, Day);
        }

        #region ItemTemplate

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
                nameof(ItemTemplate),
                typeof(DataTemplate),
                typeof(DatePicker),
                new FrameworkPropertyMetadata(null)
                );

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        #endregion ItemTemplate

        #region SelectedDate

        public static readonly RoutedEvent SelectedDateChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(SelectedDateChanged), RoutingStrategy.Bubble,
                typeof(ValueChangedEventHandler),
                typeof(ScrollPicker));

        public event ValueChangedEventHandler SelectedDateChanged
        {
            add { AddHandler(SelectedDateChangedEvent, value); }
            remove { RemoveHandler(SelectedDateChangedEvent, value); }
        }

        public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register(
            nameof(SelectedDate), typeof(DateTime), typeof(DatePicker),
            new FrameworkPropertyMetadata(default(DateTime), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedDateChangedCallback));

        private bool _preventReentry;

        private static void SelectedDateChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (DatePicker)d;
            picker._preventReentry = true;
            var date = (DateTime)e.NewValue;
            picker.Year = date.Year;
            picker.Month = date.Month;
            picker.Day = date.Day;
            picker._preventReentry = false;
            var ee = new ValueChangedEventArgs(e.OldValue, e.NewValue)
            {
                RoutedEvent = SelectedDateChangedEvent,
                Source = picker
            };
            picker.OnSelectedDateChanged(ee);
        }

        protected virtual void OnSelectedDateChanged(ValueChangedEventArgs e)
        {
            RaiseEvent(e);
        }

        public DateTime SelectedDate
        {
            get { return (DateTime)GetValue(SelectedDateProperty); }
            set { SetValue(SelectedDateProperty, value); }
        }

        #endregion SelectedDate

        #region Exponent

        public static readonly DependencyProperty ExponentProperty = DependencyProperty.Register(
            nameof(Exponent), typeof(double), typeof(DatePicker), new PropertyMetadata(1.0,OnExponentChanged));

        private static void OnExponentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (DatePicker)d;
            var value = (double)e.NewValue;
            picker.YearPicker.Exponent = value;
            picker.MonthPicker.Exponent = value;
            picker.DayPicker.Exponent = value;
        }

        public double Exponent
        {
            get { return (double)GetValue(ExponentProperty); }
            set { SetValue(ExponentProperty, value); }
        }

        #endregion Exponent

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged
    }
}