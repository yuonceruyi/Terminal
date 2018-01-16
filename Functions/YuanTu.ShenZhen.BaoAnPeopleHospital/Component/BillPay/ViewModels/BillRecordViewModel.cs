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
using System.Drawing;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.BillPay.ViewModels
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
            #region 先不拦截

            //if (BillRecordModel.Res获取缴费概要信息.data.Any(d => Convert.ToDateTime(d.billDate).Date.AddDays(3) < DateTimeCore.Now && Convert.ToDateTime(d.billDate).Date.AddYears(1) > DateTimeCore.Now))
            //{
            //    ShowAlert(false, "该账户有超过3天的历史欠费", "该账户有1年内的历史欠费不能在自助机上缴费。\n请到人工窗口缴费。");
            //    Navigate(A.Home);
            //    return;
            //}
            #endregion

            if (string.IsNullOrEmpty(PatientModel.当前病人信息.accountNo))
            {
                ShowAlert(false, "该账户不能在自助机上缴费", "预交金账户信息不存在,该账户不能在自助机上缴费。请到人工窗口预交金账户");
                Navigate(A.Home);
            }

            ChangeNavigationContent("");

            TipMsg = "我要缴费";

            Collection = BillRecordModel.Res获取缴费概要信息.data.Select(p => new PageData
            {
                //CatalogContent = $"{p.doctName} {p.deptName}\r\n金额 {p.billFee.In元()}",
                //CatalogContent = $"{Convert.ToDateTime(p.billDate).ToString("yyyy-MM-dd")}{p.deptName}-{p.doctName}-{p.billType}-{p.billFee.In元()}",
                CatalogContent = $"{Convert.ToDateTime(p.billDate).ToString("yyyy-MM-dd")}{p.deptName.Split('-')?[1]}[{p.billType}]{p.billFee.In元()}",
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

            #region 拦截费用

            if (Convert.ToDateTime(recordInfo.billDate).Date.AddDays(3) > DateTimeCore.Now)  //选择3天内的
            {
                List<缴费概要信息> list欠费 = BillRecordModel.Res获取缴费概要信息.data.Where(d => d.billNo != BillRecordModel.所选缴费概要.billNo && Convert.ToDateTime(d.billDate).Date.AddDays(3) < DateTimeCore.Now && Convert.ToDateTime(d.billDate).Date.AddYears(1) > DateTimeCore.Now).ToList();

                if (list欠费?.Count > 0)  //有3天以及一年内的欠费
                {
                    if (list欠费.Any(d => d.deptName == "口腔科"))
                    {
                        ShowAlert(false, "您有1年内的口腔科费用欠费", "请先交纳您的历史欠费再缴纳3天内的费用。");
                        return;
                    }

                    foreach (var item in list欠费)
                    {
                        if (item.billItem.Any(d => d.itemName.Contains("诊查费")))
                        {
                            ShowAlert(false, "您有1年内的门诊诊查费欠费", "请先交纳您的历史欠费再缴纳3天内的费用。");
                            return;
                        }
                    }
                }
            }
            #endregion

            #region 账单类型支持
            //支持在自助机上缴费的BillType，默认不支持
            bool SupportBillType = false;

            //自费的
            if (recordInfo.billType == "自费")
            {
                SupportBillType = true;
            }
            //普通医保的
            if (recordInfo.billType.Contains("普通医保") || recordInfo.billType == "特检")
            {
                SupportBillType = true;
            }

            //生育医保的
            if (recordInfo.billType.Contains("生育医保"))
            {
                //生育医保特殊处方拦截。。。
                foreach (var item in recordInfo.billItem)
                {
                    if (item.itemName.Contains("人工流产术"))
                    {
                        ShowAlert(false, "缴费信息确认", $"您的缴费项目“{item.itemName}”在生育医保内；\n请移步至人工窗口收费，需要人工验证，谢谢！\n给您带来不便，还请谅解", 10);
                        Navigate(A.Home);
                        return;
                    }
                }
                SupportBillType = true;
            }

            if (!SupportBillType)
            {
                ShowAlert(false, "缴费信息确认", "目前自助机仅支持以下账单缴费\n普通医保、自费、特检、生育医保", 10);
                Navigate(A.Home);
                return;
            }
            #endregion

            var cm = (CardModel as ShenZhenCardModel);
            bool mustZiFei = true;

            #region 当前条件下是否可医保缴费

            if (recordInfo.billType != "自费")  //非自费的账单
            {
                //没通过插社保卡验证。提示一下
                if (CardModel.CardType != CardType.社保卡 || cm.RealCardType != CardType.社保卡)
                {
                    ShowAlert(true, "温馨提示", "系统记录到您的账单是可报销的医保账单人\n但您本次就医未拿社保卡验证\n故本次就医只能自费缴费", 10);
                }
                else
                {
                    if (YBModel.参保类型 == ShenZhenArea.Enums.Cblx.基本医疗保险一档 || YBModel.参保类型 == ShenZhenArea.Enums.Cblx.医疗保险二档少儿)
                    {
                        //正常可以社保缴费
                        mustZiFei = false;
                    }
                    else
                    {
                        ShowAlert(false, "缴费信息确认", "目前自助机仅支持基本医疗保险一档、医疗保险二档少儿两种医保类型缴费", 10);
                        Navigate(A.Home);
                        return;
                    }
                }
            }
            else
            {

            }

            #endregion

            PaymentModel.NoPay = false;
            PaymentModel.ConfirmAction = Confirm;

            if (mustZiFei)
            {
                PaymentModel.Self = decimal.Parse(recordInfo.billFee);
                PaymentModel.Insurance = decimal.Parse("0");
                PaymentModel.Total = decimal.Parse(recordInfo.billFee);

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
            if (!mustZiFei)
            {
                //医保预结算
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
                    }
                    else
                    {
                        PaymentModel.Self = 100 * (decimal)YBModel.现金金额;
                        PaymentModel.Insurance = 100 * (decimal)YBModel.记账金额;
                        PaymentModel.Total = 100 * (decimal)YBModel.总额;
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
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            return DoCommand(lp =>
            {
                lp.ChangeText("正在进行缴费，请稍候...");

                var record = BillRecordModel.所选缴费概要;
                //if (BillRecordModel.所选缴费概要?.billType != "自费")
                if (PaymentModel.Insurance > 0)   //如果以报销金额大于0判断的话会造成那种医保卡没钱，自费金额不累计的情况
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

                BillPayModel.Req缴费结算 = new req缴费结算
                {
                    patientId = patientInfo.patientId,
                    patientName = patientInfo.name,
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,
                    operId = FrameworkConst.OperatorId,
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    cash = PaymentModel.Total.ToString(), //单位为分
                    accountNo = patientInfo.patientId,
                    billNo = record.billNo,
                    allSelf = PaymentModel.Insurance == 0 ? "1" : "0",
                    extendBalanceInfo = YBModel.HIS结算所需医保信息,
                    extend = PaymentModel.Self.ToString()  //单位是分
                };

                FillRechargeRequest(BillPayModel.Req缴费结算);

                BillPayModel.Res缴费结算 = DataHandlerEx.缴费结算(BillPayModel.Req缴费结算);
                if (BillPayModel.Res缴费结算?.success ?? false)   //缴费成功。。。
                {
                    if(PaymentModel.PayMethod== PayMethod.预缴金)   //处理预交金余额
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
                    req.bankDate = DateTimeCore.Now.ToString("yyyy") + posinfo.TransDate + posinfo.TransTime;
                    req.posTransNo = posinfo.Trace.PadLeft(6, '0');
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.MId;
                    req.posIndexNo = posinfo.Batch;
                    req.invoice = posinfo.Auth;
                }
            }
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 || extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
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
            var queue = PrintManager.NewQueue("导诊及费用清单");
            var billPay = BillPayModel.Res缴费结算.data;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var record = BillRecordModel.所选缴费概要;
            var sb = new StringBuilder();


            var patientInfoimage = BarCode128.GetCodeImage(patientInfo.patientId, BarCode.Code128.Encode.Code128A);
            queue.Enqueue(new PrintItemImage
            {
                Align = ImageAlign.Center,
                Image = patientInfoimage,
                Height = patientInfoimage.Height / 1.5f,
                Width = patientInfoimage.Width / 1.5f
            });

            //sb.Append($"状态：缴费成功\n");
            sb.Append($"登记号:{patientInfo.patientId}   卡号:{CardModel.CardNo}\n");
            sb.Append($"姓名:{patientInfo.name}　年龄:{patientInfo.birthday.Age()}岁　性别:{patientInfo.sex}\n");
            //sb.Append($"交易类型：自助缴费\n");
            sb.Append($"病人类型：{patientInfo.patientType ?? "自费"}   就诊科室：{record.deptName}\n");
            sb.Append($"就诊医生：{record.doctName}     收费时间{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"------------------------------------------\n");
            sb.Append($"收费金额：{PaymentModel.Total.In元()}\n");
            sb.Append($"------------------------------------------\n");
            if (PaymentModel.Insurance > 0)
            {
                sb.Append($"医保报销：{PaymentModel.Insurance.In元()}　　医保余额：{YBModel.记账后}元\n");
            }
            if (PaymentModel.Self > 0)
            {
                sb.Append($"自费金额：{PaymentModel.Self.In元()}  自费支付方式：{PaymentModel.PayMethod}\n");
            }
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();

            if (PaymentModel.PayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    //sb.Append($"银联交易卡号：{posinfo.CardNo}\n");
                    sb.Append($"银联订单号：{posinfo.Trace}  系统参考号：{posinfo.Ref}\n");
                }
            }

            if (PaymentModel.PayMethod == PayMethod.微信支付 || PaymentModel.PayMethod == PayMethod.支付宝)  //微信或支付宝
            {
                var thirdpayinfo = extraPaymentModel.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    sb.Append($"交易流水：{thirdpayinfo.outTradeNo}  {(PaymentModel.PayMethod == PayMethod.微信支付 ? "微信" : "支付宝")}流水：{thirdpayinfo.outPayNo}\n");
                }
            }

            sb.Append($"预缴金余额：{patientInfo.accBalance.In元()}\n");
            //sb.Append($"收据号：{billPay.receiptNo}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            if (!string.IsNullOrEmpty(billPay?.extend))
            {
                List<OEOrdFreeModelBaoAnPeopleHospital> ShouFeiResult = JsonConvert.DeserializeObject<List<OEOrdFreeModelBaoAnPeopleHospital>>(billPay?.extend);
                foreach (OEOrdFreeModelBaoAnPeopleHospital item in ShouFeiResult)
                {
                    sb.Append($"------------------------------------------\n");
                    //sb.Append($"接收科室:{item.locDesc}\n所在地点:{item.Adress}\n");
                    queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                    sb.Clear();

                    StringBuilder sbTemp = new StringBuilder();
                    if (!string.IsNullOrEmpty(item.locDesc))
                    {
                        sbTemp.Append($"请前往科室:{item.locDesc}\n");
                    }
                    if (!string.IsNullOrEmpty(item.Adress))
                    {
                        sbTemp.Append($"所在地点:{item.Adress}\n");
                    }
                    if (sbTemp.Length > 0)
                    {
                        queue.Enqueue(new PrintItemText { Text = sbTemp.ToString(), Font = new Font("微软雅黑", 12, FontStyle.Bold) });
                        sbTemp.Clear();
                    }

                    sb.Append($"名称　　单位　　数量　　单价　　金额\n");
                    queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                    sb.Clear();
                    //"UOM":"支",
                    //"tariCode":"XY00595",
                    //"Price":"1.23",
                    //"Qty":"1",
                    //"ArcmiDesc":"氧氟沙星滴耳液[15mg:5ml]#",
                    //"Amt":"1.23元"
                    foreach (OEOrdFreItemModelBaoAnPeopleHospital detail in item.OEOrdItem)
                    {
                        string totalPrice = (!string.IsNullOrEmpty(detail.Amt)) && detail.Amt.StartsWith(".") ? ("0" + detail.Amt) : detail.Amt;
                        sb.Append($"{detail.ArcmiDesc} \n{detail.tariCode}\n　　　　{detail.UOM}　　{detail.Qty}　　{detail.Price}　　{totalPrice}\n");
                        queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                        sb.Clear();
                    }
                }
            }
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的缴费凭证。\n如需发票请到一楼人工收费处打印\n\n\n\n");
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
            sb.Append($"姓名：{patientInfo.name}　　性别：{patientInfo.sex}\n");
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