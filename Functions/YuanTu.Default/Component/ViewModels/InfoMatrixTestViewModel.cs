using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Prism.Commands;
using YuanTu.Consts.Models;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Component.ViewModels
{
    public class InfoMatrixTestViewModel : ViewModelBase
    {
        private ObservableCollection<object> _data = new ObservableCollection<object>();
        private int _columnCount = 4;
        private int _rowCount = 4;
        private string _infoMatrixTitle = "请触摸下方卡片选择科室";

        public override string Title => "科室选择";
        public InfoMatrixTestViewModel()
        {
            #region Debug

            var confirm = new DelegateCommand<Info>(i => MessageBox.Show(i.ToString()));

            ColumnPlus = new DelegateCommand(() => ColumnCount++);
            ColumnMinus = new DelegateCommand(() => ColumnCount--);
            RowPlus = new DelegateCommand(() => RowCount++);
            RowMinus = new DelegateCommand(() => RowCount--);

            Info = new DelegateCommand(() =>
            {
                var l = new List<Info>();
                for (var i = 0; i < 50; i++)
                    l.Add(new Info
                    {
                        Title = "标题" + (i + 1),
                        No = i,
                        ConfirmCommand = confirm
                    });
                Data = new ObservableCollection<object>(l);
            });
            var payTypePaths = new[]
            {
                "微信支付","银联","诊疗卡账户","支付宝"
            };
            for (int i = 0; i < payTypePaths.Length; i++)
            {
                payTypePaths[i] = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource", $"{payTypePaths[i]}.png");
            }
            InfoIcon = new DelegateCommand(() =>
            {
                var l = new List<Info>();
                for (var i = 0; i < 50; i++)
                    l.Add(new InfoIcon
                    {
                        Title = "标题" + (i + 1),
                        No = i,
                        ConfirmCommand = confirm,
                        IconUri = new Uri(payTypePaths[i% payTypePaths.Length])
                    });
                Data = new ObservableCollection<object>(l);
            });
            InfoMore = new DelegateCommand(() =>
            {
                var l = new List<Info>();
                for (var i = 0; i < 50; i++)
                    l.Add(new InfoMore
                    {
                        Title = "标题"+(i + 1), No = i,
                        ConfirmCommand = confirm,
                        Amount = i*5,
                        SubTitle = "副标题"+(i+2),
                        Type = "费用类型"
                    });
                Data = new ObservableCollection<object>(l);
            });
            var regTypePaths = new []
            {
                "普通门诊","专家门诊","便民门诊","知名专家" 
            };
            for (int i = 0; i < regTypePaths.Length; i++)
            {
                regTypePaths[i] = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource", $"{regTypePaths[i]}.png");
            }
            InfoType = new DelegateCommand(() =>
            {
                var l = new List<Info>();
                for (var i = 0; i < 50; i++)
                    l.Add(new InfoType
                    {
                        Title = "标题" + (i + 1),
                        No = i,
                        ConfirmCommand = confirm,
                        Remark = "文本介绍",
                        IconUri = new Uri(regTypePaths[i% regTypePaths.Length])
                    });
                Data = new ObservableCollection<object>(l);
            });

            #endregion Debug

            #region Mock

            var confirmCommand = new DelegateCommand<Info>(i => MessageBox.Show(i.ToString()), i => true);

            var list = new List<object>();
            for (var i = 0; i < 50; i++)
            {
                list.Add(new Info
                {
                    Title = (i + 1).ToString(),
                    No = i,
                    ConfirmCommand = confirmCommand
                });
            }
            Data = new ObservableCollection<object>(list);

            #endregion Mock
        }

        public string InfoMatrixTitle
        {
            get { return _infoMatrixTitle; }
            set
            {
                _infoMatrixTitle = value; 
                OnPropertyChanged();
            }
        }

        public int RowCount
        {
            get { return _rowCount; }
            set
            {
                _rowCount = value;
                OnPropertyChanged();
            }
        }

        public int ColumnCount
        {
            get { return _columnCount; }
            set
            {
                _columnCount = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<object> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }
        
        #region Debug

        public DelegateCommand ColumnPlus { get; set; }
        public DelegateCommand ColumnMinus { get; set; }
        public DelegateCommand RowPlus { get; set; }
        public DelegateCommand RowMinus { get; set; }
        public DelegateCommand Info { get; set; }
        public DelegateCommand InfoIcon { get; set; }
        public DelegateCommand InfoMore { get; set; }
        public DelegateCommand InfoType { get; set; }

        #endregion Debug

    }
}