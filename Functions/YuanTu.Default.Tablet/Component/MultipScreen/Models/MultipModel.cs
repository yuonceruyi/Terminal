using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Events;
using YuanTu.Consts;
using YuanTu.Consts.EventModels;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.UserCenter.Auth;
using YuanTu.Consts.Models.UserCenter.Register;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;
using YuanTu.Default.Tablet.Component.Cashier.Models;

namespace YuanTu.Default.Tablet.Component.MultipScreen.Models
{
    public interface IMultipModel : IModel
    {
        bool ShowContent { get; set; }
        ImageBrush Background { get; set; }
        string Hint { get; set; }
        DateTime Now { get; set; }
        ImageSource MainLogo { get; }
        string MainTitle { get; set; }

        MultipartData MultipartData { get; set; }
    }

    public class MultipartData : ModelBase
    {
        public string DataTemplate { get; set; }
    }

    public class MultipartListData : MultipartData
    {
        private List<PayInfoItem> _leftList;

        private List<PayInfoItem> _midList;

        private List<PayInfoItem> _rightList;

        public MultipartListData()
        {
            DataTemplate = "ListTemplate";
        }

        public List<PayInfoItem> LeftList
        {
            get => _leftList;
            set
            {
                _leftList = value;
                OnPropertyChanged();
            }
        }

        public List<PayInfoItem> RightList
        {
            get => _rightList;
            set
            {
                _rightList = value;
                OnPropertyChanged();
            }
        }

        public List<PayInfoItem> MidList
        {
            get => _midList;
            set
            {
                _midList = value;
                OnPropertyChanged();
            }
        }
    }

    public class MultipartPayCardData : MultipartData
    {
        private Uri _backUri;

        private Uri _cardUri;

        private List<PayInfoItem> _leftList;

        public MultipartPayCardData()
        {
            DataTemplate = "PayCardTemplate";
        }

        public List<PayInfoItem> LeftList
        {
            get => _leftList;
            set
            {
                _leftList = value;
                OnPropertyChanged();
            }
        }

        public Uri CardUri
        {
            get => _cardUri;
            set
            {
                _cardUri = value;
                OnPropertyChanged();
            }
        }

        public Uri BackUri
        {
            get => _backUri;
            set
            {
                _backUri = value;
                OnPropertyChanged();
            }
        }
    }

    public class MultipModel : ModelBase, IMultipModel
    {
        public MultipModel(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<ViewChangeEvent>().Subscribe(ViewHasChanged);
            eventAggregator.GetEvent<PubSubEvent<DateTime>>().Subscribe(now => Now = now);
        }

        [Dependency]
        public NavigationEngine NavigationEngine { get; set; }

        [Dependency]
        public IResourceEngine ResourceEngine { get; set; }

        private void ViewHasChanged(ViewChangeEvent eveEvent)
        {
            var to = eveEvent.To;
            if (to == A.CK.Info)
            {
                OnToInfo();
            }
            else if (to == A.XC.Confirm)
            {
                OnToRegisterConfirm();
            }
            else if (to == A.YY.Confirm)
            {
                OnToAppointConfirm();
            }
            else if (to == AInner.SY.Card)
            {
                OnToCashierCard();
            }
            else
            {
                //todo 默认界面
                ShowContent = false;
                MultipartData = null;
                Background = new ImageBrush(ResourceEngine.GetImageResource("扩展屏默认页"));
            }
        }

        private T GetInstance<T>()
        {
            return ServiceLocator.Current.GetInstance<T>();
        }

        private void OnToCashierCard()
        {
            var cashier = GetInstance<ICashierModel>();
            var payInfoItems = new List<PayInfoItem>
            {
                new PayInfoItem("应付金额：", cashier.Amount.In元(), true),
                new PayInfoItem("优惠金额：", 0m.In元()),
                new PayInfoItem("支付方式：", "区域诊疗卡")
            };
            MultipartData = new MultipartPayCardData
            {
                LeftList = payInfoItems,
                CardUri = ResourceEngine.GetImageResourceUri("卡_诊疗卡"),
                BackUri = ResourceEngine.GetImageResourceUri("身份证扫描处")
            };
            Hint = "请确认预约信息";
            ShowContent = true;
            Background = new ImageBrush(ResourceEngine.GetImageResource("Main"));
        }

