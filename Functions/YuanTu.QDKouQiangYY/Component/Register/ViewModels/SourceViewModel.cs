using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System.Collections.ObjectModel;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.QDArea;
using YuanTu.QDArea.Enums;
using YuanTu.QDArea.Models.Register;
using YuanTu.QDArea.Models.TakeNum;

namespace YuanTu.QDKouQiangYY.Component.Register.ViewModels
{
    public class SourceViewModel : YuanTu.Default.Component.Register.ViewModels.SourceViewModel
    {
        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public IDeptartmentModel DepartmentModel { get; set; }

        [Dependency]
        public IDoctorModel DoctorModel { get; set; }

        [Dependency]
        public IRegLockExtendModel RegLockExtendModel { get; set; }

        [Dependency]
        public IRegisterModel RegisterModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IRegUnLockExtendModel RegUnLockExtendModel { get; set; }

        [Dependency]
        public IRegTypesModel RegTypesModel { get; set; }

        [Dependency]
        public IAppoRecordModel RecordModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            #region 解除锁号
            if (ConfigQD.ScheduleVersion == "1")
            {
                //查询预约记录
                var patientModel = GetInstance<IPatientModel>();
                var cardModel = GetInstance<ICardModel>();
                var takeNumExtendModel = GetInstance<ITakeNumExtendModel>();

                takeNumExtendModel.version = ConfigQD.ScheduleVersion;

                DoCommand(lp =>
                {
                    lp.ChangeText("正在查询锁号记录，请稍候...");
                    RecordModel.Req挂号预约记录查询 = new req挂号预约记录查询
                    {
                        patientId = patientModel.当前病人信息?.patientId,
                        patientName = patientModel.当前病人信息?.name,
                        startDate = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                        endDate = DateTimeCore.Now.AddDays(7).ToString("yyyy-MM-dd"),
                        searchType = "1",//1、预约 2、挂号 3 加号4 诊间加号
                        cardNo = cardModel.CardNo,
                        cardType = ((int)cardModel.CardType).ToString(),
                        status = "101",//锁号状态的
                        appoNo = "",//传空
                        regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                        extend = takeNumExtendModel.ToJsonString(),
                    };
                    RecordModel.Res挂号预约记录查询 = DataHandlerEx.挂号预约记录查询(RecordModel.Req挂号预约记录查询);
                    if (RecordModel.Res挂号预约记录查询?.success ?? false)
                    {
                        if (RecordModel.Res挂号预约记录查询?.data.Where(p => p.status == "101").ToList().Count > 0)
                        {
                            lp.ChangeText("正在解除锁号记录，请稍候...");

                            RegUnLockExtendModel.version = ConfigQD.ScheduleVersion;
                            foreach (var obj in RecordModel.Res挂号预约记录查询.data.Where(obj => !obj.lockId.IsNullOrWhiteSpace() && obj.status == "101"))
                            {
                                RegisterModel.Req挂号解锁 = new req挂号解锁
                                {
                                    lockId = obj.lockId,
                                    extend = RegUnLockExtendModel.ToJsonString(),
                                };
                                RegisterModel.Res挂号解锁 = DataHandlerEx.挂号解锁(RegisterModel.Req挂号解锁);
                                //不处理返回值
                            }
                        }

                    }
                });
            }
            #endregion
        }
        protected override void Confirm(Info i)
        {
            SourceModel.所选号源 = i.Tag.As<号源明细>();
            ChangeNavigationContent(i.Title);

            #region 锁号

            if (ConfigQD.ScheduleVersion == "1")
            {
                DoCommand(lp =>
                {
                    lp.ChangeText("正在锁号，请稍候...");
                    var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                    var scheduleInfo = ScheduleModel.所选排班;
                    var deptInfo = DepartmentModel.所选科室;
                    var doctInfo = DoctorModel.所选医生;

                    RegLockExtendModel.appoNo = SourceModel?.所选号源?.appoNo;
                    RegLockExtendModel.version = ConfigQD.ScheduleVersion;
                    RegLockExtendModel.regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1";
                    RegLockExtendModel.phone = patientInfo.phone;

                    RegisterModel.Req挂号锁号 = new req挂号锁号
                    {
                        cardNo = CardModel.CardNo,
                        cardType = ((int)CardModel.CardType).ToString(),
                        patientId = patientInfo.patientId,
                        regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                        medDate = scheduleInfo.medDate,
                        scheduleId = scheduleInfo.scheduleId,
                        deptCode = deptInfo.deptCode,
                        doctCode = doctInfo?.doctCode,
                        medAmPm = scheduleInfo.medAmPm,
                        extend = RegLockExtendModel.ToJsonString(),
                    };

                    RegisterModel.Res挂号锁号 = DataHandlerEx.挂号锁号(RegisterModel.Req挂号锁号);
                    if (!RegisterModel.Res挂号锁号?.success ?? false)
                    {
                        ShowAlert(false, $"{ChoiceModel.Business}锁号", $"{ChoiceModel.Business}锁号失败\r\n原因：{RegisterModel.Res挂号锁号.msg}", debugInfo: $"{ RegisterModel.Res挂号锁号.msg}");
                        return;
                    }

                    //强制确定按钮显示
                    if (ChoiceModel.Business == Business.预约 &&
                        RegisterModel.Res挂号锁号?.data?.appointMode == ((int)ApptChargeMode.用户可选收费).ToString())
                    {
                        PaymentModel.NoPay = true;
                    }
                });
            }
            Next();
            #endregion
        }
    }
}
