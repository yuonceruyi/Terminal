using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace YuanTu.Consts.UserControls
{
    /// <summary>
    /// VerticalKeyboard.xaml 的交互逻辑
    /// </summary>
    public partial class VerticalKeyboard : UserControl
    {
        private static readonly string[] LetterArr = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Select(p=>p.ToString()).ToArray();
        private static readonly string[] NumberArr="1234567890".ToCharArray().Select(p => p.ToString()).ToArray();
        private static readonly string[] ControlArr = "←".ToCharArray().Select(p => p.ToString()).ToArray();
        private static readonly string[] AllCharArr = LetterArr.Concat(NumberArr).Concat(ControlArr).ToArray();

        public VerticalKeyboard()
        {
            InitializeComponent();
            ItemCtl.ItemsSource = AllCharArr;

        }


        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
            nameof(Key), typeof(string), typeof(VerticalKeyboard), new FrameworkPropertyMetadata(string.Empty,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Key
        {
            get { return (string) GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty KeysProperty = DependencyProperty.Register(
            "Keys", typeof(string[]), typeof(VerticalKeyboard), new FrameworkPropertyMetadata((a,b) =>
            {
                //DependencyObject d, DependencyPropertyChangedEventArgs e
                var kb = (VerticalKeyboard) a;
                kb.ItemCtl.ItemsSource = (b.NewValue as string[])??AllCharArr;
            }));

        public string[] Keys
        {
            get { return (string[]) GetValue(KeysProperty); }
            set { SetValue(KeysProperty, value); }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = (SimpleButton) sender;
            var key = btn.Tag.ToString();
            Key = key;
           
            
        }
    }

}
