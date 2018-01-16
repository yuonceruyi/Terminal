using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Navigating;


namespace YuanTu.YanTaiYDYY.Component.ViewModels
{
    public class ChoiceViewModel : YuanTu.Default.Component.ViewModels.ChoiceViewModel
    {
        protected override void Do(ChoiceButtonInfo param)
        {
            //Result result;
            var choiceModel = GetInstance<IChoiceModel>();
            choiceModel.Business = param.ButtonBusiness;
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var engine = NavigationEngine;

            switch (param.ButtonBusiness)
            {
                case Business.建档:
                    var config = GetInstance<IConfigurationManager>();
                    if (config.GetValue("SelectCreateType") == "1")
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Select),
                            CreateJump,
                            new FormContext(InnerA.JDChoneZhi_Context, InnerA.JDCZ.RechargeWay), param.Name);
                    else
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.IDCard),
                            CreateJump,
                            new FormContext(InnerA.JDChoneZhi_Context, InnerA.JDCZ.RechargeWay), param.Name);
                    break;

                case Business.挂号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                        RegisterJump,
                        new FormContext(A.XianChang_Context, A.XC.Dept), param.Name);
                    break;

                case Business.预约:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card), AppointJump,
                        new FormContext(A.YuYue_Context, A.YY.Date), param.Name);
                    break;

                case Business.取号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card), TakeNumJump,
                        new FormContext(A.QuHao_Context, A.QH.Record), param.Name);
                    break;

                case Business.缴费:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card), BillPayJump,
                        new FormContext(A.JiaoFei_Context, A.JF.BillRecord), param.Name);
                    break;

                case Business.充值:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                        RechargeJump,
                        new FormContext(A.ChongZhi_Context, A.CZ.RechargeWay), param.Name);
                    break;

                case Business.查询:
                    Navigate(A.QueryChoice);

                    break;

                case Business.住院押金:
                    OnInRecharge(param);
                    break;

                case Business.外院卡注册:
                    {
                        engine.JumpAfterFlow(null,
                                  CreateJump,
                                  new FormContext(InnerA.WaiYuanCard_Contenxt, InnerA.WYC.WYCard), param.Name);
                        return;
                    }

                case Business.实名认证:
                    engine.JumpAfterFlow(null,
                                RealAuthJump,
                                new FormContext(A.RealAuth_Context, A.SMRZ.Card), param.Name);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        protected override Task<Result<FormContext>> CreateJump()
        {
            var camera = GetInstance<ICameraService>();
            camera.SnapShot("主界面 建档发卡");
            Thread.Sleep(100);
            return Task.Run(() => Result<FormContext>.Success(default(FormContext)));
        }

        protected override Task<Result<FormContext>> RegisterJump()
        {
            return Task.Run(() => Result<FormContext>.Success(default(FormContext)));
        }

        protected override Task<Result<FormContext>> AppointJump()
        {
            var patientModel = GetInstance<IPatientModel>();

            //临时卡不允许预约
            if (!string.IsNullOrWhiteSpace(patientModel.Res病人信息查询?.data[patientModel.PatientInfoIndex]?.extend)
                && patientModel.Res病人信息查询?.data[patientModel.PatientInfoIndex]?.extend == "1")
            {
                ShowAlert(false, "自助预约", "临时卡不允许预约，请到人工窗口补充身份信息\r\n");
                return Task.Run(() => Result<FormContext>.Fail(""));
            }
            else
            {
                return Task.Run(() => Result<FormContext>.Success(default(FormContext)));
            }
        }

        protected override Task<Result<FormContext>> TakeNumJump()
        {
            var patientModel = GetInstance<IPatientModel>();
            var recordModel = GetInstance<IAppoRecordModel>();
            var cardModel = GetInstance<ICardModel>();
            var takeNumModel = GetInstance<ITakeNumModel>();
            var lp = GetInstance<LoadingProcesser>();
            lp.ChangeText("正在查询预约记录信息，请稍候...");
            recordModel.Req挂号预约记录查询 = new req挂号预约记录查询
            {
                patientId = patientModel.当前病人信息?.patientId,
                patientName = patientModel.当前病人信息?.name,
                startDate = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                endDate = DateTimeCore.Now.AddDays(7).ToString("yyyy-MM-dd"),
                searchType = "1",
                cardNo = cardModel.CardNo,
                cardType = ((int)cardModel.CardType).ToString()
            };

            recordModel.Res挂号预约记录查询 = DataHandlerEx.挂号预约记录查询(recordModel.Req挂号预约记录查询);
            if (recordModel.Res挂号预约记录查询?.success ?? false)
            {
                if (recordModel.Res挂号预约记录查询?.data?.Count > 1)
                {
                    return Task.Run(() => Result<FormContext>.Success(default(FormContext)));
                }
                if (recordModel.Res挂号预约记录查询?.data?.Count == 1)
                {
                    recordModel.所选记录 = recordModel.Res挂号预约记录查询.data.FirstOrDefault();
                    var record = recordModel.所选记录;

                    takeNumModel.List = new List<PayInfoItem>
                        {
                            new PayInfoItem("就诊日期：", record.medDate.SafeConvertToDate("yyyy-MM-dd","yyyy年MM月dd日")),
                            new PayInfoItem("就诊科室：", record.deptName),
                            new PayInfoItem("就诊医生：", record.doctName),
                            new PayInfoItem("就诊时段：", record.medAmPm.SafeToAmPm()),
                            new PayInfoItem("就诊序号：", record.appoNo),
                            new PayInfoItem("诊察金额：", record.regAmount.In元(), true),
                            //new PayInfoItem("预约来源：", record.regAmount.In元(), true)
                        };
                    return Task.Run(() => Result<FormContext>.Success(new FormContext(A.QuHao_Context, A.QH.TakeNum)));
                }
                ShowAlert(false, "预约记录查询", "没有获得预约记录信息(列表为空)");
                return Task.Run(() => Result<FormContext>.Fail(""));
            }
            ShowAlert(false, "预约记录查询", "没有获得预约记录信息", debugInfo: recordModel.Res挂号预约记录查询?.msg);
            return Task.Run(() => Result<FormContext>.Fail(""));
        }
        protected override Task<Result<FormContext>> BillPayJump()
        {
            var billRecordModel = GetInstance<IBillRecordModel>();
            var patientModel = GetInstance<IPatientModel>();
            var cardModel = GetInstance<ICardModel>();
            var lp = GetInstance<LoadingProcesser>();
            lp.ChangeText("正在查询待缴费记录信息，请稍候...");
            billRecordModel.Req获取缴费概要信息 = new req获取缴费概要信息
            {
                patientId = patientModel.Res病人信息查询?.data[patientModel.PatientInfoIndex].patientId,
                cardType = ((int)cardModel.CardType).ToString(),
                cardNo = cardModel.CardNo,
                billType = ""
            };
            billRecordModel.Res获取缴费概要信息 = DataHandlerEx.获取缴费概要信息(billRecordModel.Req获取缴费概要信息);
            if (billRecordModel.Res获取缴费概要信息?.success ?? false)
            {
                if (billRecordModel.Res获取缴费概要信息?.data?.Count > 0)
                {
                    return Task.Run(() => Result<FormContext>.Success(default(FormContext)));
                }
                ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息(列表为空)");
                return Task.Run(() => Result<FormContext>.Fail(""));
            }
            ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息", debugInfo: billRecordModel.Res获取缴费概要信息?.msg);
            return Task.Run(() => Result<FormContext>.Fail(""));
        }
        protected override Task<Result<FormContext>> RechargeJump()
        {
            var camera = GetInstance<ICameraService>();
            camera.SnapShot("主界面 预缴金充值");
            Thread.Sleep(100);
            return Task.Run(() => Result<FormContext>.Success(null));
        }
        protected override Task<Result<FormContext>> IpRechargeJump()
        {
            var camera = GetInstance<ICameraService>();
            camera.SnapShot("主界面 住院预缴金充值");
            var patientModel = GetInstance<IPatientModel>();
            if (patientModel.住院患者信息.status == "入院")
            {
                return Task.Run(() => Result<FormContext>.Success(default(FormContext)));
            }
            else
            {
                ShowAlert(false, "住院缴押金", "出院患者不能缴押金");
                return Task.Run(() => Result<FormContext>.Fail(""));
            }
        }
    }
}
