using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.YuHangSecondHospital.Component.TakeNum.Models;

namespace YuanTu.YuHangSecondHospital.Component.TakeNum.ViewModels
{
    public class TakeNumDateViewModel : ViewModelBase
    {
        protected readonly string[] Week = { "日", "一", "二", "三", "四", "五", "六" };
        public override string Title => "请触摸下方卡片选择预约日期";
        protected virtual int AppointingDays { get; set; } = 8;
        protected int AppointingStartOffset { get; set; } = 0;

        [Dependency]
        public IPreTakeNumModel PreTakeNumModel { get; set; }

        protected string DayOfWeek(DateTime date)
        {
            return "星期" + Week[Convert.ToInt32(date.DayOfWeek)];
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var list = new InfoMore[AppointingDays];

            for (var i = 0; i < AppointingDays; i++)
            {
                var date = DateTimeCore.Today.AddDays(i + AppointingStartOffset);
                list.SetValue(new InfoMore
                {
                    Title = date.ToString("yyyy-MM-dd"),
                    SubTitle = DayOfWeek(date),
                    Amount = null,
                    ConfirmCommand = confirmCommand,
                    Tag = i
                }, i);
            }
            Data = new ObservableCollection<InfoMore>(list);

            PlaySound(SoundMapping.选择预约日期);
        }

        protected virtual void Confirm(Info i)
        {
            PreTakeNumModel.TakeNumDate = i.Title;
            ChangeNavigationContent(i.Title);

            DoCommand(lp =>
            {
                var patientModel = GetInstance<IPatientModel>();
                var recordModel = GetInstance<IAppoRecordModel>();
                var cardModel = GetInstance<ICardModel>();
                var takeNumModel = GetInstance<ITakeNumModel>();
                lp.ChangeText("正在查询预约记录，请稍候...");
                recordModel.Req挂号预约记录查询 = new req挂号预约记录查询
                {
                    patientId = patientModel.当前病人信息?.patientId,
                    patientName = patientModel.当前病人信息?.name,
                    startDate = PreTakeNumModel.TakeNumDate,
                    endDate = PreTakeNumModel.TakeNumDate,
                    searchType = "1",
                    cardNo = cardModel.CardNo,
                    cardType = ((int)cardModel.CardType).ToString()
                };
                recordModel.Res挂号预约记录查询 = DataHandlerEx.挂号预约记录查询(recordModel.Req挂号预约记录查询);
                if (recordModel.Res挂号预约记录查询?.success ?? false)
                {
                    if (recordModel.Res挂号预约记录查询?.data?.Count > 1)
                    {
                        Next();
                        return;
                    }
                    if (recordModel.Res挂号预约记录查询?.data?.Count == 1)
                    {
                        recordModel.所选记录 = recordModel.Res挂号预约记录查询.data.FirstOrDefault();
                        var record = recordModel.所选记录;

                        takeNumModel.List = new List<PayInfoItem>
                        {
                            new PayInfoItem("就诊日期：", record?.medDate.SafeConvertToDate("yyyy-MM-dd", "yyyy年MM月dd日")),
                            new PayInfoItem("就诊科室：", record?.deptName),
                            new PayInfoItem("就诊医生：", record?.doctName),
                            new PayInfoItem("就诊时段：", record?.medAmPm.SafeToAmPm()),
                            new PayInfoItem("就诊序号：", record?.appoNo),
                            new PayInfoItem("挂号金额：", record?.regAmount.In元(), true)
                        };
                        Navigate(A.QH.TakeNum);
                        return;
                    }
                    ShowAlert(false, "预约记录查询", "没有获得预约记录信息(列表为空)");
                    return;
                }
                ShowAlert(false, "预约记录查询", "没有获得预约记录信息", debugInfo: recordModel.Res挂号预约记录查询?.msg);
            });
        }

        #region Binding

        private ObservableCollection<InfoMore> _data;

        public ObservableCollection<InfoMore> Data
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