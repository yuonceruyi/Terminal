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
using System.Windows.Threading;
using YuanTu.Consts.UserControls;

namespace YuanTu.XiaoShanZYY.Component.TakeNum.Controls
{
    /// <summary>
    /// RegNoKeyPanel.xaml 的交互逻辑
    /// </summary>
    public partial class RegNoKeyPanel : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(RegNoKeyPanel));

        public static readonly DependencyProperty IDModeProperty = DependencyProperty.Register(
            nameof(IDMode), typeof(SpecialKeyType), typeof(RegNoKeyPanel),
            new FrameworkPropertyMetadata(SpecialKeyType.Clear, (d, a) =>
            {
                var keyPanel = (RegNoKeyPanel)d;
                var st = (SpecialKeyType)a.NewValue;
                switch (st)
                {
                    case SpecialKeyType.Clear:
                        keyPanel.Button.Content = "清空";
                        keyPanel.Button.Tag = "Clear";
                        break;

                    case SpecialKeyType.IdX:
                        keyPanel.Button.Content = "X";
                        keyPanel.Button.Tag = "X";
                        break;

                    case SpecialKeyType.DecimalPlaces:
                        keyPanel.Button.Content = "●";
                        keyPanel.Button.Tag = ".";
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }));

        public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register(nameof(MaxLength),
            typeof(int), typeof(RegNoKeyPanel), new FrameworkPropertyMetadata(11, (d, a) =>
            {
                var keyPanel = (RegNoKeyPanel)d;
                var newValue = (int)a.NewValue;
                var oldValue = (int)a.OldValue;
                if (newValue == oldValue)
                    return;
                if (!string.IsNullOrEmpty(keyPanel.Text) && keyPanel.Text.Length > newValue)
                    keyPanel.Text = keyPanel.Text.Substring(0, newValue);
            }));

        private static bool _startRun = false;
        private readonly DispatcherTimer _depTimer;

        public RegNoKeyPanel()
        {
            InitializeComponent();
            _depTimer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher);
            _depTimer.Tick += _depTimer_Tick;
            _depTimer.Interval = TimeSpan.FromMilliseconds(100);
        }

        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }

        public SpecialKeyType IDMode
        {
            get { return (SpecialKeyType)GetValue(IDModeProperty); }
            set { SetValue(IDModeProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public void Clear()
        {
            Text = "";
        }

        public void ButtonClick(object sender, RoutedEventArgs e)
        {
            var tag = ((Button)sender).Tag.ToString();
            switch (tag)
            {
                case "Clear":
                    Clear();
                    break;

                case "Back":
                    if (string.IsNullOrWhiteSpace(Text))
                        return;
                    Text = Text?.Substring(0, Text.Length - 1);
                    break;

                default:
                    if (string.IsNullOrEmpty(Text) || Text.Length < MaxLength)
                        Text += tag;
                    break;
            }
        }

        private void DelButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Clear();
        }


        private void _depTimer_Tick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Text))
                return;
            Text = Text?.Substring(0, Text.Length - 1);
        }
    }
}
