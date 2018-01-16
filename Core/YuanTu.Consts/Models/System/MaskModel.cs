using System;
using System.Windows;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace YuanTu.Consts.Models
{
    public class MaskModel : BindableBase
    {
        private readonly Action<FrameworkElement> _action;
        private FrameworkElement _element;
        private bool _isVisiable;
        private double _opacity;

        public MaskModel(Action<FrameworkElement> action)
        {
            _action = action;
            Click=new DelegateCommand(() =>
            {
                if (ClickAction != null)
                {
                    var point = Mouse.GetPosition(Application.Current.MainWindow);
                    ClickAction.Invoke(point);
                }
            });
        }

        public bool IsVisiable
        {
            get { return _isVisiable; }
            set
            {
                _isVisiable = value;
                OnPropertyChanged();
                if (_isVisiable)
                {
                    Element = null;
                }
            }
        }

        public FrameworkElement Element
        {
            get { return _element; }
            set
            {
                _element = value;
                _action?.Invoke(value);
            }
        }

        public double Opacity
        {
            get { return _opacity; }
            set
            {
                _opacity = value; 
                OnPropertyChanged();
            }
        }

        public ICommand Click { get;private set; }
        public Action<Point> ClickAction { get; set; }
    }
}