using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Navigating;
using YuanTu.ZheJiangHospital.Component.Appoint;
using YuanTu.ZheJiangHospital.Component.Auth.Models;
using YuanTu.ZheJiangHospital.Component.BillPay.Models;
using YuanTu.ZheJiangHospital.Component.Recharge.Models;
using YuanTu.ZheJiangHospital.Component.TakeNum.Models;
using YuanTu.ZheJiangHospital.HIS;

namespace YuanTu.ZheJiangHospital.Component.ViewModels
{
    public class ChoiceViewModel : Default.Component.ViewModels.ChoiceViewModel
    {
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
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Select),
                            CreateJump,
                            new FormContext(null, A.Home), param.Name);
                    else
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                            CreateJump,
                            new FormContext(null, A.Home), param.Name);
                    break;

                case Business.挂号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        RegisterJump,
                        new FormContext(A.XianChang_Context, A.XC.Wether), param.Name);
                    break;

                case Business.预约:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        AppointJump,
                        new FormContext(A.YuYue_Context, A.YY.Dept), param.Name);
                    break;

                case Business.取号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        TakeNumJump,
                        new FormContext(A.QuHao_Context, A.QH.Record), param.Name);
                    break;

                case Business.缴费:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        BillPayJump,
                        new FormContext(A.JiaoFei_Context, A.JF.BillRecord), param.Name);
                    break;

                case Business.充值:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        RechargeJump,
                        new FormContext(A.ChongZhi_Context, A.CZ.RechargeWay), param.Name);
                    break;

                case Business.查询:
                    Navigate(A.QueryChoice);

                    break;

                case Business.住院押金:
                    OnInRecharge(param);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region JumpAction
        protected override Task<Result<FormContext>> CreateJump()
        {
            Navigate(A.Home);
            return Task.FromResult(
                Result<FormContext>.Fail(string.Empty));
            //return Task.FromResult(
            //    Result<FormContext>.Success(
            //        new FormContext(null, A.Home)));
        }

        protected override Task<Result<FormContext>> AppointJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 预约挂号");

                var appoint = GetInstance<IAppointModel>();

                var req = new Req科室列表查询();
                var result = AppointService.Run<Res科室列表查询, Req科室列表查询>(req);
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "预约科室查询", "没有获得预约科室信息", debugInfo: result.Message);
                    return Result<FormContext>.Fail("");
                }
                var res = result.Value;
                appoint.科室信息List = res.list;

                return Result<FormContext>.Success(null);
            });
        }

        protected override Task<Result<FormContext>> TakeNumJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 预约取号");

                lp.ChangeText("正在查询预约记录，请稍候...");

                var info = GetInstance<IAuthModel>();
                var takeNum = GetInstance<ITakeNumModel>();
                var paymentModel = GetInstance<IPaymentModel>();
                var res = DataHandler.GetAppointRecord(info.Info.IDNO);
                if (!res.IsSuccess)
                {
                    ShowAlert(false, "预约记录查询", "没有获得预约记录信息", debugInfo: res.Message);
                    return Result<FormContext>.Fail("");
                }
                var list = res.Value;
                if (!list.Any())
                {
                    ShowAlert(false, "预约记录查询", "没有获得预约记录信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }
                var today = DateTimeCore.Today;
                var valid = list.Where(i => i.STATUS == 0 && i.MEDDATE == today).ToList();
                takeNum.Records = valid;
                if (valid.Count == 0)
                {
                    ShowAlert(false, "预约记录查询", "没有获得今天可以取号的预约记录信息(列表为空)");
                    return Result<FormContext>.Success(default(FormContext));
                }
                if (valid.Count > 1)
                {
                    ShowAlert(true, "预约记录提示", "由于HIS系统原因\n存在多条有效预约记录时\n取得的号不保证是选的\n可以通过多次取号取得所有的预约号");
                    return Result<FormContext>.Success(default(FormContext));
                }

                var record = valid.First();
                takeNum.Record = record;

                //takeNum.List = new List<PayInfoItem>
                //{
                //    new PayInfoItem("就诊日期：", record.MEDDATE.ToString("yyyy年MM月dd日")),
                //    new PayInfoItem("就诊科室：", record.DEPTNAME),
                //    new PayInfoItem("就诊医生：", record.DOCTNAME),
                //    new PayInfoItem("就诊时段：", record.MEDAMPM.SafeToAmPm()+" "+record.MEDTIME),
                //    new PayInfoItem("就诊序号：", record.APPONO.ToString()),
                //    new PayInfoItem("挂号金额：", record.REGAMOUNT.In元(), true)
                //};
                //return Result<FormContext>.Success(new FormContext(A.QuHao_Context, A.QH.TakeNum));

                paymentModel.Self = record.REGAMOUNT * 100m;
                paymentModel.Insurance = 0;
                paymentModel.Total = record.REGAMOUNT * 100m;
                paymentModel.NoPay = true;
                paymentModel.ConfirmAction = () => Confirm(this);

                paymentModel.LeftList = new List<PayInfoItem>
                {
                    new PayInfoItem("日期：", $"{record.MEDDATE:MM月dd日}"),
                    new PayInfoItem("时间：", record.MEDAMPM.SafeToAmPm() + " " + record.MEDTIME),
                    new PayInfoItem("科室：", record.DEPTNAME),
                    new PayInfoItem("医生：", record.DOCTNAME)
                };

                paymentModel.RightList = new List<PayInfoItem>
                {
                    new PayInfoItem("个人支付：", paymentModel.Self.In元()),
                    new PayInfoItem("医保报销：", paymentModel.Insurance.In元()),
                    new PayInfoItem("支付金额：", paymentModel.Total.In元(), true)
                };

                return Result<FormContext>.Success(new FormContext(A.QuHao_Context, A.QH.Confirm));
            });
        }

        public static Result Confirm(ViewModelBase vm)
        {
            return vm.DoCommand(lp =>
                {
                    lp.ChangeText("正在进行取号，请稍候...");

                    // TODO 参数来源
                    var auth = GetInstance<IAuthModel>();
                    var cardModel = GetInstance<ICardModel>();
                    var req = new Req取预约号
                    {
                        身份证号 = auth.Info.IDNO
                    };

                    switch (cardModel.CardType)
                    {
                        case CardType.身份证:
                            req.医保类别 = "1";
                            break;

                        case CardType.市医保卡:
                            req.医保类别 = "2";
                            break;

                        case CardType.省医保卡:
                            req.医保类别 = "3";
                            break;
                    }

                    var res = DataHandler.RunExe<Res取预约号>(req);
                    if (res.Success)
                    {
                        var printModel = GetInstance<IPrintModel>();
                        var configurationManager = GetInstance<IConfigurationManager>();
                        printModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = "取号成功",
                            TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分取号",
                            PrinterName = configurationManager.GetValue("Printer:Receipt"),
                            Printables = TakeNumPrintables(res),
                            TipImage = "提示_凭条"
                        });

                        vm.NavigationEngine.Navigate(A.QH.Print);
                        return Result.Success();
                    }
                    vm.ShowAlert(false, "预约取号失败", $"预约取号失败:{res.Message}");

                    return Result.Fail(res.Message);
                })
                .Result;
        }

        private static Queue<IPrintable> TakeNumPrintables(Res取预约号 res)
        {
            var printManager = GetInstance<IPrintManager>();
            var queue = printManager.NewQueue("取号单");
            var sb = new StringBuilder();
            sb.Append($"状态：取号成功\n");
            sb.Append($"姓名：{res.病人姓名}\n");
            sb.Append($"门诊号：{res.门诊号码}\n");
            sb.Append($"交易类型：现场挂号\n");
            sb.Append($"科室名称：{res.挂号科室}\n");
            sb.Append($"就诊医生：{res.挂号医生}\n");
            sb.Append($"就诊时间：{DateTimeCore.Today:yyyy-MM-dd}\n");
            sb.Append($"就诊场次：{res.挂号班别.SafeToAmPm()}\n");
            //sb.Append($"就诊地址：{res.就诊地址}\n");
            sb.Append($"就诊序号：{res.就诊序号}\n");
            sb.Append($"交易时间：{res.挂号日期}\n");
            sb.Append($"挂号费总额：{res.挂号费总额}\n");
            sb.Append($"账户支付：{res.病人账户支付}\n");
            sb.Append($"医保支付：{res.医保支付}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

        protected override Task<Result<FormContext>> BillPayJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 结算");
                var auth = GetInstance<IAuthModel>();
                lp.ChangeText("正在查询待缴费信息，请稍候...");
                var res = DataHandler.GetToPayRecord(auth.Info.PATIENTID);
                if (!res.IsSuccess)
                {
                    ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息", debugInfo: res.Message);
                    return Result<FormContext>.Fail("");
                }
                var records = res.Value;
                if (!records.Any())
                {
                    ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }
                var billPay = GetInstance<IBillPayModel>();
                foreach (var record in records)
                {
                    record.YPDJ *= 100m;
                    record.HJJE *= 100m;
                }
                billPay.Records = records;
                billPay.Total = records.Sum(r => r.HJJE);

                return Result<FormContext>.Success(default(FormContext));
            });
        }
        protected override Task<Result<FormContext>> RechargeJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 预缴金充值");
                var account = GetInstance<IAccountModel>();
                if (!account.HasAccount)
                {
                    ShowAlert(false, "余额查询", "未开通账户");
                    return Result<FormContext>.Fail("");
                }
                if (account.Balance < 0)
                {
                    ShowAlert(false, "余额查询", "余额查询失败");
                    return Result<FormContext>.Fail("");
                }
                Thread.Sleep(100);
                return Result<FormContext>.Success(null);
            });
        }

        #endregion JumpAction
    }
}