        private void OnToAppointConfirm()
        {
            var registerModel = GetInstance<IRegisterModel>();
            var patientInfo = GetInstance<IAuthModel>().当前就诊人信息;
            var schedule = registerModel.当前选择医生排班;
            var source = registerModel.当前选择号源;
            var payInfoItems = new List<PayInfoItem>
            {
                new PayInfoItem("医院：", registerModel.当前选择医院?.name),
                new PayInfoItem("就诊时间：", $"{schedule?.medDate} {schedule?.medAmPm.SafeToAmPm()}"),
                new PayInfoItem("科室：", registerModel.当前选择科室?.deptName),
                new PayInfoItem("医生：", schedule?.name),
                new PayInfoItem("就诊号源：", $"{source?.appoNo} ({source?.medBegTime}~{source?.medEndTime})"),
                new PayInfoItem("就诊人：", patientInfo?.patientName),
                new PayInfoItem("账户余额：", Convert.ToDecimal(patientInfo?.balance).In元()),
                new PayInfoItem("应付：", Convert.ToDecimal(schedule?.regAmount).In元(), true)
            };
            MultipartData = new MultipartListData
            {
                MidList = payInfoItems
            };
            Hint = "请确认预约信息";
            ShowContent = true;
            Background = new ImageBrush(ResourceEngine.GetImageResource("Main"));
        }

        private void OnToRegisterConfirm()
        {
            var registerModel = GetInstance<IRegisterModel>();
            var patientInfo = GetInstance<IAuthModel>().当前就诊人信息;
            var payInfoItems = new List<PayInfoItem>
            {
                new PayInfoItem("医院：", registerModel.当前选择医院?.name),
                new PayInfoItem("就诊时间：",
                    $"{registerModel.当前选择医生排班?.medDate} {registerModel.当前选择医生排班?.medAmPm.SafeToAmPm()}"),
                new PayInfoItem("科室：", registerModel.当前选择科室?.deptName),
                new PayInfoItem("医生：", registerModel.当前选择医生排班?.name),
                new PayInfoItem("就诊人：", patientInfo?.patientName),
                new PayInfoItem("账户余额：", Convert.ToDecimal(patientInfo?.balance).In元()),
                new PayInfoItem("应付：", Convert.ToDecimal(registerModel.Res获取挂号应付金额.data.discountFee).In元(), true)
            };
            MultipartData = new MultipartListData
            {
                MidList = payInfoItems
            };
            Hint = "请确认挂号信息";
            ShowContent = true;
            Background = new ImageBrush(ResourceEngine.GetImageResource("Main"));
        }

        private void OnToInfo()
        {
            var authModel = GetInstance<IAuthModel>();
            var patientInfo = authModel.当前就诊人信息;

            var payInfoItems = new List<PayInfoItem>
            {
                new PayInfoItem("姓名：", patientInfo.patientName),
                new PayInfoItem("性别：", patientInfo.sex == 1 ? "男" : "女"),
                new PayInfoItem("身份证号：", patientInfo.idNo.Mask(6, 8)),
                new PayInfoItem("卡号：", authModel.CardNo),
                new PayInfoItem("账户余额：", Convert.ToDecimal(patientInfo.balance).In元(), true)
            };

            MultipartData = new MultipartListData
            {
                MidList = payInfoItems
            };
            Hint = "请确认个人信息";
            ShowContent = true;
            Background = new ImageBrush(ResourceEngine.GetImageResource("Main"));
        }

        #region Bindings

        public string MainTitle { get; set; } = "「区域诊疗一号通」";

        public ImageSource MainLogo => ResourceEngine.GetImageResource("MainLogo");

        private MultipartData _multipartData;

        public MultipartData MultipartData
        {
            get => _multipartData;
            set
            {
                _multipartData = value;
                OnPropertyChanged();
            }
        }

        private ImageBrush _background;

        public ImageBrush Background
        {
            get => _background;
            set
            {
                _background = value;
                OnPropertyChanged();
            }
        }

        private bool _showContent;

        public bool ShowContent
        {
            get => _showContent;
            set
            {
                _showContent = value;
                OnPropertyChanged();
            }
        }

        private string _hint;

        public string Hint
        {
            get => _hint;
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        private DateTime _now;

        public DateTime Now
        {
            get => _now;
            set
            {
                _now = value;
                OnPropertyChanged();
            }
        }

        #endregion Bindings
    }

    public class MultipartTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var data = item as MultipartData;
            if (data == null)
                return null;
            return (container as FrameworkElement).FindResource(data.DataTemplate) as DataTemplate;
        }
    }
}