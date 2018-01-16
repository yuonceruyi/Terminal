using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Navigating;
using YuanTu.Core.Services.PrintService;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.ShenZhenArea.Models;
using Newtonsoft.Json;
using YuanTu.ShenZhenArea.Services;
using YuanTu.ShenZhenArea.Gateway;
using System;
using YuanTu.Core.Log;
using System.Drawing;

namespace YuanTu.ShenZhen.BaoAnChineseMedicineHospital.Component.BillPay.ViewModels
{
    public class BillRecordViewModel : YuanTu.Default.Component.BillPay.ViewModels.BillRecordViewModel
    {
        [Dependency]
        public IYBModel YBModel { get; set; }


        [Dependency]
        public IYBService YB_Service { get; set; }


        [Dependency]
        public IAccountingService Account_Service { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            TipMsg = "我要缴费";

            Collection = BillRecordModel.Res获取缴费概要信息.data.Select(p => new PageData
            {
                //CatalogContent = $"{p.doctName} {p.deptName}\r\n金额 {p.billFee.In元()}",
                //CatalogContent = $"{Convert.ToDateTime(p.billDate).ToString("yyyy-MM-dd")}{p.deptName}-{p.doctName}-{p.billType}-{p.billFee.In元()}",
                CatalogContent = $"{Convert.ToDateTime(p.billDate).ToString("yyyy-MM-dd")}{p.deptName}[{p.billType}]{p.billFee.In元()}",
                List = p.billItem,
                Tag = p
            }).ToArray();
            BillCount = $"{BillRecordModel.Res获取缴费概要信息.data.Count}张处方单";
            TotalAmount = BillRecordModel.Res获取缴费概要信息.data.Sum(p => decimal.Parse(p.billFee)).In元();
            PlaySound(SoundMapping.选择待缴费处方);
        }

