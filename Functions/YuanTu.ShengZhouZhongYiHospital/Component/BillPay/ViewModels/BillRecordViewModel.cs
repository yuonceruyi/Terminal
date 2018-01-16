using Microsoft.Practices.ServiceLocation;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Core.Reporter;
using YuanTu.Core.Services.PrintService;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.ShengZhouZhongYiHospital.Component.BillPay.Services;
using YuanTu.ShengZhouZhongYiHospital.HisNative.Models;

using BillPayModel = YuanTu.ShengZhouZhongYiHospital.Component.BillPay.Models.BillPayModel;

namespace YuanTu.ShengZhouZhongYiHospital.Component.BillPay.ViewModels
{
    public class BillRecordViewModel : YuanTu.Default.Component.BillPay.ViewModels.BillRecordViewModel
    {
        [Dependency]
        public IBusinessConfigManager BusinessConfigManager { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            TipMsg = "我要缴费";
            List<缴费明细信息> billItem = new List<缴费明细信息>();
            BillRecordModel.Res获取缴费概要信息.data.ForEach((p) => { billItem = billItem.Concat(p.billItem).ToList(); });
            Collection = new List<PageData>
            {
                new PageData
                {
                    CatalogContent =
                        $"总共金额 {BillRecordModel.Res获取缴费概要信息.data.Sum(t => decimal.Parse(t.billFee)).In元()}",
                    List = billItem,
                }
            };
            BillCount = null;
            TotalAmount = null;


        }

        protected override Result Confirm()
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();

