using Microsoft.Practices.ServiceLocation;
using System.ComponentModel;
using System.Windows.Forms;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Models.Payment;
using YuanTu.Core.Log;
using YuanTu.Default.Component.Tools.Models;

namespace YuanTu.QDArea.QingDaoSiPay
{
    public class SiPay
    {
        #region 变量、属性
        private static string errMsg = string.Empty;
        public static string ErrMsg
        {
            get
            {
                return errMsg;
            }
        }
        #endregion 变量、属性
        public static bool Pay(string deptCode,string balanceType)
        {
            Models.Receive.BasPerInfo perInfo = new Models.Receive.BasPerInfo();
            Models.Send.BillPaySend billPay = new Models.Send.BillPaySend();
            Models.Send.RemainSend remainSend = new Models.Send.RemainSend();
            Models.Receive.BillPayReceive billPayReceive = new Models.Receive.BillPayReceive();
            Models.Receive.RemainReceive remainReceive = new Models.Receive.RemainReceive();
            
            //读卡
            int sign = Function.ReadCard(ref perInfo);
            if (sign != 0)
            {
                Logger.POS.Error($"[社保卡]读卡异常，详情：{Function.ErrMsg}");
                //MessageBox.Show("医保读卡失败" + Function.ErrMsg);
                errMsg = Function.ErrMsg;
                return false;
            }
            billPay.CardNo = perInfo.personNo;//个人编号
            billPay.HisDeptCode = deptCode;//科室编码
            billPay.DoctCode = string.Empty;//主治医生
            billPay.CaseNo = string.Empty;//就诊卡号
            billPay.Name = balanceType;//交易名称
            var extPay = ServiceLocator.Current.GetInstance<IExtraPaymentModel>();           

            billPay.TotCost = (extPay.TotalMoney / 100M).ToString("F2");//总金额
            billPay.PayCost = (extPay.TotalMoney / 100M).ToString("F2");//交易金额
            billPay.TransCost = (extPay.TotalMoney / 100M).ToString("F2");//个人账户支付额

            #region 余额获取
            //            remainSend.CardNo = perInfo.personNo;//个人编号
            //            sign = QingDaoSi.Function.GetRemain(remainSend, ref remainReceive);
            //            if (sign != 0)
            //            {
            //                Logger.log.Info("医保卡查询余额 失败");
            //                MessageBox.Show(QingDaoSi.Function.ErrMsg);
            //                return false;
            //            }
            //            decimal cost = 0;
            //            if (string.IsNullOrEmpty(billPay.TotCost) || decimal.TryParse(billPay.TotCost, out cost))
            //            {
            //                if (Convert.ToDecimal(billPay.TotCost) > remainReceive.Remain)
            //                {
            //                    Logger.log.Info("余额不足");
            //                    MessageBox.Show("医保卡余额不足，请选择其他支付方式");
            //                    return false;
            //                }
            //            }
            //            else
            //            {
            //                Logger.log.Info("余额转换失败");
            //                MessageBox.Show("余额转换失败");
            //                return false;
            //            }
            #endregion

            sign = Function.BillPay(billPay, ref billPayReceive);
            if (sign != 0)
            {
                Logger.POS.Error($"[社保卡]支付异常，详情：{Function.ErrMsg}");
                return false;
            }
            var pret = new TransResDto
            {
                RespCode = "00",
                RespInfo = "交易成功",
                CardNo = billPay.CardNo,
                Amount = extPay.TotalMoney.ToString("0"),
                TransTime = billPayReceive.Time.Substring(9, 6), //订单支付时间
                TransDate = billPayReceive.Time.Substring(0, 8), //订单支付日期
                Trace = billPayReceive.TransNo, //交易流水号
                Ref = billPayReceive.SeqNo, //医院支付流水号
                TId = billPayReceive.Batch, //银联批次号,
                MId = billPayReceive.PosTransNo,//POS流水号  
                Memo = billPayReceive.BankReferenceNo,//银联交易参考号
            };
            //医保支付结算返回信息            
            extPay.PaymentResult = pret;
            extPay.ThridRemain = billPayReceive.Remain * 100;//账户余额
            return true;
        }
        public static bool Pay(string deptCode, string balanceType,string idNo)
        {
            Models.Receive.BasPerInfo perInfo = new Models.Receive.BasPerInfo();
            Models.Send.BillPaySend billPay = new Models.Send.BillPaySend();
            Models.Send.RemainSend remainSend = new Models.Send.RemainSend();
            Models.Receive.BillPayReceive billPayReceive = new Models.Receive.BillPayReceive();
            Models.Receive.RemainReceive remainReceive = new Models.Receive.RemainReceive();

            //读卡
            int sign = Function.ReadCard(ref perInfo);
            if (sign != 0)
            {
                Logger.POS.Error($"[社保卡]读卡异常，详情：{Function.ErrMsg}");
                errMsg = Function.ErrMsg;
                return false;
            }
            if (idNo != perInfo.IDCard)
            {
                errMsg = $"医保卡支付 非本人医保卡，提示后返回：就诊卡IDNo：{idNo} 社保卡支IDNo：{perInfo.name}";
                Logger.POS.Info(errMsg);                
                return false;
            }
            billPay.CardNo = perInfo.personNo;//个人编号
            billPay.HisDeptCode = deptCode;//科室编码
            billPay.DoctCode = string.Empty;//主治医生
            billPay.CaseNo = string.Empty;//就诊卡号
            billPay.Name = balanceType;//交易名称
            var extPay = ServiceLocator.Current.GetInstance<IExtraPaymentModel>();

            billPay.TotCost = (extPay.TotalMoney / 100M).ToString("F2");//总金额
            billPay.PayCost = (extPay.TotalMoney / 100M).ToString("F2");//交易金额
            billPay.TransCost = (extPay.TotalMoney / 100M).ToString("F2");//个人账户支付额

            #region 余额获取
            //            remainSend.CardNo = perInfo.personNo;//个人编号
            //            sign = QingDaoSi.Function.GetRemain(remainSend, ref remainReceive);
            //            if (sign != 0)
            //            {
            //                Logger.log.Info("医保卡查询余额 失败");
            //                MessageBox.Show(QingDaoSi.Function.ErrMsg);
            //                return false;
            //            }
            //            decimal cost = 0;
            //            if (string.IsNullOrEmpty(billPay.TotCost) || decimal.TryParse(billPay.TotCost, out cost))
            //            {
            //                if (Convert.ToDecimal(billPay.TotCost) > remainReceive.Remain)
            //                {
            //                    Logger.log.Info("余额不足");
            //                    MessageBox.Show("医保卡余额不足，请选择其他支付方式");
            //                    return false;
            //                }
            //            }
            //            else
            //            {
            //                Logger.log.Info("余额转换失败");
            //                MessageBox.Show("余额转换失败");
            //                return false;
            //            }
            #endregion

            sign = Function.BillPay(billPay, ref billPayReceive);
            if (sign != 0)
            {
                Logger.POS.Error($"[社保卡]支付异常，详情：{Function.ErrMsg}");
                errMsg = Function.ErrMsg;
                return false;
            }
            var pret = new TransResDto
            {
                RespCode = "00",
                RespInfo = "交易成功",
                CardNo = billPay.CardNo,
                Amount = extPay.TotalMoney.ToString("0"),
                TransTime = billPayReceive.Time.Substring(9, 6), //订单支付时间
                TransDate = billPayReceive.Time.Substring(0, 8), //订单支付日期
                Trace = billPayReceive.TransNo, //交易流水号
                Ref = billPayReceive.SeqNo, //医院支付流水号
                TId = billPayReceive.Batch, //银联批次号,
                MId = billPayReceive.PosTransNo,//POS流水号  
                Memo = billPayReceive.BankReferenceNo,//银联交易参考号
            };
            //医保支付结算返回信息            
            extPay.PaymentResult = pret;
            extPay.ThridRemain = billPayReceive.Remain * 100;//账户余额
            return true;
        }

    }
}
