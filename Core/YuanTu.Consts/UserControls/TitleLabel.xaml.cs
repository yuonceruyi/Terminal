using System.Windows;
using System.Windows.Controls;

namespace YuanTu.Consts.UserControls
{
    /// <summary>
    ///     TitleLabel.xaml 的交互逻辑
    /// </summary>
    public partial class TitleLabel : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(object), typeof(TitleLabel),
                new PropertyMetadata(default(object),(d,a)=>
                {
                    var titleLabel = (TitleLabel)d;
                    titleLabel.TitlePresenter.Content = a.NewValue;
                }));

        public static readonly DependencyProperty ValueProperty = 
            DependencyProperty.Register(nameof(Value), typeof(object), typeof(TitleLabel), 
                new PropertyMetadata(default(object),(d,a)=>
                {
                    var titleLabel = (TitleLabel)d;
                    titleLabel.ValuePresenter.Content = a.NewValue;
                }));

        public static readonly DependencyProperty TitleColumnWidthProperty = DependencyProperty.Register(
            nameof(TitleColumnWidth), typeof(GridLength), typeof(TitleLabel),
            new PropertyMetadata(new GridLength(1, GridUnitType.Star), (d, a) =>
            {
                var titleLabel = (TitleLabel) d;
                if (a.NewValue is GridLength)
                    titleLabel.TitleColumnDefinition.Width = (GridLength) a.NewValue;
            }));

        public static readonly DependencyProperty ValueColumnWidthProperty = DependencyProperty.Register(
            nameof(ValueColumnWidth), typeof(GridLength), typeof(TitleLabel),
            new PropertyMetadata(new GridLength(1, GridUnitType.Star), (d, a) =>
            {
                var titleLabel = (TitleLabel) d;
                if (a.NewValue is GridLength)
                    titleLabel.ValueColumnDefinition.Width = (GridLength) a.NewValue;
            }));

        public TitleLabel()
        {
            InitializeComponent();
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