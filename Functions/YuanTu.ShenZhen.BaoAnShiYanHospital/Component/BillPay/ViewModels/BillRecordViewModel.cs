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

namespace YuanTu.ShenZhen.BaoAnShiYanHospital.Component.BillPay.ViewModels
{
    public class BillRecordViewModel : YuanTu.Default.Component.BillPay.ViewModels.BillRecordViewModel
    {
        [Dependency]
        public IYBModel YBModel { get; set; }


        [Dependency]
        public IYBService YB_Service { get; set; }


        [Dependency]
        public IAccountingService Account_Service { get; set; }


        protected override void Do()
        {
            BillRecordModel.所选缴费概要 = SelectData.Tag.As<缴费概要信息>();
            ChangeNavigationContent(SelectData.CatalogContent);
            var recordInfo = BillRecordModel.所选缴费概要;

            /*
             * 石岩的需求：
             * 使用就诊卡缴费，验证是否医保
             * 
            if (CardModel.CardType == CardType.就诊卡 && recordInfo.billType == "普通医保")
            {
                ShowAlert(false, "缴费信息确认", "使用就诊卡的医保病人，请到人工窗口缴费", 10);
                Navigate(A.Home);
                return;
            }
            */

            //不支持在自助机上缴费，默认不支持
            bool NotSupportSelfPay = true;

            //自费
            if (recordInfo.billType == "自费")
            {
                NotSupportSelfPay = false;
            }

            if (recordInfo.billType == "普通医保" && YBModel.参保类型 == ShenZhenArea.Enums.Cblx.基本医疗保险一档)
            {
                NotSupportSelfPay = false;
            }

            if (NotSupportSelfPay)
            {
                //ShowAlert(false, "缴费信息确认", "目前自助机仅支持以下三种类型账单缴费\n基本医疗保险（一档）、医疗保险二档（少儿）、自费", 10);
                ShowAlert(false, "缴费信息确认", "目前自助机仅支持以下类型账单缴费\n基本医疗保险（一档）、自费", 10);
                Navigate(A.Home);
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
                    //new PayInfoItem("科室：", recordInfo.deptName),
                    //new PayInfoItem("医生：", recordInfo.doctName)
                };

                PaymentModel.RightList = new List<PayInfoItem>
                {
                    //new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                    //new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
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
                        //new PayInfoItem("科室：", recordInfo.deptName),
                        //new PayInfoItem("医生：", recordInfo.doctName)
                    };

                    if (PaymentModel.Insurance > 0)
                    {
                        PaymentModel.RightList = new List<PayInfoItem>
                    {
                        new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                        new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                        new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
                    };
                    }
                    else
                    {
                        PaymentModel.RightList = new List<PayInfoItem>
                    {
                        new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
                    };
                    }
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

                BillPayModel.Req缴费结算 = new req缴费结算
                {
                    patientId = patientInfo.patientId,
                    patientName = patientInfo.name,
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,
                    operId = FrameworkConst.OperatorId,
                    tradeMode = PaymentModel.PayMethod == PayMethod.未知 ? PayMethod.现金.GetEnumDescription(): PaymentModel.PayMethod.GetEnumDescription(),
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
            var billPay = BillPayModel.Res缴费结算.data;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var record = BillRecordModel.所选缴费概要;
            //var queue = PrintManager.NewQueueTopBarcode("导诊及清单费用", patientInfo.patientId);
            var queue = PrintManager.NewQueue("导诊及清单费用");
            var sb = new StringBuilder();
            #region 登记号条码
            var imagePatient = BarCode128.GetCodeImage(patientInfo.patientId, BarCode.Code128.Encode.Code128A);
            queue.Enqueue(new PrintItemImage
            {
                Align = ImageAlign.Center,
                Image = imagePatient,
                Height = imagePatient.Height / 1.5f,
                Width = imagePatient.Width / 1.5f
            });

            #endregion
            //sb.Append($"登记号:{patientInfo.patientId}   卡号:{CardModel.CardNo}\n");
            sb.Append($"登记号:{patientInfo.patientId}\n");
            sb.Append($"卡号:{CardModel.CardNo}\n");
            if (!string.IsNullOrEmpty(patientInfo.phone))
            {
                sb.Append($"手机号码:{patientInfo.phone}\n");
            }
            sb.Append($"姓名:{patientInfo.name}　年龄:{patientInfo.birthday.Age()}岁　性别:{patientInfo.sex}\n");
            sb.Append($"病人类型：{patientInfo.patientType ?? "自费"}   就诊科室：{record.deptName}\n就诊医生：{record.doctName}\n");
            //sb.Append($"就诊医生：{record.doctName}     收费时间{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"收费时间{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"----------------------------------\n");
            //sb.Append($"收费金额：{PaymentModel.Total.In元()}\n");
            //sb.Append($"----------------------------------\n");

            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            decimal selfPartFee = 0;
            if (!string.IsNullOrEmpty(billPay?.extend))
            {
                List<OEOrdFreeModelBaoAnShiYanPeopleHospital> ShouFeiResult = JsonConvert.DeserializeObject<List<OEOrdFreeModelBaoAnShiYanPeopleHospital>>(billPay?.extend);

                OEOrdFreeModelBaoAnShiYanPeopleHospital 诊查费 = null;
                foreach (OEOrdFreeModelBaoAnShiYanPeopleHospital item in ShouFeiResult)
                {
                    foreach (var detail in item.OEOrdItem)
                    {
                        if (detail.ArcmiDesc.Contains("诊查费"))
                        {
                            诊查费 = item;
                            break;
                        }
                    }
                    if (诊查费 != null)
                    {
                        break;
                    }
                }


                if (诊查费 != null)   //诊查费放到所有明细的最后
                {
                    ShouFeiResult.Remove(诊查费);
                    ShouFeiResult.Add(诊查费);
                }

                int itemIndex = 1;
                foreach (OEOrdFreeModelBaoAnShiYanPeopleHospital item in ShouFeiResult)
                {
                    queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                    sb.Clear();
                    if (item != 诊查费)   //诊查费不加前往科室
                    {
                        sb.Append($"{itemIndex}、请前往科室:{item.locDesc}\n地点:{item.Adress}\n");
                        itemIndex++;
                        queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 10, FontStyle.Bold) });
                        sb.Clear();
                    }

                    sb.Append($"名称　单位　数量　单价　金额　是否医保\n");
                    queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                    sb.Clear();
                    foreach (var detail in item.OEOrdItem)
                    {
                        int totalPrice = Convert.ToInt32(Convert.ToDecimal(((!string.IsNullOrEmpty(detail.Amt)) && detail.Amt.StartsWith(".") ? ("0" + detail.Amt) : detail.Amt).Replace("元", "")) * 100);
                        sb.Append($"{detail.ArcmiDesc}\n");
                        sb.Append($"{detail.tariCode}　{detail.UOM}　{Convert.ToInt32(detail.Qty)}　{detail.Price}　{totalPrice.ToString().In元()}　{(detail.xmlb == "医保" ?"是": "否")}\n");
                        queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                        sb.Clear();
                        if (!string.IsNullOrEmpty(detail.xmlb))
                        {
                            if (detail.xmlb != "医保")
                            {
                                selfPartFee += totalPrice;
                            }
                        }
                    }
                    sb.Append($"----------------------------\n");
                }
            }

