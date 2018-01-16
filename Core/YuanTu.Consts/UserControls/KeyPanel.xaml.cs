using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace YuanTu.Consts.UserControls
{
    /// <summary>
    ///     KeyPanel.xaml 的交互逻辑
    /// </summary>
    public partial class KeyPanel : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(KeyPanel));

        public static readonly DependencyProperty IDModeProperty = DependencyProperty.Register(
            nameof(IDMode), typeof(SpecialKeyType), typeof(KeyPanel),
            new FrameworkPropertyMetadata(SpecialKeyType.Clear, (d, a) =>
            {
                var keyPanel = (KeyPanel) d;
                var st = (SpecialKeyType) a.NewValue;
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
            typeof(int), typeof(KeyPanel), new FrameworkPropertyMetadata(11, (d, a) =>
            {
                var keyPanel = (KeyPanel) d;
                var newValue = (int) a.NewValue;
                var oldValue = (int) a.OldValue;
                if (newValue == oldValue)
                    return;
                if (!string.IsNullOrEmpty(keyPanel.Text) && keyPanel.Text.Length > newValue)
                    keyPanel.Text = keyPanel.Text.Substring(0, newValue);
            }));

        private static bool _startRun = false;
        private readonly DispatcherTimer _depTimer;

        public KeyPanel()
        {
            InitializeComponent();
            _depTimer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher);
            _depTimer.Tick += _depTimer_Tick;
            _depTimer.Interval = TimeSpan.FromMilliseconds(100);
        }

        public int MaxLength
        {
            get { return (int) GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }

        public SpecialKeyType IDMode
        {
            get { return (SpecialKeyType) GetValue(IDModeProperty); }
            set { SetValue(IDModeProperty, value); }
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public void Clear()
        {
            Text = "";
        }

        public void ButtonClick(object sender, RoutedEventArgs e)
        {
            var tag = ((Button) sender).Tag.ToString();
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

    public enum SpecialKeyType
    {
        Clear,
        IdX,
        DecimalPlaces
    }
}