        protected override void Do()
        {
            BillRecordModel.所选缴费概要 = SelectData.Tag.As<缴费概要信息>();
            ChangeNavigationContent(SelectData.CatalogContent);
            var recordInfo = BillRecordModel.所选缴费概要;

            if (CardModel.CardType == CardType.就诊卡 && recordInfo.billType != "自费")
            {
                ShowAlert(false, "缴费信息确认", "使用就诊卡的医保病人，请到人工窗口缴费", 10);
                Navigate(A.Home);
                return;
            }

            //不支持在自助机上缴费，默认不支持
            bool ForbidSelfPay = true;

            //自费
            if (recordInfo.billType == "自费")
            {
                ForbidSelfPay = false;
            }
            //生育通道
            if (recordInfo.billType == "生育通道")
            {
                ForbidSelfPay = false;
            }
            //特检
            if (recordInfo.billType == "特检")
            {
                ForbidSelfPay = false;
            }
            #region 普通医保的一档或者二档少儿医保的方案
            //if (recordInfo.billType == "普通医保" && (YBModel.参保类型 == ShenZhenArea.Enums.Cblx.基本医疗保险一档 || YBModel.参保类型 == ShenZhenArea.Enums.Cblx.医疗保险二档少儿))
            //{
            //    ForbidSelfPay = false;
            //}
            #endregion

            #region 普通医保全部放开的方案

            if (recordInfo.billType == "普通医保")
            {
                ForbidSelfPay = false;
            }

            #endregion

            if (ForbidSelfPay)
            {
                //ShowAlert(false, "缴费信息确认", "目前自助机支持以下类型缴费\n基本医疗保险（一档）、医疗保险二档（少儿）、生育医保、自费、特检", 10);
                ShowAlert(false, "缴费信息确认", "目前自助机支持以下类型缴费\n普通医保、生育医保、自费、特检", 10);
                //Navigate(A.Home);   //不跳到首页
                return;
            }

            if (recordInfo.billType == "自费")
            {
                PaymentModel.Self = decimal.Parse(recordInfo.billFee);
                PaymentModel.Insurance = decimal.Parse("0");
                PaymentModel.Total = decimal.Parse(recordInfo.billFee);
                PaymentModel.NoPay = false;
                PaymentModel.ConfirmAction = Confirm;

                var dateTime = recordInfo.billDate?.SafeToSplit(' ', 2);
                PaymentModel.LeftList = new List<PayInfoItem>
                {
                    new PayInfoItem("日期：", dateTime?[0] ?? recordInfo.billDate),
                    new PayInfoItem("时间：", dateTime?.Count()>1?dateTime[1] : null),
                    new PayInfoItem("科室：", recordInfo.deptName),
                    new PayInfoItem("医生：", recordInfo.doctName)
                };

                PaymentModel.RightList = new List<PayInfoItem>
                {
                    new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                    new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                    new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
                };

                Next();
            }

            bool breakYB = false;  //是否退出医保
            YB_Service.获取就诊信息();
            if (recordInfo.billType != "自费")
            {
                DoCommand(lp =>
                {
                    lp.ChangeText("正在进行医保计算，请稍候...");

                    //医保预结算
                    var 医保门诊挂号结果 = YB_Service.医保门诊挂号();
                    if (!医保门诊挂号结果.IsSuccess)
                    {
                        ShowAlert(false, "医保门诊挂号失败", 医保门诊挂号结果.Message + "\n点击确定进行自费结算");
                        breakYB = true;
                    }

                    if (!breakYB)
                    {
                        var 医保门诊登记结果 = YB_Service.医保门诊登记();
                        if (!医保门诊登记结果.IsSuccess)
                        {
                            ShowAlert(false, "医保门诊登记失败", 医保门诊登记结果.Message, 20);
                            Navigate(A.Home);
                            return;
                        }
                    }

                    if (!breakYB)
                    {
                        var 医保门诊费用结果 = YB_Service.医保门诊费用();
                        if (!医保门诊费用结果.IsSuccess)
                        {
                            ShowAlert(false, "医保门诊预结算失败", "医保门诊预结算失败\n错误信息:" + 医保门诊费用结果.Message, 20);
                            Navigate(A.Home);
                            return;
                        }
                    }
                    if (breakYB)
                    {
                        PaymentModel.Self = decimal.Parse(recordInfo.billFee);
                        PaymentModel.Insurance = decimal.Parse("0");
                        PaymentModel.Total = decimal.Parse(recordInfo.billFee);
                        PaymentModel.NoPay = false;
                        PaymentModel.ConfirmAction = Confirm;
                    }
                    else
                    {
                        PaymentModel.Self = 100 * (decimal)YBModel.现金金额;
                        PaymentModel.Insurance = 100 * (decimal)YBModel.记账金额;
                        PaymentModel.Total = 100 * (decimal)YBModel.总额;
                        PaymentModel.NoPay = false;
                        PaymentModel.ConfirmAction = Confirm;
                    }

                    var dateTime = recordInfo.billDate?.SafeToSplit(' ', 2);
                    PaymentModel.LeftList = new List<PayInfoItem>
                    {
                        new PayInfoItem("日期：", dateTime?[0] ?? recordInfo.billDate),
                        new PayInfoItem("时间：", dateTime?.Count()>1?dateTime[1] : null),
                        new PayInfoItem("科室：", recordInfo.deptName),
                        new PayInfoItem("医生：", recordInfo.doctName)
                    };

                    PaymentModel.RightList = new List<PayInfoItem>
                    {
                        new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                        new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                        new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
                    };
                    if (PaymentModel.Self == 0)
                    {
                        PaymentModel.NoPay = true;
                    }
                    Next();
                });
            }
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在进行缴费，请稍候...");

                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                var record = BillRecordModel.所选缴费概要;
                if (PaymentModel.Insurance > 0)
                {
                    //医保支付确认
                    var 医保门诊支付确认结果 = YB_Service.医保门诊支付确认(false);
                    if (!医保门诊支付确认结果.IsSuccess)  //结算失败
                    {
                        ShowAlert(false, "医保记账失败", "医保记账失败,请稍候重试\n错误信息:" + 医保门诊支付确认结果.Message, 20);
                        Navigate(A.Home);
                        return 医保门诊支付确认结果;
                    }
                }
                YB_Service.处理医保挂号结算信息();
                if (PaymentModel.PayMethod == PayMethod.未知)
                {
                    PaymentModel.PayMethod = PayMethod.预缴金;
                }

                BillPayModel.Req缴费结算 = new req缴费结算
                {
                    patientId = patientInfo.patientId,
                    patientName = patientInfo.name,
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,
                    operId = FrameworkConst.OperatorId,
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    cash = PaymentModel.Total.ToString(),
                    accountNo = patientInfo.patientId,
                    billNo = record.billNo,
                    allSelf = PaymentModel.Insurance == 0 ? "1" : "0",
                    extendBalanceInfo = YBModel.HIS结算所需医保信息
                };

                FillRechargeRequest(BillPayModel.Req缴费结算);

                BillPayModel.Res缴费结算 = DataHandlerEx.缴费结算(BillPayModel.Req缴费结算);
                if (BillPayModel.Res缴费结算?.success ?? false)   //缴费成功。。。
                {
                    if (PaymentModel.PayMethod == PayMethod.预缴金)   //处理预交金余额
                    {
                        patientInfo.accBalance = (Convert.ToInt64(patientInfo.accBalance) - Convert.ToInt64(PaymentModel.Self)).ToString();
                    }

                    ExtraPaymentModel.Complete = true;
                    //todo  处理医保记账、消费记账
                    if (PaymentModel.Insurance > 0)
                    {
                        Account_Service.医保消费记账(true);
                    }
                    Account_Service.门诊结算记账(true);

                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "缴费成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分缴费",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = BillPayPrintables(),
                        TipImage = "提示_凭条"
                    });
                    Navigate(A.JF.Print);
                    ShowAlert(true, "缴费成功温馨提示", "您已成功缴费。如需报销或用于其它证明时应及时去人工窗口打印发票，否则造成后期打印发票日期与就诊日期不相符。", 20);
                    if (BillRecordModel?.Res获取缴费概要信息?.data.Count > 1)
                    {
                        ShowAlert(true, "其他费用提现", "您还有其他待缴费处方；回到主界面后可重复刚才步骤继续缴费。", 20);
                    }
                    return Result.Success();
                }
                else            //缴费失败
                {
                    if (PaymentModel.Insurance > 0)  //医保退费
                    {
                        if (DataHandler.UnKnowErrorCode.Contains(BillPayModel.Res缴费结算.code))  //单边账。。
                        {
                            BillPayModel.Res缴费结算.msg = $"{BillPayModel.Res缴费结算.code} 服务受理异常,缴费失败!";
                            PrintModel.SetPrintInfo(true, new PrintInfo
                            {
                                TypeMsg = "社保单边账",
                                TipMsg = $"您于{DateTimeCore.Now.ToString("HH:mm")}分缴费失败",
                                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                Printables = BillErrSheBaoPayPrintables(),
                                TipImage = "提示_凭条"
                            });

                            ShowAlert(false, "缴费结算结果未知", $"{BillPayModel.Res缴费结算.code} 服务受理异常,缴费失败!", 20);
                            //todo 处理医保记账、消费记账
                            Account_Service.门诊结算记账(false, true);
                            Account_Service.医保消费记账(false);
                            Navigate(A.JF.Print);
                            return Result.Fail(BillPayModel.Res缴费结算?.code ?? -100, BillPayModel.Res缴费结算?.msg);
                        }

                        //医保退费
                        var 医保门诊退费结果 = YB_Service.医保门诊退费();
                        if (!医保门诊退费结果.IsSuccess)
                        {
                            ShowAlert(false, "医保门诊退费失败", 医保门诊退费结果.Message, 20);
                        }
                        var 医保门诊支付确认 = YB_Service.医保门诊支付确认(true);
                        if (!医保门诊支付确认.IsSuccess)
                        {
                            ShowAlert(false, "医保门诊退费确认失败", 医保门诊退费结果.Message, 20);
                        }
                    }

                    ShowAlert(false, "缴费结算失败", BillPayModel.Res缴费结算.msg, 20);
                    //处理消费记账
                    Account_Service.门诊结算记账(false);
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "缴费失败",
                        TipMsg = $"您于{DateTimeCore.Now.ToString("HH:mm")}分缴费失败",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = BillPayPrintablesFail(),
                        TipImage = "提示_凭条"
                    });
                    Navigate(A.JF.Print);  //打印缴费失败的提示

                    //第三方支付失败时去支付流程里面处理，不在业务里面处理
                    if (NavigationEngine.State != A.Third.PosUnion)
                    {
                        PrintModel.SetPrintInfo(false, new PrintInfo
                        {
                            TypeMsg = "缴费失败",
                            DebugInfo = BillPayModel.Res缴费结算?.msg
                        });
                        Navigate(A.JF.Print);
                    }
                    ExtraPaymentModel.Complete = true;
                    return Result.Fail(BillPayModel.Res缴费结算?.code ?? -100, BillPayModel.Res缴费结算?.msg);
                }
            }).Result;
        }

        protected override void FillRechargeRequest(req缴费结算 req)
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
                    req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                }
            }
        }

        protected override Queue<IPrintable> BillPayPrintables()
        {
            var queue = PrintManager.NewQueue("门诊费用缴费");
            var billPay = BillPayModel.Res缴费结算.data;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var record = BillRecordModel.所选缴费概要;
            var sb = new StringBuilder();
            #region 登记号二维码
            var image = BarCode128.GetCodeImage(patientInfo.patientId, BarCode.Code128.Encode.Code128A);
            queue.Enqueue(new PrintItemImage
            {
                Align = ImageAlign.Center,
                Image = image,
                Height = image.Height / 1.5f,
                Width = image.Width / 1.5f
            });
            #endregion

            sb.Append($"\n");
            sb.Append($"登记号：{patientInfo.patientId}　卡号：{CardModel.CardNo}\n");
            sb.Append($"姓名：{patientInfo.name}　年龄：{patientInfo.birthday.ToAge()}岁 　性别：{patientInfo.sex}\n");
            sb.Append($"病人类型：{patientInfo.patientType} 　就诊科室：{record.deptName}\n");
            sb.Append($"收据号：{billPay.receiptNo} 　收费时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"------------------------------\n");
            sb.Append($"收费金额：{PaymentModel.Total.In元()}\n");
            sb.Append($"------------------------------\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            if (!string.IsNullOrEmpty(billPay?.extend))
            {
                try
                {
                    List<OEOrdFreeModelBaoAnChineseMedicineHospital> ShouFeiResult = JsonConvert.DeserializeObject<List<OEOrdFreeModelBaoAnChineseMedicineHospital>>(billPay?.extend);
                    foreach (OEOrdFreeModelBaoAnChineseMedicineHospital item in ShouFeiResult)
                    {
                        sb.Append($"接收科室:{item.RecLocDesc}\n地点:{item.RecLocAddress}\n");

                        if (!string.IsNullOrEmpty(item.TMPZYPDesc1))
                        {
                            sb.Append($"{item.TMPZYPDesc1}\n");
                        }

                        if (!string.IsNullOrEmpty(item.TMPZYPDesc2))
                        {
                            sb.Append($"{item.TMPZYPDesc2}\n");
                        }

                        if (!string.IsNullOrEmpty(item.TMPZYPDesc3))
                        {
                            sb.Append($"{item.TMPZYPDesc3}\n");
                        }

                        if (!string.IsNullOrEmpty(item.TMPZYPDesc4))
                        {
                            sb.Append($"{item.TMPZYPDesc4}\n");
                        }
                        if (!string.IsNullOrEmpty(item.PresNonum))
                        {
                            sb.Append($"{item.PresNonum}\n");
                        }
                        sb.Append($"名称　单位　单价　数量　金额　是否医保");
                        queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                        sb.Clear();
                        foreach (OEOrdFreItemModelBaoAnChineseMedicineHospital detail in item.OEOrdItem)
                        {
                            var tPrice = Math.Round(Convert.ToDecimal(detail.Price), 2, MidpointRounding.AwayFromZero);
                            var tInsurance = string.IsNullOrEmpty(detail.xmlb) ? "否" : "是";
                            sb.AppendFormat($"{detail.ArcmiDesc}({detail.tariCode})\n　　　{detail.UOM}　{tPrice}　{ detail.Qty}　{detail.TotalAmount}　{tInsurance}\n");
                            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                            sb.Clear();
                        }
                    }
                    sb.Append($"------------------------------\n");
                    queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                    sb.Clear();
                }
                catch (Exception ex)
                {
                    Logger.Main.Debug(ex.Message + "\n" + ex.StackTrace);
                    sb.Append($"返回的缴费明细解析失败；请凭此单据到人工收费窗口打印导诊单。给您带来不便敬请谅解\n");
                    queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                    sb.Clear();
                }
            }

            sb.Append($"合计金额：{PaymentModel.Total.In元()}\n");
            if (PaymentModel.Insurance > 0)
            {
                sb.Append($"医保报销：{PaymentModel.Insurance.In元()}\n");
                sb.Append($"记账前余额：{YBModel.记账前}元    记账后余额：{YBModel.记账后}\n");
            }

            if (PaymentModel.Self > 0)
            {
                sb.Append($"自费金额：{PaymentModel.Self.In元()}　　支付方式：{PaymentModel.PayMethod}\n");
            }

            if (PaymentModel.PayMethod == PayMethod.预缴金)
            {
                sb.Append($"预缴金余额：{patientInfo.accBalance.In元()}\n");
            }

            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的缴费凭证\n温馨提示：如需报销或用于其它证明时请及时打印发票，否则造成发票打印日期与就诊日期不相符。谢谢合作！");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

        protected virtual Queue<IPrintable> BillErrSheBaoPayPrintables()
        {
            var queue = PrintManager.NewQueue("社保单边账");
            var billPay = BillPayModel.Res缴费结算.data;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var record = BillRecordModel.所选缴费概要;
            var sb = new StringBuilder();
            //sb.Append($"状态：缴费失败\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：自助缴费\n");
            sb.Append($"金额总计：{PaymentModel.Total.In元()}\n");
            sb.Append($"支付方式：{PaymentModel.PayMethod.GetEnumDescription()}\n");
            sb.Append($"社保单据号：{YBModel.djh}\n");
            sb.Append($"就诊ID：{YBModel.就诊记录ID}\n");
            sb.Append($"社保金额：{PaymentModel.Insurance.In元()}\n");
            sb.Append($"失败原因：{BillPayModel.Res缴费结算.msg}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请凭该凭证寻找工作人员处理\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }


        protected virtual Queue<IPrintable> BillPayPrintablesFail()
        {
            var queue = PrintManager.NewQueue("门诊费用缴费");
            var billPay = BillPayModel.Res缴费结算.data;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var record = BillRecordModel.所选缴费概要;
            var sb = new StringBuilder();
            sb.Append($"状态：缴费失败\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：自助缴费\n");
            sb.Append($"金额总计：{PaymentModel.Total.In元()}\n");
            sb.Append($"支付方式：{PaymentModel.PayMethod}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请凭该凭证寻找工作人员处理\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }
    }
}