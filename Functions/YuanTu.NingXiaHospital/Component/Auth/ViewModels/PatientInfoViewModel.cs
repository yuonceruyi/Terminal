using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Models;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Devices.CardReader;
using YuanTu.NingXiaHospital.HisService;
using YuanTu.NingXiaHospital.HisService.Request;
using YuanTu.NingXiaHospital.HisService.Response;

namespace YuanTu.NingXiaHospital.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        private ResQueryPrescription _resQp;

        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser, IRFCpuCardReader[] rfCpuCardReaders) : base(rfCardDispenser)
        {
            _rfCpuCardReader = rfCpuCardReaders?.FirstOrDefault(p => p.DeviceId == "DKA6_IC"); ;
        }

        private IRFCpuCardReader _rfCpuCardReader;

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            TopBottom.InfoItems = null;

            Name = PatientModel.当前病人信息.name;
            Sex = PatientModel.当前病人信息.sex;
            Birth = PatientModel.当前病人信息.birthday;
            Phone = string.IsNullOrEmpty(PatientModel.当前病人信息.phone) ? null : PatientModel.当前病人信息.phone;
            IdNo = PatientModel.当前病人信息.idNo;
            IsAuth = false;
            CanUpdatePhone = false;
            ShowUpdatePhone = false;
        }

        public override void Confirm()
        {
            if (ChoiceModel.Business == Business.缴费)
            {
                SignAndQp();
                return;
            }
            var patientInfo = PatientModel.当前病人信息;
            ChangeNavigationContent($"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}");

            var resource = ResourceEngine;
            TopBottom.InfoItems = new ObservableCollection<InfoItem>(new[]
            {
                new InfoItem
                {
                    Title = "姓名",
                    Value = patientInfo.name,
                    Icon = resource.GetImageResourceUri("姓名图标_YellowBlue")
                },
                new InfoItem
                {
                    Title = "余额",
                    Value = patientInfo.accBalance.In元(),
                    Icon = resource.GetImageResourceUri("余额图标_YellowBlue")
                }
            });

            Next();
        }

        public void SignAndQp()
        {
            if (PatientModel.当前病人信息.patientType == "0" || CardModel.CardType == CardType.社保卡)
            {
                DoCommand(lp => { QueryPrescription(); });
                return;
            }
            Logger.Net.Info("开始签约");
            DoCommand(lp =>
            {
                var result = CardModel.CardType == CardType.身份证 ? IdCardSign() : BankCardSign();
                if (result)
                {
                    QueryPrescription();
                }
                else
                {
                    ShowAlert(false, "签约提示", result.Message);
                    return;
                }
            });
        }

        private void QueryPrescription()
        {
            if (FrameworkConst.DoubleClick)
            {
                BillRecordModel.Req获取缴费概要信息 = new req获取缴费概要信息
                {
                    patientId = IdCardModel.Name,
                    cardType = "",
                    cardNo = "",
                    billType = ""
                };
                BillRecordModel.Res获取缴费概要信息 = DataHandlerEx.获取缴费概要信息(BillRecordModel.Req获取缴费概要信息);
                PrintModel.SetPrintInfo(true, new PrintInfo
                {
                    TypeMsg = "缴费成功",
                    TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分缴费",
                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                    PrintablesList = BillPayPrintables(),
                    TipImage = "提示_凭条"
                });
                Navigate(A.JF.Print);
                return;
            }
            var reqQp = new
            {
                request = new Request
                {
                    head = new ReqHeadQueryPrescription
                    {
                        klx = CardModel.CardType == CardType.社保卡 ? "0" : "1",
                        kh = CardModel.CardNo ?? IdCardModel.IdCardNo,
                        czy = FrameworkConst.OperatorId
                    }
                }
            };
            var resultQp = DllHandler.QueryPrescription(reqQp.ToJsonString());
            if (resultQp)
            {
                if (resultQp.Value.recode == "1")
                {
                    _resQp = resultQp.Value;
                    PaymentModel.Self = decimal.Parse(resultQp.Value.redata.head.grxjzfje) * 100;
                    PaymentModel.Insurance = decimal.Parse(resultQp.Value.redata.head.grzhzfje) * 100;
                    PaymentModel.Total = decimal.Parse(resultQp.Value.redata.head.fyje) * 100;
                    PaymentModel.NoPay =
                        ChoiceModel.Business == Business.预约 ||
                        PaymentModel.Self == 0; //默认预约或者自费金额为0时不支付            
                    PaymentModel.ConfirmAction = BillConfirm;

                    PaymentModel.MidList = new List<PayInfoItem>
                    {
                        new PayInfoItem("医疗费总额：", PaymentModel.Total.In元()),
                        new PayInfoItem("医保支付金额：", PaymentModel.Insurance.In元()),
                        new PayInfoItem("个人支付金额：", PaymentModel.Self.In元())
                    };
                    Navigate(A.JF.Confirm);
                }
                else
                {
                    ShowAlert(false, "处方查询", $"错误：{resultQp.Value.remessage}");
                }
            }
            else
            {
                ShowAlert(false, "处方查询", $"未查询到处方信息：{resultQp.Message}");
            }
        }

        private Result BankCardSign()
        {
            var reqBankCardSign = new
            {
                request = new Request
                {
                    head = new ReqHeadPatientSign
                    {
                        klx = "1",
                        kh = CardModel.CardNo,
                        sfzh = IdCardModel.IdCardNo,
                        xm = IdCardModel.Name,
                        xb = IdCardModel.Sex == Consts.Enums.Sex.男 ? "0" : "1",
                        czy = "0000"
                    }
                }
            };
            var resultSign = DllHandler.PatientSign(reqBankCardSign.ToJsonString());
            if (resultSign && resultSign.Value.recode != "0")
            {
                PatientModel.当前病人信息.patientId = Common.GetNewPatientId("1", CardModel.CardNo);
                PatientModel.当前病人信息.patientType = "0";
                return Result.Success();
            }
            return Result.Fail($"签约失败：{resultSign.Message}");
        }

        public Result IdCardSign()
        {
            var reqIdCardSign = new
            {
                request = new Request
                {
                    head = new ReqHeadPatientSign
                    {
                        klx = "1",
                        kh = IdCardModel.IdCardNo,
                        sfzh = IdCardModel.IdCardNo,
                        xm = IdCardModel.Name,
                        xb = IdCardModel.Sex == Consts.Enums.Sex.男 ? "0" : "1",
                        czy = "0000"
                    }
                }
            };
            var resultSign = DllHandler.PatientSign(reqIdCardSign.ToJsonString());
            if (resultSign && resultSign.Value.recode != "0")
            {
                PatientModel.当前病人信息.patientId = Common.GetNewPatientId("1", IdCardModel.IdCardNo);
                PatientModel.当前病人信息.patientType = "0";
                return Result.Success();
            }
            return Result.Fail($"签约失败：{resultSign.Message}");
        }

        [Dependency]
        public IBillRecordModel BillRecordModel { get; set; }

        protected Result BillConfirm()
        {
            return DoCommand(lp =>
            {
                var reqBill = new
                {
                    request = new Request
                    {
                        head = new ReqHeadBill
                        {
                            fylx = "1",
                            jsid = _resQp.redata.head.jsid,
                            klx = _resQp.redata.head.klx,
                            kh = _resQp.redata.head.kh,
                            hbid = "1",
                            fyje = _resQp.redata.head.fyje,
                            czy = FrameworkConst.OperatorId,
                            zfmode = "1"
                        }
                    }
                };
                var resBill = DllHandler.Bill(reqBill.ToJsonString());
                if (resBill && resBill.Value.recode != "0")
                {

                    var cardNo = new StringBuilder();
                    foreach (var detail in resBill.Value.redata.detail)
                    {
                        cardNo.Append(detail.fphm);
                        cardNo.Append(",");
                    }

                    BillRecordModel.Req获取缴费概要信息 = new req获取缴费概要信息
                    {
                        patientName = PatientModel.当前病人信息.name,//IdCardModel.Name,
                        cardType = "",
                        cardNo = cardNo.ToString().Trim(','),
                        billType = ""
                    };
                    BillRecordModel.Res获取缴费概要信息 = DataHandlerEx.获取缴费概要信息(BillRecordModel.Req获取缴费概要信息);

                    var resBillNotice = BillNotice(true, resBill.Value.redata.detail[0].yblsh, true);
                    自费交易记录同步到his系统();
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "缴费成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分缴费",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        PrintablesList = BillPayPrintables(),
                        TipImage = "提示_凭条"
                    });
                    _rfCpuCardReader.UnInitialize();
                    Navigate(A.JF.Print);
                    return Result.Success();
                }
                else
                {
                    var resBillNotice = BillNotice(false, resBill.Value.redata.detail[0].yblsh, false);
                    Task.Run(() => { 自费交易记录同步到his系统();});
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = "缴费失败",
                        DebugInfo = $"病人ID{PatientModel.当前病人信息.patientId}|支付类别{PaymentModel.PayMethod}|{resBill.Message}|{resBill.Value.remessage}"
                    });
                    Navigate(A.JF.Print);
                    return Result.Fail($"");
                }
            }).Result;
        }

        private Result BillNotice(bool isSuccess, string yblsh, bool flag)
        {
            var bz = (flag ? "成功|" : "失败|") + GetTradeString();
            var zftype = "";
            if (PaymentModel.PayMethod == PayMethod.支付宝)
                zftype = "0";
            else if (PaymentModel.PayMethod == PayMethod.微信支付)
                zftype = "1";
            else if (PaymentModel.PayMethod == PayMethod.银联)
                zftype = "2";
            else
            {
                zftype = "3";
            }
            var req = new
            {
                request = new RequestBillNotice
                {
                    head = new ReqHeadBillNotice
                    {
                        jsid = _resQp.redata.head.jsid,
                        tzms = isSuccess ? "1" : "0",
                        fy = "",
                        yblsh = ""
                    },
                    detail = new List<ReqDeatilBillNotice>
                    {
                        new ReqDeatilBillNotice
                        {
                            yylsh = "",
                            yblsh = yblsh,
                            xjjyfy = new TradeInfo
                            {
                                bz = bz,
                                zf_type = zftype,
                                xj_id = "",
                                yh_kfh = "",
                                yh_zh = ""
                            }
                        }
                    }
                }
            };
            var res = DllHandler.BillNotice(req.ToJsonString());
            if (res && res.Value.recode != "0")
                return Result.Success();
            return Result.Fail($"{res.Message}");
        }

        protected List<Queue<IPrintable>> BillPayPrintables()
        {
            var sb = new StringBuilder();
            var ques = new List<Queue<IPrintable>>();
            if (BillRecordModel?.Res获取缴费概要信息?.data != null)
            {
                foreach (var item in BillRecordModel?.Res获取缴费概要信息?.data)
                {
                    var queue = PrintManager.NewQueue("门诊费用缴费");
                    sb.Append($"状态：缴费成功\n");
                    sb.Append($"姓名：{PatientModel.当前病人信息.name}\n");
                    sb.Append($"病人ID：{PatientModel.当前病人信息.patientId}\n");
                    sb.Append($"交易类型：自助缴费\n");
                    sb.Append($"医保支付：{PaymentModel.Insurance.In元()}\n");
                    sb.Append($"自费金额：{PaymentModel.Self.In元()}\n");
                    string type = PaymentModel.Self == 0 ? "医保":PaymentModel.PayMethod.ToString();
                    sb.Append($"支付方式：{type}\n");
                    sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
                    queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                    sb.Clear();
                    sb.Append($"收费项目单号：{item.billNo}\n");
                    queue.Enqueue(new PrintItemTriText("名称", "", "金额"));
                    foreach (var detail in item.billItem)
                    {
                        queue.Enqueue(new PrintItemTriText(detail.itemName, "", detail.billFee.InRMB()));
                    }
                    sb.Clear();
                    sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
                    sb.Append($"请妥善保管好您的缴费凭证\n");
                    sb.Append($"如需发票请到结算处打印\n");
                    sb.Append($"\n");
                    sb.Append($"---------请撕开----------\n");
                    sb.Append($"\n");
                    queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                    sb.Clear();
                    ques.Add(queue);
                }
            }
            else
            {
                var queue = PrintManager.NewQueue("门诊费用缴费");
                sb.Append($"状态：缴费成功\n");
                sb.Append($"姓名：{IdCardModel.Name}\n");
                sb.Append($"病人ID：{PatientModel.当前病人信息.patientId}\n");
                sb.Append($"交易类型：自助缴费\n");
                sb.Append($"金额总计：{PaymentModel.Total.In元()}\n");
                sb.Append($"支付方式：{PaymentModel.PayMethod}\n");
                sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
                sb.Append($"未获取到缴费明细\n");
                sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
                sb.Append($"请妥善保管好您的缴费凭证\n");
                queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                ques.Add(queue);
            }
            return ques;
        }

        private void 自费交易记录同步到his系统()
        {
            try
            {
                Logger.Net.Info($"开始交易记录同步到his系统");
                var req = new req交易记录同步到his系统
                {
                    platformId = FrameworkConst.OperatorId,
                    hisPatientId = CardModel.CardNo,
                    cardNo = CardModel.CardNo,
                    idNo = PatientModel.当前病人信息.idNo,
                    patientName = PatientModel.当前病人信息.name,
                    tradeType = "2",
                    tradeMode = GetEnumDescription(PaymentModel.PayMethod),
                    cash = PaymentModel.Self.ToString("0"),
                    operId = FrameworkConst.OperatorId,
                    bankTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    inHos = "1",
                    remarks = "缴费"
                };
                FillRechargeRequest(req);
                var res = DataHandlerEx.交易记录同步到his系统(req);
                if (res.success)
                    Logger.Net.Info($"交易记录同步到his系统成功");
                else
                    Logger.Net.Info($"交易记录同步到his系统失败:{res.msg}");
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"交易记录同步到his系统失败异常:{ex.Message}");
            }
        }

        public static string GetEnumDescription(PayMethod value)
        {
            var fi = value.GetType().GetField(value.ToString());
            var attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            return value.ToString();
        }

        protected virtual void FillRechargeRequest(req交易记录同步到his系统 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
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
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 ||
                     extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = extraPaymentModel.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    req.payAccountNo = thirdpayinfo.buyerAccount;
                    req.transNo = thirdpayinfo.outPayNo;
                    //req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                }
            }
        }

        protected string GetTradeString()
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                return
                    $"{posinfo.CardNo}|{posinfo.Amount}|{posinfo.Auth}|{posinfo.Batch}|{posinfo.Lrc}|{posinfo.MId}|{posinfo.Memo}|{posinfo.Receipt}|{posinfo.Ref}|{posinfo.Trace}|{posinfo.RespInfo}|{posinfo.TransDate}|{posinfo.TransTime}|{posinfo.Amount}";
            }
            if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 ||
                extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = extraPaymentModel.PaymentResult as 订单状态;
                return
                    $"{thirdpayinfo.buyerAccount}|{thirdpayinfo.fee}|{thirdpayinfo.outPayNo}|{thirdpayinfo.outRefundNo}|{thirdpayinfo.outTradeNo}|{thirdpayinfo.paymentTime}|{thirdpayinfo.status}";
            }
            return "医保全额";
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            if (ChoiceModel.Business != Business.缴费)
            {
                _rfCpuCardReader.UnInitialize();
            }
            
            return base.OnLeaving(navigationContext);
        }
    }
}