using System;
using System.Collections.Generic;
using System.Linq;
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

namespace YuanTu.Default.Part.ViewModels.AdminSub
{
    /// <summary>
    /// GridUnlock.xaml 的交互逻辑
    /// </summary>
    public partial class GridUnlock : UserControl
    {
        /// <summary>
        /// 线条的颜色
        /// </summary>
        private static readonly SolidColorBrush LineColorBrush = new SolidColorBrush(Color.FromRgb(22, 114, 186)) { Opacity = 0.8 };
      

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
            "Password", typeof(string), typeof(GridUnlock), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        ///// <summary>
        ///// 九宫格密码
        ///// </summary>
        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        private string _innerPwd;

        public static readonly DependencyProperty PasswordFinishProperty = DependencyProperty.Register(
            "PasswordFinish", typeof (ICommand), typeof (GridUnlock), new PropertyMetadata(default(ICommand)));

        public ICommand PasswordFinish
        {
            get { return (ICommand) GetValue(PasswordFinishProperty); }
            set { SetValue(PasswordFinishProperty, value); }
        }
        

        public GridUnlock()
        {
            InitializeComponent();
            this.Loaded += GridUnlock_Loaded;
          
        }

        private void GridUnlock_Loaded(object sender, RoutedEventArgs e)
        {
            this.Bd.MouseLeftButtonDown += GridUnlock_MouseLeftButtonDown;
            this.Bd.MouseMove += GridUnlock_MouseMove;
            this.Bd.MouseLeftButtonUp += GridUnlock_MouseLeftButtonUp;
        }

      

        private void GridUnlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source == null || e.Source.GetType().Name != "BorderCheck")
            {
                _innerPwd = String.Empty;
                return;
            }

            var startPoint = GetPointByKey((e.Source as BorderCheck).Tag.ToString());
            //画一条直线
            var line = new Line();
            line.Stroke = LineColorBrush;
            line.SnapsToDevicePixels = true;
            line.StrokeThickness = 2;
            line.X1 = startPoint.X;
            line.Y1 = startPoint.Y;
            line.X2 = startPoint.X;
            line.Y2 = startPoint.Y;
            this.Bd.Children.Add(line);

            (e.Source as BorderCheck).IsChecked = true;
            _innerPwd = (e.Source as BorderCheck).Tag.ToString();
        }


        private void GridUnlock_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            var tmpPoint = e.GetPosition(Bd);
            (this.Bd.Children[this.Bd.Children.Count - 1] as Line).X2 = tmpPoint.X;
            (this.Bd.Children[this.Bd.Children.Count - 1] as Line).Y2 = tmpPoint.Y;

            if ((e.Source != null && e.Source.GetType().Name != "BorderCheck") || (e.Source as BorderCheck).IsChecked)
            {
                return;
            }
            (e.Source as BorderCheck).IsChecked = true;
            _innerPwd += (e.Source as BorderCheck).Tag.ToString();

            //修改原直线的终点坐标
            var bdPoint = GetPointByKey((e.Source as BorderCheck).Tag.ToString());
            (this.Bd.Children[this.Bd.Children.Count - 1] as Line).X2 = bdPoint.X;
            (this.Bd.Children[this.Bd.Children.Count - 1] as Line).Y2 = bdPoint.Y;

            //重新画一条直线
            var line = new Line();
            line.Stroke = LineColorBrush;
            line.SnapsToDevicePixels = true;
            line.StrokeThickness = 2;
            line.X1 = bdPoint.X;
            line.Y1 = bdPoint.Y;
            line.X2 = tmpPoint.X;
            line.Y2 = tmpPoint.Y;
            this.Bd.Children.Add(line);
        }


        private void GridUnlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Password = _innerPwd;
            if (PasswordFinish!=null)
            {
                PasswordFinish.Execute(Password);
            }
            ResetLine();
        }
      
        /// <summary>
        /// 根据值获取坐标点
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private Point GetPointByKey(String key)
        {
            var point = new Point();
            var oneWidth = this.GridCc.ActualWidth / this.GridCc.RowDefinitions.Count; //每个单元格的宽度
            var oneHeight = this.GridCc.ActualHeight / this.GridCc.ColumnDefinitions.Count; //每个单元格的高度
            switch (key)
            {
                case "1":
                    point.X = oneWidth * 0 + oneWidth / 2;
                    point.Y = oneHeight * 0 + oneHeight / 2;
                    break;
                case "2":
                    point.X = oneWidth * 1 + oneWidth / 2;
                    point.Y = oneHeight * 0 + oneHeight / 2;
                    break;
                case "3":
                    point.X = oneWidth * 2 + oneWidth / 2;
                    point.Y = oneHeight * 0 + oneHeight / 2;
                    break;
                case "4":
                    point.X = oneWidth * 0 + oneWidth / 2;
                    point.Y = oneHeight * 1 + oneHeight / 2;
                    break;
                case "5":
                    point.X = oneWidth * 1 + oneWidth / 2;
                    point.Y = oneHeight * 1 + oneHeight / 2;
                    break;
                case "6":
                    point.X = oneWidth * 2 + oneWidth / 2;
                    point.Y = oneHeight * 1 + oneHeight / 2;
                    break;
                case "7":
                    point.X = oneWidth * 0 + oneWidth / 2;
                    point.Y = oneHeight * 2 + oneHeight / 2;
                    break;
                case "8":
                    point.X = oneWidth * 1 + oneWidth / 2;
                    point.Y = oneHeight * 2 + oneHeight / 2;
                    break;
                case "9":
                    point.X = oneWidth * 2 + oneWidth / 2;
                    point.Y = oneHeight * 2 + oneHeight / 2;
                    break;
                default:
                    break;
            }
            return point;
        }
        /// <summary>
        /// 重置点与点之间的直线
        /// </summary>
        private void ResetLine()
        {
            foreach (UIElement element in this.GridCc.Children)
            {
                if (element.GetType().Name == "BorderCheck")
                {
                    (element as BorderCheck).IsChecked = false;
                }
            }
            for (var i = this.Bd.Children.Count - 1; i >= 0; i--)
            {
                if (this.Bd.Children[i].GetType().Name != "Grid")
                {
                    this.Bd.Children.RemoveAt(i);
                }
            }
        }
    }
}
