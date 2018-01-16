using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace YuanTu.Consts.UserControls
{
    /// <summary>
    ///     PageDataFree.xaml 的交互逻辑
    /// </summary>
    public partial class PageDataFree : UserControl
    {
        public static readonly DependencyProperty SummaryCountProperty = DependencyProperty.Register(
            nameof(SummaryCount), typeof(int), typeof(PageDataFree), new PropertyMetadata(3));

        public static readonly DependencyProperty CurrentDataProperty =
            DependencyProperty.Register(
                "CurrentData",
                typeof(PageData),
                typeof(PageDataFree),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register(nameof(ItemSource),
            typeof(IEnumerable<PageData>), typeof(PageDataFree),
            new PropertyMetadata(default(IEnumerable<PageData>), (a, b) =>
            {
                var ctx = a as PageDataFree;
                ctx._summaryindex = 0;
                ctx.UpdateSummary();
            }));

        public static readonly DependencyProperty SummaryInfoProperty = DependencyProperty.Register(
            "SummaryInfo", typeof(object), typeof(PageDataFree), new PropertyMetadata(default(object), (d, a) =>
            {
                var pd = d as PageDataFree;
                pd.SummaryDetail.Content = a.NewValue;
            }));

        public static readonly DependencyProperty GridColumnWidthProperty =
            DependencyProperty.Register(nameof(GridColumnWidth), typeof(GridLength[]), typeof(PageDataFree),
                new PropertyMetadata(default(object), (d, a) =>
                {
                    var pd = d as PageDataFree;
                    pd.GridColumnWidth = a.NewValue as GridLength[];
                }));

        public static readonly DependencyProperty GridRowHeightProperty =
            DependencyProperty.Register(nameof(GridRowHeight), typeof(GridLength[]), typeof(PageDataFree),
                new PropertyMetadata(default(object), (d, a) =>
                {
                    var pd = d as PageDataFree;
                    pd.GridRowHeight = a.NewValue as GridLength[];
                }));

        private int _detailcount;

        private int _detialindex;
        private int _summarycount;

        private int _summaryindex;

        public PageDataFree()
        {
            InitializeComponent();
        }

        public PageData CurrentData
        {
            get { return (PageData) GetValue(CurrentDataProperty); }
            set { SetValue(CurrentDataProperty, value); }
        }

        public int SummaryCount
        {
            get { return (int) GetValue(SummaryCountProperty); }
            set { SetValue(SummaryCountProperty, value); }
        }

        public IEnumerable<PageData> ItemSource
        {
            get { return (IEnumerable<PageData>) GetValue(ItemSourceProperty); }
            set
            {
                SetValue(ItemSourceProperty, value);
                CatalogList.ItemsSource = value;
            }
        }

        public object SummaryInfo
        {
            get { return (FrameworkElement) GetValue(SummaryInfoProperty); }
            set { SetValue(SummaryInfoProperty, value); }
        }

        public GridLength[] GridRowHeight
        {
            get { return (GridLength[]) GetValue(GridRowHeightProperty); }
            set
            {
                SetValue(GridRowHeightProperty, value);
                SetRow();
            }
        }

        public GridLength[] GridColumnWidth
        {
            get { return (GridLength[]) GetValue(GridColumnWidthProperty); }
            set
            {
                SetValue(GridColumnWidthProperty, value);
                SetColumn();
            }
        }

        private void SetRow()
        {
            DetailGrid.RowDefinitions.Clear();
            for (var i = 0; i < GridRowHeight.Length; i++)
            {
                var rowDefinition = new RowDefinition();
                rowDefinition.Height = GridRowHeight[i];
                DetailGrid.RowDefinitions.Add(rowDefinition);
            }
        }

        private void SetColumn()
        {
            DetailGrid.ColumnDefinitions.Clear();
            for (var i = 0; i < GridColumnWidth.Length; i++)
            {
                var columnDefinition = new ColumnDefinition();
                columnDefinition.Width = GridColumnWidth[i];
                DetailGrid.ColumnDefinitions.Add(columnDefinition);
            }
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
                return;
            _summaryindex--;
            UpdateSummary();
        }

        private void ButtonSummaryNext_Click(object sender, RoutedEventArgs e)
        {
            if (_summaryindex >= _summarycount - 1)
                return;
            _summaryindex++;
            UpdateSummary();
        }

        private void UpdateDetails()
        {
            var lst = CurrentData?.List?.OfType<object>()?.ToArray();
            if (lst != null)
            {
                _detailcount = lst.Length;
                var dta = lst[_detialindex];
                var T = dta.GetType();
                var propertys = T.GetProperties();

                for (var i = 0; i < GridRowHeight.Length; i++)
                {
                    var pro = propertys[i];
                    var txt = new TextBox {Text = pro.Name + ":"};
                    Grid.SetColumn(txt, 0);
                    Grid.SetRow(txt, i);
                    txt.TextAlignment = TextAlignment.Right;
                    txt.BorderThickness = new Thickness(0);
                    DetailGrid.Children.Add(txt);

                    var rTxt = new RichTextBox();
                    var value = pro.GetValue(dta);
                    rTxt.AppendText(value?.ToString());
                    rTxt.BorderThickness = new Thickness(0);
                    Grid.SetColumn(rTxt, 1);
                    Grid.SetRow(rTxt, i);
                    DetailGrid.Children.Add(rTxt);
                }
                LblPage.Text = $"{_detialindex + 1}/{_detailcount}";
            }
            ButtonDetailPreview.Visibility = _detialindex < 1 ? Visibility.Hidden : Visibility.Visible;
            ButtonDetailNext.Visibility = _detialindex >= _detailcount - 1 ? Visibility.Hidden : Visibility.Visible;
        }

        private void ButtonDetailPreview_Click(object sender, RoutedEventArgs e)
        {
            if (_detialindex < 1)
                return;
            _detialindex--;
            UpdateDetails();
        }

        private void ButtonDetailNext_Click(object sender, RoutedEventArgs e)
        {
            if (_detialindex >= _detailcount - 1)
                return;
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
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                        yield return (T) child;

                    foreach (var childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
        }

        private void PageDataFree_OnLoaded(object sender, RoutedEventArgs e)
        {
            ClickCatalogButton(0);
        }

        private void ClickCatalogButton(int index)
        {
            var btns = FindVisualChildren<Button>(CatalogList).ToArray();
            if (btns.Length <= index)
                return;
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
}