            sb.Append($"合计金额：{PaymentModel.Total.In元()}\n");
            if (record.billType != "自费")
            {
                //sb.Append($"实收现金：{PaymentModel.Self.In元()}\n");
                //sb.Append($"------实收现金：{PaymentModel.Self.In元()}\n");
                if (PaymentModel.Self > 0)
                    sb.Append($"------支付方式：{PaymentModel.PayMethod}\n");
                sb.Append($"----------自费部分：{selfPartFee.In元()}\n");
                sb.Append($"----------目录内自费：{(PaymentModel.Self - selfPartFee).In元()}\n");
                sb.Append($"------记账：{PaymentModel.Insurance.In元()}\n");
            }
            sb.Append($"自助机编号：{FrameworkConst.OperatorId}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();

            sb.Append($"请妥善保管好您的缴费凭证，如需打印发票请到门诊人工收费窗口\n带☆标示项目属于6岁（含）以下儿童在原价上加收30%");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 10, FontStyle.Bold) });
            sb.Clear();
            return queue;
        }


        protected virtual Queue<IPrintable> BillErrSheBaoPayPrintables()
        {
            var billPay = BillPayModel.Res缴费结算.data;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var record = BillRecordModel.所选缴费概要;
            var queue = PrintManager.NewQueue("社保单边账");
            //var queue = PrintManager.NewQueueTopBarcode("社保单边账", patientInfo.patientId);
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
            var billPay = BillPayModel.Res缴费结算.data;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var queue = PrintManager.NewQueue("门诊费用缴费");
            //var queue = PrintManager.NewQueueTopBarcode("门诊费用缴费", patientInfo.patientId);
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