            return DoCommand(lp =>
            {
                var bmModel = BillPayModel as BillPayModel;
                lp.ChangeText("正在进行缴费，请稍候...");

                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                var billRecordModel = GetInstance<IBillRecordModel>();
                int ZHIFULX = 1;
                var tranNo = string.Empty;
                if (PaymentModel.PayMethod == PayMethod.银联)
                {
                    tranNo = (extraPaymentModel.PaymentResult as TransResDto).Ref;
                    ZHIFULX = 1;
                }
                else if (PaymentModel.PayMethod == PayMethod.支付宝)
                {
                    tranNo = (extraPaymentModel.PaymentResult as 订单状态).outTradeNo.Substring(4);
                    ZHIFULX = 2;
                }
                else if (PaymentModel.PayMethod == PayMethod.微信支付)
                {
                    tranNo = (extraPaymentModel.PaymentResult as 订单状态).outTradeNo.Substring(4);
                    ZHIFULX = 3;
                }
                else
                {
                    tranNo = BusinessConfigManager.GetFlowId("缴费");
                }
                var 处方单号 = new StringBuilder();
                decimal 总费用 = 0;
                billRecordModel.Res获取缴费概要信息.data.ForEach((p) =>
                {
                    处方单号.Append($"{p.billType}/{p.billNo},");
                    总费用 += decimal.Parse(p.billFee);
                });
                bmModel.ReqHis缴费结算 = new ReqHIS缴费结算()
                {
                    患者唯一标识 = patientInfo.patientId,
                    姓名 = patientInfo.name,
                    就诊序号 = "",
                    医保类型 = patientInfo.extend,
                    处方单号 = 处方单号.ToString().Trim(','),
                    总费用 = 总费用.ToString().InRMB(),
                    银行结算流水号 = tranNo,
                    银行支付费用 = "",
                    程序名 = "",
                    操作科室 = "",
                    终端编号 = FrameworkConst.OperatorId,
                    支付类型 = ZHIFULX.ToString()
                };
                bmModel.ResHis缴费结算 = HisHandleEx.执行HIS缴费结算(bmModel.ReqHis缴费结算);
                if (bmModel.ResHis缴费结算.IsSuccess)
                {
                    lp.ChangeText("正在解除处方锁定，请稍候...");
                    PrescriptionLock.RemoveLock(); //删除墓碑
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "缴费成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分缴费",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = BillPayPrintables(),
                        TipImage = "提示_凭条"
                    });
                    try
                    {
                        lp.ChangeText("正在上送交易数据,请稍后...");
                        if (PaymentModel.Self != 0)
                        {
                            自费交易记录同步到his系统();
                        }
                        if (PaymentModel.Insurance != 0)
                        {
                            医保交易记录同步到his系统(tranNo);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Main.Error($"数据统一上传失败:{e.Message}");
                    }
                    ExtraPaymentModel.Complete = true;
                    //lp.ChangeText("正在获取导引数据，请稍后...");
                    //导引单();
                    Navigate(A.JF.Print);
                    return Result.Success();
                }
                ReportService.HIS请求失败($"嵊州缴费结算失败,HIS服务返回:{bmModel.ResHis缴费结算.Message}", null);
                ShowAlert(false, "温馨提示", $"缴费失败：{bmModel.ResHis缴费结算?.Message}");
                ExtraPaymentModel.Complete = true;
                Logger.Net.Info($"[嵊州本地服务，缴费结算] 结算失败");
                return Result.Fail(bmModel.ResHis缴费结算?.RetCode ?? -100, bmModel.ResHis缴费结算?.Message);
            }).Result;
        }

        protected override void Do()
        {
            DoCommand(lp =>
            {
                var bmModel = BillPayModel as BillPayModel;


                lp.ChangeText("正在进行缴费预结算，请稍候...");
                if (缴费预结算().IsSuccess)
                {
                    var psyinfo = bmModel.ResHis缴费预结算;
                    ChangeNavigationContent(SelectData.CatalogContent);
                    PaymentModel.Self = decimal.Parse(psyinfo.个人现金支付金额) * 100;
                    PaymentModel.Insurance = decimal.Parse(psyinfo.医保支付金额) * 100;
                    PaymentModel.Total = decimal.Parse(psyinfo.总金额) * 100;
                    PaymentModel.NoPay = PaymentModel.Self == 0;
                    PaymentModel.ConfirmAction = Confirm;
                    PaymentModel.MidList = new List<PayInfoItem>
                    {
                        new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                        new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                        new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
                    };
                    Next();
                }
            });
        }

        private Result 缴费预结算()
        {
            var bmModel = BillPayModel as BillPayModel;

            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var billRecordModel = GetInstance<IBillRecordModel>();
            var 处方单号 = new StringBuilder();
            decimal 总费用 = 0;
            billRecordModel.Res获取缴费概要信息.data.ForEach((p) =>
            {
                处方单号.Append($"{p.billType}/{p.billNo},");
                总费用 += decimal.Parse(p.billFee);
            });
            bmModel.ReqHIS缴费预结算 = new ReqHIS缴费预结算()
            {
                患者唯一标识 = patientInfo.patientId,
                姓名 = patientInfo.name,
                就诊序号 = "",
                医保类型 = patientInfo.extend,
                处方单号 = 处方单号.ToString().Trim(','),
                总费用 = 总费用.ToString().InRMB(),
                银行结算流水号 = "",
                银行支付费用 = "",
                程序名 = "",
                操作科室 = "",
                终端编号 = FrameworkConst.OperatorId,
                支付类型 = "1"
            };
            bmModel.ResHis缴费预结算 = HisHandleEx.执行HIS缴费预结算(bmModel.ReqHIS缴费预结算);
            if (bmModel.ResHis缴费预结算.IsSuccess)
            {
                return Result.Success();
            }
            ReportService.HIS请求失败($"嵊州缴费预结算失败,HIS服务返回:{bmModel.ResHis缴费预结算.Message}", null);
            ShowAlert(false, "缴费信息预结算", $"缴费信息预结算失败", debugInfo: bmModel.ResHis缴费预结算.Message);
            return Result.Fail(bmModel.ResHis缴费预结算.Message);


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
                    remarks = "缴费",
                };
                FillRechargeRequest(req);
                var res = DataHandlerEx.交易记录同步到his系统(req);
                if (res.success)
                {
                    Logger.Net.Info($"交易记录同步到his系统成功");
                }
                else
                {
                    Logger.Net.Info($"交易记录同步到his系统失败:{res.msg}");
                }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"交易记录同步到his系统失败异常:{ex.Message}");
                return;
            }
        }

        private void 医保交易记录同步到his系统(string tranNo)
        {
            try
            {
                Logger.Net.Info($"开始交易记录同步到his系统");
                var req = new req交易记录同步到his系统
                {
                    platformId = FrameworkConst.OperatorId,
                    hisPatientId = CardModel?.CardNo,
                    cardNo = CardModel?.CardNo,
                    idNo = PatientModel?.当前病人信息?.idNo,
                    patientName = PatientModel?.当前病人信息?.name,
                    tradeType = "2",
                    cash = PaymentModel?.Insurance.ToString("0"),
                    operId = FrameworkConst.OperatorId,
                    bankTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tradeMode = "医保支付",
                    inHos = "1",
                    remarks = "挂号",
                    payAccountNo = "医保账户",
                    tradeTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    transNo = tranNo
                };
                var res = DataHandlerEx.交易记录同步到his系统(req);
                if (res.success)
                {
                    Logger.Net.Info($"交易记录同步到his系统成功");
                }
                else
                {
                    Logger.Net.Info($"交易记录同步到his系统失败:{res.msg}");
                }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"交易记录同步到his系统失败异常:{ex.Message}");
                return;
            }
        }

        public static string GetEnumDescription(PayMethod value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
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

        protected override Queue<IPrintable> BillPayPrintables()
        {
            var bmModel = BillPayModel as BillPayModel;
            var payres = bmModel.ResHis缴费结算;
            var dyd = DeserializeToObject(bmModel.ResHis缴费结算.导引单信息);
            var queue = PrintManager.NewQueue("(缴费凭条)");
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            var tranNo = string.Empty;
            if (PaymentModel.PayMethod == PayMethod.银联)
            {
                tranNo = (extraPaymentModel.PaymentResult as TransResDto).Ref;
            }
            else if (PaymentModel.PayMethod == PayMethod.支付宝 || PaymentModel.PayMethod == PayMethod.微信支付)
            {
                tranNo = (extraPaymentModel.PaymentResult as 订单状态).outTradeNo;
            }
            var sb = new StringBuilder();
            sb.Append($"------------------------------------\n");
            sb.Append($"终端流水：{tranNo}\n");
            sb.Append($"电 脑 号：{payres.电脑号.SafeSubstring(0, 8)}\n");
            sb.Append($"缴费时间：{DateTimeCore.Now.ToString("yyyy-MM-dd   HH:mm:ss")}\n");
            sb.Append($"姓   名：{patientInfo.name}\n");
            sb.Append($"就诊卡号：{patientInfo?.patientId}\n");
            sb.Append($"------------------------------------\n");
            sb.Append($"费用总额：{PaymentModel.Total.In元()}\n");
            sb.Append($"个人支付：{PaymentModel.Self.In元()}\n");
            sb.Append($"医保支付：{PaymentModel.Insurance.In元()}\n");
            sb.Append($"其他支付：0.00元\n");
            sb.Append($"------------------------------------\n");
            if (!string.IsNullOrEmpty(payres.取药地点))
            {
                sb.Append($"取药窗口：{payres.取药地点}\n");
            }
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            if (dyd != null)
            {
                var dic = new Dictionary<string, List<List<string>>>();
                foreach (var info in dyd.infos)
                {
                    if (info.zxks.Contains("1西"))
                    {
                        info.zxks = info.zxks.Replace("1", "");
                    }
                    if (info.zxks.Contains("2草"))
                    {
                        info.zxks = info.zxks.Replace("2", "");
                    }
                    var index = info.xmmc.IndexOf('（');
                    if (index >= 0)
                    {
                        info.xmmc = info.xmmc.Substring(0, index);
                    }
                    if (!dic.Keys.Contains(info.zxks))
                    {
                        dic.Add(info.zxks, new List<List<string>> { new List<string> { info.xmmc, info.sl, info.dj } });
                    }
                    else
                    {
                        dic[info.zxks].Add(new List<string> { info.xmmc, info.sl, info.dj });
                    }
                }
                foreach (var key in dic.Keys)
                {
                    queue.Enqueue(new PrintItemText { Text = key + ":\n" });
                    int i = 1;
                    foreach (var value in dic[key])
                    {
                        queue.Enqueue(new PrintItemTriText($"{i++}." + value[0], value[1], value[2]));
                    }
                }
            }
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 8, FontStyle.Regular) });
            sb.Clear();
            sb.Append($"--------------------------------\n");
            sb.Append($"打印时间：{DateTimeCore.Now.ToString("HH:mm:ss")}\n");
            sb.Append($"机器号：{FrameworkConst.OperatorId}\n");
            sb.Append($"1.如需要发票请携带就诊卡和此凭条到咨询台打印\n");
            sb.Append($"2.发票补打有效期为半年，仅补打一次\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 9, FontStyle.Regular) });
            sb.Clear();
            if (!string.IsNullOrEmpty(patientInfo.patientId))
            {
                var image = BarCode128.GetCodeImage(patientInfo.patientId, BarCode.Code128.Encode.Code128A);
                queue.Enqueue(new PrintItemImage
                {
                    Align = ImageAlign.Center,
                    Image = image,
                    Height = image.Height / 2f,
                    Width = image.Width /2f
                });
            }
            sb.AppendLine(patientInfo?.patientId);
            queue.Enqueue(new PrintItemText { Text = sb.ToString(),StringFormat = PrintConfig.Center, Font = new Font("微软雅黑", 9, System.Drawing.FontStyle.Regular) });
            sb.Clear();
            sb.AppendLine(".");
            sb.AppendLine(".");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 9, FontStyle.Regular) });
            return queue;
        }

        private static dyd DeserializeToObject(string xml)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(dyd));
                StringReader reader = new StringReader(xml);
                dyd myObject = (dyd)serializer.Deserialize(reader);
                reader.Close();
                return myObject;
            }
            catch (Exception e)
            {
                Logger.Net.Info($"[嵊州本地服务，导引单格式转换失败] {e}");
                return null;
            }
        }
    }

    public class PrintItemTriText : PrintItemText
    {
        public string Text2;
        public string Text3;
        public float Text3X = PrintConfig.Default3X;

        public PrintItemTriText()
        {
        }

        public PrintItemTriText(string t1, string t2, string t3)
        {
            Text = t1;
            Text2 = t2;
            Text3 = t3;
        }

        public PrintItemTriText(string t1, string t2, string t3, float t4)
        {
            Text = t1;
            Text2 = t2;
            Text3 = t3;
            Text3X = t4;
        }

        public override float GetHeight(Graphics g, float w)
        {
            return g.MeasureString(Text, Font, (int)w).Height;
        }

        public override float Print(Graphics g, float Y, float w)
        {
            var h = GetHeight(g, w);
            g.DrawString(Text, Font, Brushes.Black, new RectangleF(10, Y, 170, h));
            g.DrawString(Text2, Font, Brushes.Black, new RectangleF(180, Y, 40, h));
            g.DrawString(Text3, Font, Brushes.Black, new RectangleF(220, Y, 70, h));
            return h;
        }
    }
}
