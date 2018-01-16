using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Recharge;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Default.Component.ViewModels;
using SerialPrinter = YuanTu.Devices.Printer;

namespace YuanTu.Default.Component.ZYRecharge.ViewModels
{
    public class MethodViewModel : ViewModelBase
    {
        [Dependency]
        public IIpRechargeModel IpRechargeModel { get; set; }

        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        public override string Title => "选择缴押金方式";

        #region Overrides of ViewModelBase

        /// <summary>
        ///     仅当在实例化试调用
        /// </summary>
        public override void OnSet()
        {
            var list = PayMethodDto.GetInfoPays(
                GetInstance<IConfigurationManager>(), 
                ResourceEngine, 
                "RechargeZY", 
                new DelegateCommand<Info>(OnPayButtonClick));

            Data = new ObservableCollection<InfoIcon>(list);

            PlaySound(SoundMapping.选择充值方式);
        }

        #endregion Overrides of ViewModelBase

        public override void OnEntered(NavigationContext navigationContext)
        {
        }

        protected virtual void OnPayButtonClick(Info i)
        {
            var payMethod = (PayMethod)i.Tag;
            IpRechargeModel.RechargeMethod = payMethod;
            ExtraPaymentModel.Complete = false;
            ExtraPaymentModel.CurrentBusiness = Business.住院押金;
            ExtraPaymentModel.CurrentPayMethod = payMethod;
            ExtraPaymentModel.FinishFunc = OnRechargeCallback;
            //准备住院充值所需病人信息
            ExtraPaymentModel.PatientInfo = new PatientInfo
            {
                Name = PatientModel.住院患者信息.name,
                PatientId = PatientModel.住院患者信息.patientHosId,
                IdNo = PatientModel.住院患者信息.idNo,
                GuardianNo = PatientModel.住院患者信息.guardianNo,
                CardNo = PatientModel.住院患者信息.patientHosId,
                Remain = decimal.Parse(PatientModel.住院患者信息.accBalance ?? "0")
            };

            ChangeNavigationContent(IpRechargeModel.RechargeMethod.ToString());
            switch (payMethod)
            {
                case PayMethod.未知:
                case PayMethod.预缴金:
                case PayMethod.社保:
                    throw new ArgumentOutOfRangeException();
                case PayMethod.现金:
                    OnCAClick();
                    break;

                case PayMethod.银联:
                case PayMethod.支付宝:
                case PayMethod.微信支付:
                case PayMethod.苹果支付:
                    Navigate(A.ZYCZ.InputAmount);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual Task<Result> OnRechargeCallback()
        {
            return DoCommand(p =>
            {
                try
                {
                    p.ChangeText("正在进行充值，请稍候...");
                    IpRechargeModel.Res住院预缴金充值 = null;
                    var patientInfo = PatientModel.住院患者信息;
                    IpRechargeModel.Req住院预缴金充值 = new req住院预缴金充值
                    {
                        patientId = patientInfo.patientHosId,
                        operId = FrameworkConst.OperatorId,
                        cash = ExtraPaymentModel.TotalMoney.ToString(),
                        tradeMode = ExtraPaymentModel.CurrentPayMethod.GetEnumDescription(),
                        accountNo = patientInfo.patientHosId
                    };

                    //填充各种支付方式附加数据
                    FillRechargeRequest(IpRechargeModel.Req住院预缴金充值);

                    IpRechargeModel.Res住院预缴金充值 = DataHandlerEx.住院预缴金充值(IpRechargeModel.Req住院预缴金充值);
                    //ExtraPaymentModel.Complete = true;
                    if (IpRechargeModel.Res住院预缴金充值.success)
                    {
                        ExtraPaymentModel.Complete = true;
                        //PrintModel.SetPrintInfo(true, "缴住院押金成功",
                        //    $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功缴押金{ExtraPaymentModel.TotalMoney.In元()}",
                        //    ConfigurationManager.GetValue("Printer:Receipt"), IpRechargePrintables(true));
                        //PrintModel.SetPrintImage(ResourceEngine.GetImageResourceUri("提示_凭条"));
                        if (FrameworkConst.DeviceType == "YT-740")
                        {
                            SerialPrint(true);
                            PrintModel.SetPrintInfo(true, new PrintInfo
                            {
                                TypeMsg = "缴住院押金成功",
                                TipMsg = $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功缴押金{ExtraPaymentModel.TotalMoney.In元()}",
                                TipImage = "提示_凭条_740"
                            });
                        }
                        else if (FrameworkConst.DeviceType == "YT-BG350")
                        {
                            PrintModel.SetPrintInfo(true, new PrintInfo
                            {
                                TypeMsg = "缴住院押金成功",
                                TipMsg = $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功缴押金{ExtraPaymentModel.TotalMoney.In元()}",
                                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                Printables = IpRechargePrintables(true),
                                TipImage = "提示_凭条"
                            });
                        }
                        else
                        {
                            PrintModel.SetPrintInfo(true, new PrintInfo
                            {
                                TypeMsg = "缴住院押金成功",
                                TipMsg = $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功缴押金{ExtraPaymentModel.TotalMoney.In元()}",
                                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                Printables = IpRechargePrintables(true),
                                TipImage = "提示_凭条"
                            });
                        }
                        patientInfo.accBalance = IpRechargeModel.Res住院预缴金充值.data.cash;
                        ChangeNavigationContent(A.ZY.InPatientInfo, A.ZhuYuan_Context,
                            $"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}");
                        Navigate(A.ZYCZ.Print);
                        return Result.Success();
                    }
                    else
                    {
                        //PrintModel.SetPrintInfo(false, "充值失败",
                        //    $"您于{DateTimeCore.Now.ToString("HH:mm")}分充值{ExtraPaymentModel.TotalMoney.In元()}失败",
                        //    ConfigurationManager.GetValue("Printer:Receipt"), IpRechargePrintables(false),
                        //    IpRechargeModel.Res住院预缴金充值?.msg);
                        if (FrameworkConst.DeviceType == "YT-740")
                        {
                            SerialPrint(false);
                            PrintModel.SetPrintInfo(true, new PrintInfo
                            {
                                TypeMsg = "缴住院押金失败",
                                TipMsg = $"您已于{DateTimeCore.Now.ToString("HH:mm")}分缴押金{ExtraPaymentModel.TotalMoney.In元()}失败",
                                TipImage = "提示_凭条"
                            });
                        }
                        else
                        {
                            PrintModel.SetPrintInfo(false, new PrintInfo
                            {
                                TypeMsg = "充值失败",
                                TipMsg = $"您于{DateTimeCore.Now.ToString("HH:mm")}分充值{ExtraPaymentModel.TotalMoney.In元()}失败",
                                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                Printables = IpRechargePrintables(false),
                                TipImage = "提示_凭条",
                                DebugInfo = IpRechargeModel.Res住院预缴金充值?.msg
                            });
                        }
                        Navigate(A.ZYCZ.Print);
                        ExtraPaymentModel.Complete = true;
                        return Result.Fail(IpRechargeModel.Res住院预缴金充值?.code ?? -100, IpRechargeModel.Res住院预缴金充值.msg);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Main.Error(
                        $"[{ExtraPaymentModel.CurrentPayMethod}充值]发起存钱交易时出现异常，原因:{ex.Message}\r\n{ex.StackTrace}\r\n{ex.InnerException}");
                    ShowAlert(false, "充值失败", "发生系统异常 ");
                    return Result.Fail("系统异常");
                }
                finally
                {
                    DBManager.Insert(new ZYRechargeInfo
                    {
                        PatientId = PatientModel?.住院患者信息?.patientHosId,
                        RechargeMethod = ExtraPaymentModel.CurrentPayMethod,
                        TotalMoney = ExtraPaymentModel.TotalMoney,
                        Success = IpRechargeModel.Res住院预缴金充值?.success ?? false,
                        ErrorMsg = IpRechargeModel.Res住院预缴金充值?.msg
                    });
                }
            });
        }

        protected virtual void FillRechargeRequest(req住院预缴金充值 req)
        {
            if (ExtraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = ExtraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
                    req.bankTime = posinfo.TransTime;
                    req.bankDate = posinfo.TransDate;
                    req.posTransNo = posinfo.Trace;
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.MId;
                }
            }
            else if (ExtraPaymentModel.CurrentPayMethod == PayMethod.支付宝 ||
                     ExtraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = ExtraPaymentModel.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    req.payAccountNo = thirdpayinfo.buyerAccount;
                    req.transNo = thirdpayinfo.outPayNo;
                    req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                }
            }
        }

        protected virtual void OnCAClick()
        {
            Navigate(A.Third.AtmCash);
        }

        protected virtual Queue<IPrintable> IpRechargePrintables(bool success)
        {
            if (!success)
            {
                if (ExtraPaymentModel.CurrentPayMethod != PayMethod.现金) //只有现金才需要打凭条
                {
                    return null;
                }
            }
            var queue = PrintManager.NewQueue("住院押金");
            var patientInfo = PatientModel.住院患者信息;
            var sb = new StringBuilder();
            sb.Append($"状态：缴押金{(success ? "成功" : "失败")}\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"住院号：{patientInfo.patientHosId}\n");
            sb.Append($"缴押金方式：{ExtraPaymentModel.CurrentPayMethod}\n");
            sb.Append($"缴押金前余额：{patientInfo.accBalance.In元()}\n");
            sb.Append($"缴押金金额：{IpRechargeModel.Req住院预缴金充值.cash.In元()}\n");
            if (success)
            {
                sb.Append($"缴押金后余额：{IpRechargeModel.Res住院预缴金充值.data.cash.In元()}\n");
                sb.Append($"收据号：{IpRechargeModel.Res住院预缴金充值.data.receiptNo}\n");
            }
            else
            {
                sb.Append($"异常原因：{IpRechargeModel.Res住院预缴金充值.msg}\n");
            }
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的个人信息。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }
        public virtual void SerialPrint(bool success)
        {
            if (!success)
            {
                if (ExtraPaymentModel.CurrentPayMethod != PayMethod.现金) //只有现金才需要打凭条
                {
                    return;
                }
            }
            SerialPrinter.SerialPrinter printer = new SerialPrinter.SerialPrinter();
            var context = printer.GetContext();
            context[SerialPrinter.PrintableContext.SerialPort] = "COM6";
            var apt = printer.Connect(context);
            if (!apt.Success)
            {
                ShowAlert(false, "打印失败", "连接打印机失败:" + apt.Message);
            }
            printer.SetLeftSpacing(30, 0);
            printer.SetFontSize(2, 2);
            printer.SetAlign(1);//居中
            printer.Text("住院押金").FeedLine();
            printer.SetFontSize(1, 1);
            printer.SetAlign(0);//左侧                
            printer.Text($"状态：缴押金{(success ? "成功" : "失败")}").FeedLine();
            printer.Text($"姓名：{PatientModel.住院患者信息.name}").FeedLine();
            printer.Text($"住院号：{PatientModel.住院患者信息.patientHosId}").FeedLine();
            switch (ExtraPaymentModel.CurrentPayMethod)
            {
                case PayMethod.银联:
                    var posinfo = ExtraPaymentModel.PaymentResult as TransResDto;
                    printer.Text($"缴费方式：银行卡").FeedLine();
                    if (!string.IsNullOrEmpty(posinfo?.Receipt))
                    {
                        printer.Text($"{posinfo?.Receipt}").FeedLine();
                    }
                    else
                    {
                        printer.Text($"银行卡号：{posinfo.CardNo}").FeedLine();
                        printer.Text($"授权号：{posinfo.Auth}").FeedLine();
                        printer.Text($"终端编号：{posinfo.TId}").FeedLine();
                        printer.Text($"终端流水：{posinfo.Trace}").FeedLine();
                        printer.Text($"批次号：{posinfo.Batch}").FeedLine();
                        printer.Text($"检索参考号：{posinfo.Ref}").FeedLine();
                        printer.Text($"交易日期：{posinfo.TransDate}").FeedLine();
                        printer.Text($"交易时间：{posinfo.TransTime}").FeedLine();
                    }
                    break;
                default:
                    printer.Text($"缴费方式：现金").FeedLine();
                    break;
            }

            printer.Text($"缴费前余额：{Convert.ToDecimal(PatientModel.住院患者信息.accBalance).In元()}").FeedLine();
            printer.Text($"缴费金额：{ExtraPaymentModel.TotalMoney.In元()}").FeedLine();
            if (success)
            {
                decimal remain = Convert.ToInt32(PatientModel.住院患者信息.accBalance) + ExtraPaymentModel.TotalMoney;
                printer.Text($"缴费后余额：{remain.In元()}").FeedLine();
            }
            else
            {
                printer.Text($"异常原因：{IpRechargeModel.Res住院预缴金充值.msg}").FeedLine();
            }
            string[] time = IpRechargeModel.Req住院预缴金充值.tradeTime.Split(' ');
            printer.Text($"交易日期：{time[0]}").FeedLine();
            printer.Text($"交易时间：{time[1]}").FeedLine();
            printer.Text($"柜员编号：{FrameworkConst.OperatorId}").FeedLine();
            printer.Text($"请妥善保管好您的个人信息").FeedLine();
            printer.Text($"祝您早日康复！").FeedLine();

            printer.CutPaper(0);
            printer.Print();
            printer.Disconnect();
        }

        #region Binding

        private ObservableCollection<InfoIcon> _data;

        public ObservableCollection<InfoIcon> Data
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