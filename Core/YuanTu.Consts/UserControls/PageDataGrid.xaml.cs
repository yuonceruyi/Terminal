using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace YuanTu.Consts.UserControls
{
    /// <summary>
    ///     PageDataGrid.xaml 的交互逻辑
    /// </summary>
    public partial class PageDataGrid : UserControl
    {
        public static readonly DependencyProperty BillCountProperty = DependencyProperty.Register(
            nameof(BillCount), typeof(string), typeof(PageDataGrid));

        public static readonly DependencyProperty TotalAmountProperty = DependencyProperty.Register(
            nameof(TotalAmount), typeof(string), typeof(PageDataGrid));

        public static readonly DependencyProperty SummaryCountProperty = DependencyProperty.Register(
            nameof(SummaryCount), typeof(int), typeof(PageDataGrid), new PropertyMetadata(3));

        public static readonly DependencyProperty CurrentDataProperty =
            DependencyProperty.Register(
                "CurrentData",
                typeof(PageData),
                typeof(PageDataGrid),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register(nameof(ItemSource),
            typeof(IEnumerable<PageData>), typeof(PageDataGrid),
            new PropertyMetadata(default(IEnumerable<PageData>), (a, b) =>
            {
                var ctx = a as PageDataGrid;
                ctx._summaryindex = 0;
                ctx.UpdateSummary();
            }));

        public static readonly DependencyProperty SummaryInfoProperty = DependencyProperty.Register(
            "SummaryInfo", typeof(object), typeof(PageDataGrid), new PropertyMetadata(default(object), (d, a) =>
            {
                var pd = d as PageDataGrid;
                pd.SummaryDetail.Content = a.NewValue;
            }));

        public static readonly DependencyProperty DetialRowsProperty =
            DependencyProperty.Register(nameof(DetialRowCount), typeof(int), typeof(PageDataGrid),
                new PropertyMetadata(6));

        private int _detailcount;

        private int _detialindex;
        private int _summarycount;

        private int _summaryindex;

        public PageDataGrid()
        {
            InitializeComponent();
        }

        public string BillCount
        {
            get { return (string)GetValue(BillCountProperty); }
            set { SetValue(BillCountProperty, value); }
        }

        public string TotalAmount
        {
            get { return (string)GetValue(TotalAmountProperty); }
            set { SetValue(TotalAmountProperty, value); }
        }

        public PageData CurrentData
        {
            get { return (PageData)GetValue(CurrentDataProperty); }
            set { SetValue(CurrentDataProperty, value); }
        }

        public int SummaryCount
        {
            get { return (int)GetValue(SummaryCountProperty); }
            set { SetValue(SummaryCountProperty, value); }
        }

        public IEnumerable<PageData> ItemSource
        {
            get { return (IEnumerable<PageData>)GetValue(ItemSourceProperty); }
            set
            {
                SetValue(ItemSourceProperty, value);
                CatalogList.ItemsSource = value;
            }
        }

        public object SummaryInfo
        {
            get { return (FrameworkElement)GetValue(SummaryInfoProperty); }
            set { SetValue(SummaryInfoProperty, value); }
        }

        public ObservableCollection<DataGridColumn> Columns
        {
            get { return DetailGrid.Columns; }
        }

        public int DetialRowCount
        {
            get { return (int)GetValue(DetialRowsProperty); }
            set { SetValue(DetialRowsProperty, value); }
        }

        private void UpdateSummary()
        {
            var lst = ItemSource;
            _summarycount = (lst.Count() + SummaryCount - 1) / SummaryCount;
            var dta =
                lst.Skip(_summaryindex * SummaryCount)
                    .Take(Math.Min(SummaryCount, lst.Count() - _summaryindex * SummaryCount));
            CatalogList.ItemsSource = dta;
            CatalogList.UpdateLayout();
            ClickCatalogButton(0);

            ButtonSummaryPreview.Visibility = _summaryindex < 1 ? Visibility.Hidden : Visibility.Visible;
            ButtonSummaryNext.Visibility = _summaryindex >= _summarycount - 1 ? Visibility.Hidden : Visibility.Visible;
        }

        private void ButtonSummaryPreview_Click(object sender, RoutedEventArgs e)
        {
            if (_summaryindex < 1)
            {
                return;
            }
            _summaryindex--;
            UpdateSummary();
        }

        private void ButtonSummaryNext_Click(object sender, RoutedEventArgs e)
        {
            if (_summaryindex >= _summarycount - 1)
            {
                return;
            }
            _summaryindex++;
            UpdateSummary();
        }

        private void UpdateDetails()
        {
            var lst = CurrentData?.List?.OfType<object>()?.ToArray() ?? new object[0];
            _detailcount = (lst.Length + DetialRowCount - 1) / DetialRowCount;
            var dta =
                lst.Skip(_detialindex * DetialRowCount)
                    .Take(Math.Min(DetialRowCount, lst.Length - _detialindex * DetialRowCount));
            DetailGrid.ItemsSource = dta;
            LblPage.Text = $"{_detialindex + 1}/{_detailcount}";
            ButtonDetailPreview.Visibility = _detialindex < 1 ? Visibility.Hidden : Visibility.Visible;
            ButtonDetailNext.Visibility = _detialindex >= _detailcount - 1 ? Visibility.Hidden : Visibility.Visible;
        }

        private void ButtonDetailPreview_Click(object sender, RoutedEventArgs e)
        {
            if (_detialindex < 1)
            {
                return;
            }
            _detialindex--;
            UpdateDetails();
        }

        private void ButtonDetailNext_Click(object sender, RoutedEventArgs e)
        {
            if (_detialindex >= _detailcount - 1)
            {
                return;
            }
            _detialindex++;
            UpdateDetails();
        }

        /// <summary>
        ///     得到指定元素的集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depObj"></param>
        /// <returns></returns>
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj != null)
            {
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (var childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void PageDataGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            ClickCatalogButton(0);
        }

        private void ClickCatalogButton(int index)
        {
            var btns = FindVisualChildren<Button>(CatalogList).ToArray();
            if (btns.Length <= index)
            {
                return;
            }
            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice,
                0, MouseButton.Left);
            args.RoutedEvent = ButtonBase.ClickEvent;
            btns[index].RaiseEvent(args);
        }

        private void ButtonCatalog_OnClick(object sender, RoutedEventArgs e)
        {
            CurrentData = (sender as Button)?.DataContext as PageData;
            _detialindex = 0;
            UpdateDetails();
        }
    }

    public class PageData
    {
        public string CatalogContent { get; set; }
        public IEnumerable List { get; set; }
        public object Tag { get; set; }
    }

    public class SamePageConverter : IMultiValueConverter
    {
        #region Implementation of IMultiValueConverter

        /// <summary>
        ///     将源值转换为绑定源的值。 数据绑定引擎在将值从绑定源传播给绑定目标时，调用此方法。
        /// </summary>
        /// <returns>
        ///     转换后的值。 如果该方法返回 null，则使用有效的 null 值。 <see cref="T:System.Windows.DependencyProperty" />.
        ///     <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> 的返回值表示转换器没有生成任何值，且绑定将使用
        ///     <see cref="P:System.Windows.Data.BindingBase.FallbackValue" />（如果可用），否则将使用默认值。
        ///     <see cref="T:System.Windows.Data.Binding" />.<see cref="F:System.Windows.Data.Binding.DoNothing" />
        ///     的返回值表示绑定不传输值，或不使用 <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> 或默认值。
        /// </returns>
        /// <param name="values">
        ///     <see cref="T:System.Windows.Data.MultiBinding" /> 中源绑定生成的值的数组。 值
        ///     <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> 表示源绑定没有要提供以进行转换的值。
        /// </param>
        /// <param name="targetType">绑定目标属性的类型。</param>
        /// <param name="parameter">要使用的转换器参数。</param>
        /// <param name="culture">要用在转换器中的区域性。</param>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values?.Length > 1 && values[0] == values[1];
        }

        /// <summary>
        ///     将绑定目标值转换为源绑定值。
        /// </summary>
        /// <returns>
        ///     从目标值转换回源值的值的数组。
        /// </returns>
        /// <param name="value">绑定目标生成的值。</param>
        /// <param name="targetTypes">要转换到的类型数组。 数组长度指示为要返回的方法所建议的值的数量与类型。</param>
        /// <param name="parameter">要使用的转换器参数。</param>
        /// <param name="culture">要用在转换器中的区域性。</param>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion Implementation of IMultiValueConverter
    }
}