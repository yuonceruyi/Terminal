using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using YuanTu.Consts.Annotations;

namespace YuanTu.Consts.UserControls
{
    /// <summary>
    ///     FullKeyboard.xaml 的交互逻辑
    /// </summary>
    public partial class FullKeyboard : UserControl, INotifyPropertyChanged
    {
        //    public static readonly DependencyProperty KeyboardImageProperty = DependencyProperty.Register(
        //nameof(KeyboardImage), typeof(string), typeof(FullKeyboard));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(FullKeyboard));

        public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register(
            nameof(MaxLength), typeof(int), typeof(FullKeyboard), new FrameworkPropertyMetadata(11, (d, a) =>
            {
                var keyboard = (FullKeyboard) d;
                var newValue = (int) a.NewValue;
                var oldValue = (int) a.OldValue;
                if (newValue == oldValue)
                    return;
                if (!string.IsNullOrEmpty(keyboard.Text) && keyboard.Text.Length > newValue)
                    keyboard.Text = keyboard.Text.Substring(0, newValue);
            }));

        private bool _lower;

        public FullKeyboard()
        {
            InitializeComponent();
        }

        //public Uri KeyboardImage
        //{
        //    get { return (Uri)GetValue(KeyboardImageProperty); }
        //    set { SetValue(KeyboardImageProperty, value); }
        //}

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public int MaxLength
        {
            get { return (int) GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }

        public bool Lower
        {
            get { return _lower; }
            set
            {
                _lower = value;
                OnPropertyChanged();
            }
        }

        public void ButtonClick(object sender, RoutedEventArgs e)
        {
            var tag = ((Button) sender).Tag.ToString();
            switch (tag)
            {
                case "delete":
                    if (string.IsNullOrWhiteSpace(Text))
                        return;
                    Text = Text?.Substring(0, Text.Length - 1);
                    break;

                case "shift":
                    Lower = !Lower;
                    break;

                default:
                    if (string.IsNullOrEmpty(Text) || Text.Length < MaxLength)
                        Text += Lower ? tag.ToLower() : tag;
                    break;
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged
    }
}