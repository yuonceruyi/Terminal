using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;

namespace YuanTu.NanYangFirstPeopleHospital.Component.ViewModels
{
    internal class ChoiceViewModel : Default.Component.ViewModels.ChoiceViewModel
    {
        protected override Task<Result<FormContext>> RegisterJump()
        {
            
            return DoCommand(lp =>
            {
                var deptartmentModel = GetInstance<IDeptartmentModel>();
                var choiceModel = GetInstance<IChoiceModel>();
                var regDateModel = GetInstance<IRegDateModel>();
                lp.ChangeText("正在查询排班科室，请稍候...");
                deptartmentModel.排班科室信息查询 = new req排班科室信息查询
                {
                    regMode = choiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = null,//不选挂号类型
                    startDate =
                        choiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : regDateModel.RegDate,
                    endDate =
                        choiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : regDateModel.RegDate
                };
                deptartmentModel.Res排班科室信息查询 = DataHandlerEx.排班科室信息查询(deptartmentModel.排班科室信息查询);
                if (deptartmentModel.Res排班科室信息查询?.success ?? false)
                {
                    if (deptartmentModel.Res排班科室信息查询?.data?.Count > 0)
                    {
                        return Result<FormContext>.Success(default(FormContext));
                    }

                    ShowAlert(false, "科室列表查询", "没有获得科室信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }

                ShowAlert(false, "科室列表查询", "没有获得科室信息", debugInfo: deptartmentModel.Res排班科室信息查询?.msg);
                return Result<FormContext>.Fail("");
            });
        }

        protected override void Do(ChoiceButtonInfo param)
        {
            var choiceModel = GetInstance<IChoiceModel>();
            choiceModel.Business = param.ButtonBusiness;
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var engine = NavigationEngine;

            var result = CheckReceiptPrinter();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "打印机检测", result.Message);
                return;
            }

            switch (param.ButtonBusiness)
            {
                case Business.建档:
                  
                    var config = GetInstance<IConfigurationManager>();
                    if (config.GetValue("SelectCreateType") == "1")
                        engine.JumpAfterFlow(new FormContext (A.ChaKa_Context,A.CK.Select ),
                            CreateJump,
                            new FormContext (A.JianDang_Context,A.JD.Print ), param.Name);
                    else
                        engine.JumpAfterFlow(new FormContext (A.ChaKa_Context,A.CK.Choice ),
                            CreateJump,
                            new FormContext (A.JianDang_Context,A.JD.Print ), param.Name);
                    break;

                case Business.挂号:
                    engine.JumpAfterFlow(new FormContext (A.ChaKa_Context,A.CK.Card ),
                        RegisterJump,
                        new FormContext (A.XianChang_Context,A.XC.Dept ), param.Name);
                    break;

                case Business.预约:
                    engine.JumpAfterFlow(new FormContext (A.ChaKa_Context,A.CK.Card), AppointJump,
                        new FormContext (A.YuYue_Context,A.YY.Date ), param.Name);
                    break;

                case Business.取号:
                    engine.JumpAfterFlow(new FormContext (A.ChaKa_Context,A.CK.Card), TakeNumJump,
                        new FormContext (A.QuHao_Context,A.QH.Record ), param.Name);
                    break;

                case Business.缴费:
                    engine.JumpAfterFlow(new FormContext (A.ChaKa_Context,A.CK.Card), BillPayJump,
                        new FormContext (A.JiaoFei_Context,A.JF.BillRecord ), param.Name);
                    break;

                case Business.充值:
                    engine.JumpAfterFlow(new FormContext (A.ChaKa_Context,A.CK.Card),
                        RechargeJump,
                        new FormContext (A.ChongZhi_Context,A.CZ.RechargeWay ), param.Name);
                    break;

                case Business.查询:
                    Navigate(A.QueryChoice);

                    break;
                case Business.住院押金:
                    //choiceModel.HasAuthFlow = false;
                    engine.JumpAfterFlow(new FormContext (A.ZhuYuan_Context,A.ZY.Card ),
                       IpRechargeJump,
                       new FormContext (A.IpRecharge_Context,A.ZYCZ.RechargeWay ), param.Name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override Task<Result<FormContext>> TakeNumJump()
        {
          
            return DoCommand(lp =>
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
                        return Result<FormContext>.Success(default(FormContext));
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
                            new PayInfoItem("预约单号：", record.appoNo),
                            new PayInfoItem("挂号金额：", record.regAmount.In元(), true)
                        };
                        return
                            Result<FormContext>.Success(new FormContext(A.QuHao_Context,A.QH.TakeNum));
                    }
                    ShowAlert(false, "预约记录查询", "没有获得预约记录信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }
                ShowAlert(false, "预约记录查询", "没有获得预约记录信息");
                return Result<FormContext>.Fail("");
            });
        }

        protected override Task<Result<FormContext>> IpRechargeJump()
        {
            return null;
        }
    }
}