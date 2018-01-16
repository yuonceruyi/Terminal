using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace YuanTu.Consts.UserControls
{
    /// <summary>
    ///     ScrollPicker.xaml 的交互逻辑
    /// </summary>
    public partial class ScrollPicker : UserControl, INotifyPropertyChanged
    {
        public static readonly RoutedEvent SelectedIndexChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(SelectedIndexChanged), RoutingStrategy.Bubble, typeof(ValueChangedEventHandler),
                typeof(ScrollPicker));
        public event ValueChangedEventHandler SelectedIndexChanged
        {
            add { AddHandler(SelectedIndexChangedEvent, value); }
            remove { RemoveHandler(SelectedIndexChangedEvent, value); }
        }

        public static readonly RoutedEvent SelectedItemChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(SelectedItemChanged), RoutingStrategy.Bubble, typeof(ValueChangedEventHandler),
                typeof(ScrollPicker));
        public event ValueChangedEventHandler SelectedItemChanged
        {
            add { AddHandler(SelectedItemChangedEvent, value); }
            remove { RemoveHandler(SelectedItemChangedEvent, value); }
        }

        public ScrollPicker()
        {
            InitializeComponent();
            Loaded += (s, a) => UpdateVisual();
        }

        #region SelectedIndex

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(
                nameof(SelectedIndex),
                typeof(int),
                typeof(ScrollPicker),
                new FrameworkPropertyMetadata(
                    default(int),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedIndexChanged,
                    OnCoerceSelectedIndex
                    )
                );

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (ScrollPicker)d;
            if (!picker._preventReentry)
                picker.ItemsPanel.SelectedIndex = (int)e.NewValue;
            picker.UpdateVisual();
            var ee = new ValueChangedEventArgs((int)e.OldValue, (int)e.NewValue)
            {
                RoutedEvent = SelectedIndexChangedEvent,
                Source = picker,
            };
            picker.OnSelectedIndexChanged(ee);
        }

        private static object OnCoerceSelectedIndex(DependencyObject d, object baseValue)
        {
            var picker = (ScrollPicker)d;
            var i = (int)baseValue;
            if (i < 0)
                return 0;
            var count = picker.Values.Count;
            if (i >= count)
                return count - 1;
            return i;
        }

        protected virtual void OnSelectedIndexChanged(ValueChangedEventArgs e)
        {
            RaiseEvent(e);
        }

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        #endregion SelectedIndex

        #region SelectedItem

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(ScrollPicker),
                new FrameworkPropertyMetadata(default(object), OnSelectedItemChanged, OnCoerceSelectedItem)
                );

        private static object OnCoerceSelectedItem(DependencyObject d, object baseValue)
        {
            var picker = (ScrollPicker)d;
            if (picker.Values == null)
                return picker.SelectedItem;
            if (picker.Values.Contains(baseValue))
                return baseValue;
            return picker.SelectedItem;
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (ScrollPicker)d;
            if (!picker._preventReentry)
                picker.ItemsPanel.SelectedItem = e.NewValue;
            picker.UpdateVisual();
            var ee = new ValueChangedEventArgs((int)(e.OldValue ?? 0), (int)e.NewValue)
            {
                RoutedEvent = SelectedItemChangedEvent,
                Source = picker,
            };
            picker.OnSelectedItemChanged(ee);
        }

        protected virtual void OnSelectedItemChanged(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        #endregion

        #region Values

        public static readonly DependencyProperty ValuesProperty =
            DependencyProperty.Register(
                nameof(Values),
                typeof(ObservableCollection<object>),
                typeof(ScrollPicker),
                new FrameworkPropertyMetadata(
                    default(ObservableCollection<object>),
                    OnValuesChanged
                    )
                );

        private static void OnValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (ScrollPicker)d;
            picker._needRecalculate = true;
            picker.OnValuesChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
            picker.UpdateVisual();
        }
        private void OnValuesChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            // Remove handler for oldValue.CollectionChanged
            var oldValueINotifyCollectionChanged = oldValue as INotifyCollectionChanged;

            if (null != oldValueINotifyCollectionChanged)
            {
                oldValueINotifyCollectionChanged.CollectionChanged -= newValueINotifyCollectionChanged_CollectionChanged;
            }
            // Add handler for newValue.CollectionChanged (if possible)
            var newValueINotifyCollectionChanged = newValue as INotifyCollectionChanged;
            if (null != newValueINotifyCollectionChanged)
            {
                newValueINotifyCollectionChanged.CollectionChanged += newValueINotifyCollectionChanged_CollectionChanged;
            }
        }

        void newValueINotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //Do your stuff here.
            //UpdateVisual();
        }

        public ObservableCollection<object> Values
        {
            get { return (ObservableCollection<object>)GetValue(ValuesProperty); }
            set { SetValue(ValuesProperty, value); }
        }

        #endregion Values

        #region ItemTemplate

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
                nameof(ItemTemplate),
                typeof(DataTemplate),
                typeof(ScrollPicker),
                new FrameworkPropertyMetadata(null, PropertyChangedCallback)
                );

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (ScrollPicker)d;
            picker._needRecalculate = true;
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        #endregion ItemTemplate

        #region Exponent

        public static readonly DependencyProperty ExponentProperty = DependencyProperty.Register(
            nameof(Exponent), typeof(double), typeof(ScrollPicker), new PropertyMetadata(1.0, OnExponentChanged, OnCoerceExponent));

        private static object OnCoerceExponent(DependencyObject dependencyObject, object baseValue)
        {
            var value = (double)baseValue;
            if (value < 0)
                return 1;
            return value;
        }

        private static void OnExponentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public double Exponent
        {
            get { return (double)GetValue(ExponentProperty); }
            set { SetValue(ExponentProperty, value); }
        }

        #endregion Exponent

        #region Helpers

        private int GetIndexAtVisual()
        {
            var target = Canvas.ActualHeight / 2 - Canvas.GetTop(ItemsPanel);
            return GetIndexAtHeight(target);
        }

        private int GetIndexAtMouse()
        {
            var pt = Mouse.GetPosition(Canvas);
            var target = pt.Y - Canvas.GetTop(ItemsPanel);
            return GetIndexAtHeight(target);
        }

        private int GetIndexAtHeight(double target)
        {
            if (target < 0)
                return 0;

            var current = 0.0;
            var items = ItemsPanel.Items;
            for (var i = 0; i < items.Count; i++)
            {
                var listBoxItem = ItemsPanel.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                current += listBoxItem.ActualHeight;
                if (current >= target)
                    return i;
            }
            return items.Count - 1;
        }

        private double CalculateSelectedTop(int index)
        {
            var totalHeight = Enumerable.Range(0, index).Aggregate(0.0, (s, i) => s + GetItemHeight(i));
            var targetTop = (Canvas.ActualHeight - GetItemHeight(index)) / 2 - totalHeight;
            return targetTop;
        }

        private double GetItemHeight(int index)
        {
            if (index >= ItemsPanel.Items.Count)
                return 0;
            var listBoxItem = ItemsPanel.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
            return listBoxItem?.ActualHeight ?? 0;
        }

        public void UpdateVisual()
        {
            var index = ItemsPanel.SelectedIndex;
            if (index < 0)
                return;
            var top = CalculateSelectedTop(index);
            var da = new DoubleAnimation
            {
                To = top,
                Duration = new Duration(TimeSpan.FromMilliseconds(100))
            };
            ItemsPanel.BeginAnimation(Canvas.TopProperty, da);
        }

        #endregion Helpers

        #region EventHandlers

        private bool _dragging;
        private Point _dragPt;
        private double _originalDragOffset;
        private bool _needRecalculate;
        private double _min;
        private double _max;

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;
            if (This.IsMouseOver)
            {
                _dragPt = Mouse.GetPosition(Canvas);
                _originalDragOffset = Canvas.GetTop(ItemsPanel);
                _dragging = true;
            }
            if (!This.IsMouseCaptured)
            {
                This.CaptureMouse();
                e.Handled = true;
                //Mouse.Capture(This, CaptureMode.SubTree);
            }
            else
                This.ReleaseMouseCapture();
        }

        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;
            if (_dragging)
            {
                if (_dragPt != Mouse.GetPosition(Canvas))
                    ItemsPanel.SelectedIndex = GetIndexAtVisual();
                else
                    ItemsPanel.SelectedIndex = GetIndexAtMouse();
                UpdateVisual();

                _dragging = false;
            }

            if (This.IsMouseCaptured)
                This.ReleaseMouseCapture();
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!This.IsMouseOver)
                return;
            if (e.Delta > 0)
                ItemsPanel.SelectedIndex++;
            else if (ItemsPanel.SelectedIndex > 0)
                ItemsPanel.SelectedIndex--;
            UpdateVisual();
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed || !_dragging)
                return;
            var pt = Mouse.GetPosition(Canvas);
            var diff = pt.Y - _dragPt.Y;
            var powed = Math.Pow(Math.Abs(diff), Exponent) * Math.Sign(diff);
            ItemsPanel.BeginAnimation(Canvas.TopProperty, null);
            if (_needRecalculate)
            {
                if (ItemsPanel.Items.Count > 0)
                {
                    _max = Canvas.ActualHeight - GetItemHeight(0);
                    _min = -Enumerable.Range(0, ItemsPanel.Items.Count - 1).Aggregate(0.0, (s, i) => s + GetItemHeight(i));
                }
                _needRecalculate = false;
            }
            var source = _originalDragOffset + powed;
            Canvas.SetTop(ItemsPanel, Math.Max(Math.Min(source, _max), _min));

            ItemsPanel.SelectedIndex = GetIndexAtVisual();
        }

        private bool _preventReentry;

        private void ListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_preventReentry)
                return;
            _preventReentry = true;
            SelectedIndex = ItemsPanel.SelectedIndex;
            SelectedItem = ItemsPanel.SelectedItem;
            _preventReentry = false;
        }

        #endregion EventHandlers

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs e);

    public class ValueChangedEventArgs : RoutedEventArgs
    {
        public ValueChangedEventArgs(object oldValue, object newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
        public object OldValue { get; }
        public object NewValue { get; }
        protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
        {
            ValueChangedEventHandler handler = (ValueChangedEventHandler)genericHandler;

            handler(genericTarget, this);
        }
    }
}