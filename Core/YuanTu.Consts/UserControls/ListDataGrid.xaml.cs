using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Prism.Commands;
using YuanTu.Consts.Annotations;
using System.Collections.ObjectModel;

namespace YuanTu.Consts.UserControls
{
    /// <summary>
    /// DataGridTest.xaml 的交互逻辑
    /// </summary>
    public partial class ListDataGrid : UserControl
    {
        public ListDataGrid()
        {
            InitializeComponent();
        }

        public class PageDataEx
        {
            public string CatalogContent { get; set; }
            public IEnumerable List { get; set; }
            public object Tag { get; set; }
        }

        private int _detialindex = 0;
        private int _detailcount = 0;

        public static readonly DependencyProperty DetialRowsProperty =
            DependencyProperty.Register(nameof(DetialRowCount), typeof (int), typeof (ListDataGrid),
                new PropertyMetadata(6));

        public int DetialRowCount
        {
            get { return (int) GetValue(DetialRowsProperty); }
            set { SetValue(DetialRowsProperty, value); }
        }


       

        public double DetailContentHeight
        {
            get { return (double)DetailGrid.Height; }
            set { DetailGrid.Height=value; }
        }

        public PageDataEx CurrentData
        {
            get { return (PageDataEx) GetValue(CurrentDataProperty); }
            set
            {
                SetValue(CurrentDataProperty, value);
                UpdateDetails();
            }
        }

        public static readonly DependencyProperty CurrentDataProperty =
            DependencyProperty.Register(
                nameof(CurrentData),
                typeof (PageDataEx),
                typeof (ListDataGrid),
                new FrameworkPropertyMetadata(default(PageDataEx), (a, b) => {
                    var ctx = (a as ListDataGrid);
                       ctx._detialindex = 0;
                      ctx?.UpdateDetails();
                }) );




       


        private void UpdateDetails()
        {
            var lst = CurrentData?.List?.OfType<object>()?.ToArray()??new object[0];
            _detailcount = (lst.Length + DetialRowCount - 1) / DetialRowCount;
            var dta = lst.Skip(_detialindex * DetialRowCount).Take(Math.Min(DetialRowCount, lst.Length - _detialindex * DetialRowCount));
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
        public ObservableCollection<DataGridColumn> Columns
        {
            get { return DetailGrid.Columns; }
        }

    }
    
}
