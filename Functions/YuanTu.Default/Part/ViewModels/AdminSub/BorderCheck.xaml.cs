using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace YuanTu.Default.Part.ViewModels.AdminSub
{
    /// <summary>
    /// 九宫格单格
    /// </summary>
    public partial class BorderCheck : UserControl
    {
        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(
            "Radius", typeof (double), typeof (BorderCheck), new PropertyMetadata(double.Parse("32"), (o, e) =>
            {
                var ck = (o as BorderCheck);
                if (ck!=null)
                {
                    ck.Width= (double)e.NewValue;
                    ck.Height = (double)e.NewValue;
                    ck.InEll.Width = (double) e.NewValue > 20 ? ((double) e.NewValue - 20) : ((double) e.NewValue - 1);
                    ck.InEll.Height = (double) e.NewValue > 20 ? ((double) e.NewValue - 20) : ((double) e.NewValue - 1);
                }

            }));

        public double Radius
        {
            get { return (double) GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }
        /// <summary>
        /// 是否选中
        /// </summary>
        public Boolean IsChecked
        {
            get
            {
                return (Boolean)GetValue(IsCheckedProperty);
            }
            set
            {
                SetValue(IsCheckedProperty, value);
            }
        }
        /// <summary>
        /// IsChecked附加属性
        /// </summary>
        public static DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(Boolean), typeof(BorderCheck), new PropertyMetadata(new PropertyChangedCallback(OnIsCheckedPropertyChanged)));
        /// <summary>
        /// IsChecked 变更事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnIsCheckedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            BorderCheck bc = sender as BorderCheck;
            if ((Boolean)e.NewValue == (Boolean)e.OldValue)
            {
                return;
            }
            if (bc.IsChecked)
            {
                bc.Bd.BorderThickness = new Thickness(1.5);
            }
            else
            {
                bc.Bd.BorderThickness = new Thickness(0);
            }
        }

        /// <summary>
        /// 九宫格单格
        /// </summary>
        public BorderCheck()
        {
            InitializeComponent();

            this.MouseEnter += delegate
            {
                this.Cursor = Cursors.Hand;
            };
            this.MouseLeave += delegate
            {
                this.Cursor = Cursors.Arrow;
            };
        }
    }
}
