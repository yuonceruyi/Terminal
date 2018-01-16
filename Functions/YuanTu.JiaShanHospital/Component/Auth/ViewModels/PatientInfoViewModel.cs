using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Models;
using YuanTu.Core.Reporter;
using YuanTu.Core.Services.PrintService;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.JiaShanHospital.HealthInsurance.Model;
using YuanTu.JiaShanHospital.NativeServices;
using YuanTu.JiaShanHospital.NativeServices.Dto;
using Prism.Regions;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;

namespace YuanTu.JiaShanHospital.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : YuanTu.Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        [Dependency]
        public IPaymentModel PaymentModel { get; set; }
        [Dependency]
        public ISiModel SiModel { get; set; }
        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }
        [Dependency]
        public IQueryChoiceModel QueryChoiceModel { get; set; }

        private int _readCount = 0;
        private string _cardNo;
        public string CardNo
        {
            get { return _cardNo; }
            set
            {
                _cardNo = value;
                OnPropertyChanged();
            }
        }
        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser, IMagCardDispenser[] magCardDispenser)
            : base(rfCardDispenser)
        {
            _magCardDispenser = magCardDispenser?.FirstOrDefault(p => p.DeviceId == "Act_F6_Mag");
        }

        protected override bool GetNewCardNo()
        {
            try
            {
                if (FrameworkConst.DoubleClick && FrameworkConst.VirtualHardWare)
                    return View.Dispatcher.Invoke(() =>
                    {
                        var ret = InputTextView.ShowDialogView("输入物理卡号");
                        if (ret.IsSuccess)
                        {
                            CardModel.CardNo = ret.Value;
                            return true;
                        }
                        return false;
                    });

                if (!_magCardDispenser.Connect().IsSuccess)
                {
                    ReportService.发卡器离线(null, ErrorSolution.发卡器离线);
                    ShowAlert(false, "建档发卡", "发卡机连接失败");
                    return false;
                }
                if (!_magCardDispenser.Initialize().IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机初始化失败");
                    return false;
                }
                var result = _magCardDispenser.EnterCard(TrackRoad.Trace2);
                if (!result.IsSuccess)
                {
                    //读卡失败时,回收卡片重新发卡
                    if (!_magCardDispenser.MoveCard(CardPosF6.吞入, "发卡机读卡号失败，故回收卡片").IsSuccess)
                    {
                        ShowAlert(false, "建档发卡", "发卡机读卡号失败后回收卡失败");
                        return false;
                    }
                    if (_readCount >= 3)
                    {
                        ShowAlert(false, "建档发卡", "发卡机读卡号失败,请联系管理员查看发卡器");
                        _readCount = 0;
                        return false;
                    }
                    _readCount++;
                    GetNewCardNo();
                }
                CardModel.CardNo = result.Value[TrackRoad.Trace2];
                return true;
            }
            catch (Exception ex)
            {
                ShowAlert(false, "建档发卡", "发卡机读卡失败");
                Logger.Main.Error($"[建档发卡]{ex.Message + ex.StackTrace}");
                return false;
            }
        }
        protected override void PrintCard()
        {
            _magCardDispenser?.MoveCardOut();
        }
        protected override void CreatePatient()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在准备就诊卡，请稍候...");
                //todo 发卡机发卡
                if (!GetNewCardNo()) return;

                Logger.Net.Info($"发卡机就诊卡准备完毕卡号：{CardModel.CardNo}");
                PaymentModel.Self = 1 * 100;
                PaymentModel.Insurance = 0;
                PaymentModel.Total = 1 * 100;
                PaymentModel.NoPay = false;
                PaymentModel.ConfirmAction = Bill;
                PaymentModel.MidList = new List<PayInfoItem>()
                    {
                        new PayInfoItem("办卡费用：","1元",true),
                    };
                Next();
            });
        }
        public override void Confirm()
        {
            if (Phone.IsNullOrWhiteSpace() && !(ChoiceModel.Business == Business.建档 && CardModel.CardType == CardType.社保卡))
            {
                ShowUpdatePhone = true;
                return;
            }
            if (ChoiceModel.Business == Business.建档)
            {
                CreatePatient();
                return;
            }
            else
            {
                if (CardModel.CardType == CardType.就诊卡 && PatientModel.当前病人信息.patientType != "01")
                {
                    ShowConfirm("医保病人提示", "就诊卡无法医保报销，是否继续操作？", cb =>
                    {
                        if (!cb)
                        {
                            Navigate(A.CK.Choice);
                        }
                        else
                        {
                            Next();
                        }
                    }, 0, ConfirmExModel.Build("是", "否"));
                    return;
                }
            }
            Next();
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            TopBottom.InfoItems = null;

            if (ChoiceModel.Business == Business.建档 && CardModel.CardType == CardType.身份证)
            {
                Name = IdCardModel.Name;
                Sex = IdCardModel.Sex.ToString();
                Birth = IdCardModel.Birthday.ToString("yyyy-MM-dd");
                Phone = null;
                IdNo = IdCardModel.IdCardNo.Mask(14, 3);
                IsAuth = false;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;
            }
            else if (ChoiceModel.Business == Business.建档 && CardModel.CardType == CardType.社保卡)
            {
                try
                {
                    Logger.Main.Info($"[社保卡建档处理] {CardModel.CardNo}");
                    CardNo = CardModel.CardNo;
                    Name = null;
                    Sex = null;
                    Birth = null;
                    Phone = null;
                    IdNo = null;
                    IsAuth = false;
                    ShowUpdatePhone = false;
                    CanUpdatePhone = false;
                }
                catch (Exception ex)
                {
                    Logger.Main.Error($"[社保卡建档处理] {ex.Message} {ex.StackTrace}");
                }
            }
            else
            {
                IsAuth = true;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;
                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                Name = patientInfo.name;
                Sex = patientInfo.sex;
                Birth = patientInfo.birthday.SafeToSplit(' ', 1)[0];
                Phone = patientInfo.phone.Mask(3, 4);
                IdNo = patientInfo.idNo.Mask(14, 3);
                GuardIdNo = patientInfo.guardianNo.Mask(14, 3);
            }
        }
        public Result Bill()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在建档，请稍候...");
                var reqAdd = new AddArchiveRequest();
                if (CardModel.CardType == CardType.身份证)
                {
                    reqAdd = new AddArchiveRequest
                    {
                        BussinessType = 8,
                        ArchiveType = "0100",
                        CardNo = CardModel.CardNo,
                        Phone = Phone,
                        IdNumber = IdCardModel.IdCardNo,
                        HomeAddress = IdCardModel.Address,
                        Sex = IdCardModel.Sex.ToString(),
                        Name = IdCardModel.Name,
                        Birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                    };
                }
                else
                {
                    reqAdd = new AddArchiveRequest
                    {
                        BussinessType = 8,
                        ArchiveType = "0200",
                        CardNo = CardModel.CardNo,
                    };
                }
                var resAdd = new Result<string>();
                Logger.Net.Info($"嘉善:建档入参{JsonConvert.SerializeObject(reqAdd)}");
                resAdd = LianZhongHisService.ExcuteHospitalAddArchive(reqAdd);
                Logger.Net.Info($"嘉善:建档出参{JsonConvert.SerializeObject(resAdd)}");
                if (resAdd.IsSuccess)
                {
                    if (!_magCardDispenser.MoveCardOut().IsSuccess)
                    {
                        if (!_magCardDispenser.MoveCardOut().IsSuccess)
                        {
                            ShowAlert(false, "温馨提示", "弹出卡失败,卡损坏或发卡机故障,请联系工作人员");
                            if (!_magCardDispenser.MoveCard(CardPosF6.吞入, "弹出卡失败").IsSuccess)
                            {
                                ShowAlert(false, "温馨提示", "回收损坏卡失败,请联系工作人员");
                            }
                            return Result.Fail("发卡机故障");
                        }
                    }
                    Logger.Net.Info($"嘉善:出卡成功");
                    var reqBill = new CheckoutRequest()
                    {
                        //CheckoutType ="",
                        BussinessType = 2,
                        CardNo = CardModel.CardNo,
                        CheckoutType = ""
                    };
                    var extraPaymentModel = GetInstance<IExtraPaymentModel>();
                    reqBill.AlipayAmount = (PaymentModel.Self / 100).ToString();
                    switch (PaymentModel.PayMethod)
                    {
                        case PayMethod.未知:
                            break;
                        case PayMethod.现金:
                            break;
                        case PayMethod.银联:
                            reqBill.PayFlag = PayMedhodFlag.银联;
                            reqBill.Account = (extraPaymentModel.PaymentResult as TransResDto).CardNo;
                            reqBill.AlipayTradeNo = (extraPaymentModel.PaymentResult as TransResDto).Ref;
                            break;
                        case PayMethod.预缴金:
                            //req.PayFlag = PayMedhodFlag.院内账户;
                            break;
                        case PayMethod.社保:
                            break;
                        case PayMethod.支付宝:
                            reqBill.PayFlag = PayMedhodFlag.支付宝;
                            reqBill.AlipayTradeNo = (extraPaymentModel.PaymentResult as 订单状态).outTradeNo;
                            break;
                        case PayMethod.微信支付:
                            reqBill.PayFlag = PayMedhodFlag.微信;
                            reqBill.AlipayTradeNo = (extraPaymentModel.PaymentResult as 订单状态).outTradeNo;
                            break;
                        case PayMethod.苹果支付:
                            break;
                        case PayMethod.智慧医疗:
                            break;
                        default:
                            break;
                    }
                    Logger.Net.Info($"嘉善:建档缴费开始");
                    var resBill = LianZhongHisService.ExcuteHospitalCheckout(reqBill);
                    Logger.Net.Info($"嘉善:建档缴费结束");
                    if (resBill.IsSuccess)
                    {
                        var billId = resBill.Value.BillId;
                        ExtraPaymentModel.Complete = true;
                        PrintModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = "建档缴费成功",
                            TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分建档缴费",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = CreatePrintables(billId),
                            TipImage = "提示_凭条和发卡"
                        });
                        lp.ChangeText("正在建档缴费交易记录上传，请稍候...");
                        Logger.Net.Info($"开始建档缴费交易记录上传:{billId}");
                        UpLoadTradeInfo(billId);
                        Navigate(A.JD.Print);
                        return Result.Success();
                    }

                    //第三方支付失败时去支付流程里面处理，不在业务里面处理
                    if (NavigationEngine.State != A.Third.PosUnion)
                    {
                        //PrintModel.SetPrintInfo(false, "缴费失败", errorMsg: BillPayModel.Res缴费结算?.msg);
                        PrintModel.SetPrintInfo(false, new PrintInfo
                        {
                            TypeMsg = "建档缴费失败",
                            DebugInfo = resBill.Message
                        });
                        Navigate(A.JD.Print);
                    }
                    if (resBill.ResultCode == -100)
                    {
                        UpLoadTradeInfo("123456");
                    }
                    ExtraPaymentModel.Complete = true;
                    return Result.Fail(resBill.Message);
                }

                PrintModel.SetPrintInfo(false, new PrintInfo
                {
                    TypeMsg = "建档失败",
                    DebugInfo = resAdd.Message
                });
                ShowAlert(false, "友情提示", $"建档交易失败:{resAdd.Message}");
                ExtraPaymentModel.Complete = true;
                return Result.Fail(-1, "建档失败");
            }).Result;
        }

        private void UpLoadTradeInfo(string billId)
        {
            try
            {
                UpLoadBillInfo(billId);
            }
            catch (Exception e)
            {
                Logger.Net.Info($"开始建档缴费交易记录上传失败:{e.Message}");
            }
        }

        protected Queue<IPrintable> CreatePrintables(string id)
        {
            var queue = PrintManager.NewQueue("自助发卡");
            var sb = new StringBuilder();
            sb.Append($"状态：办卡成功\n");
            sb.Append($"就诊卡号：{CardModel.CardNo}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"交易ID：{id}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"支付方式：{PaymentModel.PayMethod}\n");
            sb.Append($"请妥善保管好您的个人信息。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
        private void UpLoadBillInfo(string settleId)
        {
            Logger.Net.Info("开始建档交易记录同步到his系统");
            try
            {
                var req = new req交易记录同步到his系统
                {
                    platformId = FrameworkConst.OperatorId,
                    hisPatientId = CardModel?.CardNo,
                    cardNo = CardModel?.CardNo,
                    idNo = IdCardModel.IdCardNo,
                    patientName = IdCardModel.Name,
                    tradeType = "2",
                    cash = PaymentModel?.Total.ToString(),
                    operId = FrameworkConst.OperatorId,
                    bankTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tradeMode = GetEnumDescription(PaymentModel.PayMethod),
                    inHos = "1",
                    remarks = "建档",
                    settleId = settleId
                };
                FillRechargeRequest(req);
                var res = DataHandlerEx.交易记录同步到his系统(req);
                if (res.success)
                {
                    Logger.Net.Info("建档交易记录同步到his系统成功");
                }
                else
                {
                    Logger.Net.Info($"建档交易记录同步到his系统失败:{res.msg}");
                }
            }
            catch (Exception ex)
            {
                Logger.Net.Error($"建档交易记录同步到his系统失败异常:{ex.Message}");
            }
        }
        public static string GetEnumDescription(PayMethod value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }
        protected void FillRechargeRequest(req交易记录同步到his系统 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
                    req.bankDate = posinfo.TransDate;
                    req.posTransNo = posinfo.Trace;
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.MId;
                }
            }
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 || extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = extraPaymentModel.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    req.payAccountNo = thirdpayinfo.buyerAccount;
                    req.transNo = thirdpayinfo.outPayNo;
                    //req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                    req.extend = thirdpayinfo.outTradeNo;
                }
            }
        }
    }
}
