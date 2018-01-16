using Microsoft.Practices.ServiceLocation;
using Prism.Interactivity.InteractionRequest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using YuanTu.Consts.Services;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Devices.Ink;

namespace YuanTu.Default.Component.Auth.Dialog.Views
{
    /// <summary>
    /// FullInputBoard.xaml 的交互逻辑
    /// </summary>
    public partial class FullInputBoard : UserControl, INotifyPropertyChanged
    {
        private string _innerspell = string.Empty;//目前输入的拼音
        private string _innerWorkds = string.Empty;//目前存入的汉字
        private int pageIndex = 0;//当前页面Index
        private int pageCount = 0;//总页面数
        private const int PageItems = 8;//当前页面候选词个数
        private List<Keywords> _totalwords = null;
        private InputMode _inputMode;
        private bool isExpanded;
        private bool isPinInput;
        public bool IsPinInput
        {
            get
            {
                return isPinInput;
            }
            set
            {
                isPinInput = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPinInput)));
            }
        }
        public bool IsExpanded
        {
            get
            {
                return isExpanded;
            }
            set
            {
                isExpanded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
            }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(FullInputBoard), new PropertyMetadata(default(string)));

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FullInputBoard()
        {
            InitializeComponent();
            InputMode = InputMode.PinInput;
            ButtonSummaryNext.Visibility = Visibility.Collapsed;
            ButtonSummaryPreview.Visibility = Visibility.Collapsed;
            this.DataContext = this;
        }

        public void CharKey_Click(object sender, EventArgs e)
        {
            var tag = ((Button)sender).Tag.ToString();

            switch (tag)
            {
                case "delete":
                    if (!_innerspell.IsNullOrWhiteSpace())
                    {
                        _innerspell = _innerspell?.Substring(0, _innerspell.Length - 1);
                    }
                    else if (!_innerWorkds.IsNullOrWhiteSpace())
                    {
                        _innerWorkds = _innerWorkds?.Substring(0, _innerWorkds.Length - 1);
                        SelectWords?.Invoke(_innerWorkds);
                        Text = _innerWorkds;
                    }
                    KeyAction?.Invoke(KeyType.DeleteKey);
                    break;
                case "0":
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":

                    if (string.IsNullOrEmpty(_innerspell))
                    {
                        _innerWorkds += tag;
                        KeyAction?.Invoke(KeyType.CharKey);
                    }
                    break;
                default:
                    _innerspell += tag;
                    KeyAction?.Invoke(KeyType.CharKey);
                    break;
            }
            pageIndex = 0;
            Update();
        }

        private void TempWords_Click(object sender, RoutedEventArgs e)
        {
            var wd = (sender as Button)?.DataContext as Keywords;
            _innerWorkds += wd?.Key;
            _innerspell = string.Empty;

            if (InputMode == InputMode.HandInput)
                ClearAlternates();

            pageIndex = 0;
            Update();
            SelectWords?.Invoke(_innerWorkds);
            Text = _innerWorkds;
            KeyAction?.Invoke(KeyType.CandidatesKey);
        }

        private void TempWordsLast_Click(object sender, RoutedEventArgs e)
        {
            if (pageIndex > 0)
            {
                pageIndex--;
                Update();
                KeyAction?.Invoke(KeyType.PageKey);
            }
        }

        private void TempWordsNext_Click(object sender, RoutedEventArgs e)
        {
            if (pageIndex < pageCount - 1)
            {
                pageIndex++;
                Update();
                KeyAction?.Invoke(KeyType.PageKey);
            }
        }

        private void Update()
        {

            if (InputMode == InputMode.PinInput)
            {
                var onlychangepage = (py.Text == _innerspell) && (hz.Text == _innerWorkds) &&
                                     (_totalwords?.Any() ?? false);
                py.Text = _innerspell;
                hz.Text = _innerWorkds;
                List<Keywords> tmp = null;
                if (_innerspell.IsNullOrWhiteSpace())
                {
                    CatalogList.ItemsSource = null;
                    _totalwords = null;
                    pageCount = 0;
                    pageIndex = 0;
                }
                else
                {
                    if (!onlychangepage)
                    {
                        var res = ServiceLocator.Current.GetInstance<IResourceEngine>();
                        var dbpath = res.GetResourceFullPath("py");
                        var dbwords = DBManager.Query<Keywords>(dbpath, "Select * from Keywords where Phonetic like $1 group by key order by Times desc",
                            $"{_innerspell}%");
                        _totalwords = dbwords.OrderBy(p => p.Phonetic.Length).ToList();
                        pageCount = (_totalwords.Count + PageItems - 1) / PageItems;
                    }
                    var skip = PageItems * pageIndex;
                    var realcount = Math.Min(PageItems, _totalwords.Count - skip);
                    tmp = _totalwords.Skip(skip).Take(realcount).ToList();
                }
                CatalogList.ItemsSource = tmp;
            }
            else
            {
                hz.Text = _innerWorkds;
                List<Keywords> tmp = null;
                if (_core.Alternates == null)
                {
                    CatalogList.ItemsSource = null;
                    _totalwords = null;
                    pageCount = 0;
                    pageIndex = 0;
                }
                else
                {
                    _totalwords = _core.Alternates
                        .Where(p =>
                    {
                        //汉字的 UNICODE 编码范围是4e00-9fbb
                        //·
                        if (p.IsNullOrWhiteSpace())
                        {
                            return false;
                        }
                        return (p[0] >= 0x4e00 && p[0] <= 0x9fbb)||(p[0]>=0x41&&p[0]<=0x7A)||p[0]== '·'||p[0]=='.'|| (p[0] >= 0x30 && p[0] <= 0x39);
                    })
                    .Select(p => new Keywords
                    {
                        Key = p.ToString()
                    }).ToList();
                    pageCount = (_totalwords.Count + PageItems - 1) / PageItems;

                    var skip = PageItems * pageIndex;
                    var realcount = Math.Min(PageItems, _totalwords.Count - skip);
                    tmp = _totalwords.Skip(skip).Take(realcount).ToList();
                }
                CatalogList.ItemsSource = tmp;
            }
            ButtonSummaryPreview.Visibility = pageIndex > 0 ? Visibility.Visible : Visibility.Collapsed;
            ButtonSummaryNext.Visibility = pageIndex < pageCount - 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        public Action<string> SelectWords { get; set; }

        public Action<KeyType> KeyAction { get; set; }

        private void ChangeInput_Click(object sender, RoutedEventArgs e)
        {
            InputMode = InputMode == InputMode.PinInput ? InputMode.HandInput : InputMode.PinInput;
            KeyAction?.Invoke(KeyType.ShiftKey);
        }

        public InputMode InputMode
        {
            get { return _inputMode; }
            set
            {
                _inputMode = value;
                IsPinInput = _inputMode == InputMode.HandInput ? false : true;
                this.ButtonChangeInput.Content = _inputMode == InputMode.HandInput ? "拼音" : "手写";
                ButtonSummaryNext.Visibility = Visibility.Collapsed;
                ButtonSummaryPreview.Visibility = Visibility.Collapsed;
                CatalogList.ItemsSource = null;
                py.Text = _innerspell = null;
                if (_inputMode != InputMode.HandInput) return;
                this.WritingCanvas.Strokes.Clear();
                _core.ClearAlternates();
            }
        }

        public bool EnableShift
        {
            get { return ButtonChangeInput.Visibility == Visibility.Visible; }
            set { ButtonChangeInput.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        private readonly RecognizationCore _core = new RecognizationCore();

        private void WritingCanvasOnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            _core.Recognize(this.WritingCanvas.Strokes);
            pageIndex = 0;
            Update();
            KeyAction?.Invoke(KeyType.Line);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearAlternates();
            KeyAction?.Invoke(KeyType.DeleteKey);
        }

        private void ClearAlternates()
        {
            if (_core.Alternates?.Any() ?? false)
            {
                this.WritingCanvas.Strokes.Clear();
                _core.ClearAlternates();
            }
            else
            {
                if (!_innerWorkds.IsNullOrWhiteSpace())
                {
                    _innerWorkds = _innerWorkds?.Substring(0, _innerWorkds.Length - 1);
                    SelectWords?.Invoke(_innerWorkds);
                    Text = _innerWorkds;
                }
            }
            pageIndex = 0;
            Update();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            KeyAction?.Invoke(KeyType.CloseKey);
        }

        private void StretchButton_Click(object sender, RoutedEventArgs e)
        {
            StretchButton.Content = IsExpanded ? "扩大" : "缩小";
            IsExpanded = !IsExpanded;
        }
        
    }

    public class Keywords
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Phonetic { get; set; }
        public int Times { get; set; }
    }

    public enum InputMode
    {
        PinInput, HandInput
    }

    public enum KeyType
    {
        /// <summary>
        /// 字母
        /// </summary>
        CharKey,

        /// <summary>
        /// 候选词
        /// </summary>
        CandidatesKey,

        /// <summary>
        /// 翻页
        /// </summary>
        PageKey,

        /// <summary>
        /// 输入法切换
        /// </summary>
        ShiftKey,

        /// <summary>
        /// 删除
        /// </summary>
        DeleteKey,

        /// <summary>
        /// 手写笔迹
        /// </summary>
        Line,

        /// <summary>
        /// 关闭按钮
        /// </summary>
        CloseKey,
    }
}