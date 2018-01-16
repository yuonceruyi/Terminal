using System.Windows;
using System.Windows.Controls;

namespace YuanTu.Consts.UserControls
{
    /// <summary>
    ///     TitleTextBlock.xaml 的交互逻辑
    /// </summary>
    public partial class TitleTextBlock : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(TitleTextBlock),
                new PropertyMetadata(default(string), (d, a) =>
                {
                    var titleLabel = (TitleTextBlock) d;
                    titleLabel.TitleBlock.Text = (string) a.NewValue;
                }));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(TitleTextBlock),
                new PropertyMetadata(default(string), (d, a) =>
                {
                    var titleLabel = (TitleTextBlock) d;
                    titleLabel.ValueBlock.Text = (string) a.NewValue;
                }));

        public static readonly DependencyProperty TitleStyleProperty = DependencyProperty.Register(
            nameof(TitleStyle), typeof(Style), typeof(TitleTextBlock), new PropertyMetadata(default(Style), (d, a) =>
            {
                var titleLabel = (TitleTextBlock) d;
                titleLabel.TitleBlock.Style = (Style) a.NewValue;
            }));

        public static readonly DependencyProperty ValueStyleProperty = DependencyProperty.Register(
            nameof(ValueStyle), typeof(Style), typeof(TitleTextBlock), new PropertyMetadata(default(Style), (d, a) =>
            {
                var titleLabel = (TitleTextBlock) d;
                titleLabel.ValueBlock.Style = (Style) a.NewValue;
            }));

        public static readonly DependencyProperty TitleColumnWidthProperty = DependencyProperty.Register(
            nameof(TitleColumnWidth), typeof(GridLength), typeof(TitleTextBlock),
            new PropertyMetadata(new GridLength(1, GridUnitType.Star), (d, a) =>
            {
                var titleLabel = (TitleTextBlock) d;
                if (a.NewValue is GridLength)
                    titleLabel.TitleColumnDefinition.Width = (GridLength) a.NewValue;
            }));

        public static readonly DependencyProperty ValueColumnWidthProperty = DependencyProperty.Register(
            nameof(ValueColumnWidth), typeof(GridLength), typeof(TitleTextBlock),
            new PropertyMetadata(new GridLength(1, GridUnitType.Star), (d, a) =>
            {
                var titleLabel = (TitleTextBlock) d;
                if (a.NewValue is GridLength)
                    titleLabel.ValueColumnDefinition.Width = (GridLength) a.NewValue;
            }));

        public TitleTextBlock()
        {
            InitializeComponent();
        }

        public Style TitleStyle
        {
            get { return (Style) GetValue(TitleStyleProperty); }
            set { SetValue(TitleStyleProperty, value); }
        }

        public Style ValueStyle
        {
            get { return (Style) GetValue(ValueStyleProperty); }
            set { SetValue(ValueStyleProperty, value); }
        }

        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public object Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public GridLength TitleColumnWidth
        {
            get { return (GridLength) GetValue(TitleColumnWidthProperty); }
            set { SetValue(TitleColumnWidthProperty, value); }
        }

        public GridLength ValueColumnWidth
        {
            get { return (GridLength) GetValue(ValueColumnWidthProperty); }
            set { SetValue(ValueColumnWidthProperty, value); }
        }
    }
}