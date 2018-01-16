using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Component.TakeNum.ViewModels
{
    public class ApptRecordViewModel : ViewModelBase
    {
        public override string Title => "选择预约记录";

        [Dependency]
        public IAppoRecordModel RecordModel { get; set; }

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public ITakeNumModel TakeNumModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public ICancelAppoModel CancelAppoModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = RecordModel.Res挂号预约记录查询.data.Select(p => new InfoAppt
            {
                Date = $"{p.medDate.SafeConvertToDate("yyyy-MM-dd","yyyy年MM月dd日")}",
                Time = $"{p.medAmPm.SafeToAmPm()}",
                Department = $"{p.deptName.SafeSubstring(0, 7)}",
                Doctor = $"{p.doctName}",
                ConfirmCommand = confirmCommand,
                Tag = p,
                IsEnabled = p.status == "0"
            });
            Data = new ObservableCollection<InfoAppt>(list);
            
            
            PlaySound(SoundMapping.选择预约记录);
        }

        protected virtual void Confirm(Info i)
        {
            RecordModel.所选记录 = i.Tag.As<挂号预约记录>();
            ChangeNavigationContent(RecordModel.所选记录.doctName);
            var record = RecordModel.所选记录;

            TakeNumModel.List = new List<PayInfoItem>
            {
                new PayInfoItem("就诊日期：", record.medDate.SafeConvertToDate("yyyy-MM-dd","yyyy年MM月dd日")),
                new PayInfoItem("就诊科室：", record.deptName),
                new PayInfoItem("就诊医生：", record.doctName),
                new PayInfoItem("就诊时段：", $"{record.medAmPm.SafeToAmPm()} {record.medTime}"),
                new PayInfoItem("就诊序号：", record.appoNo),
                new PayInfoItem("挂号金额：", record.regAmount.In元(), true)
            };

            Next();
        }

        #region Binding

        private ObservableCollection<InfoAppt> _data;

        public ObservableCollection<InfoAppt> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding
    }
}