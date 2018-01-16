using YuanTu.QDArea.QingDaoSiPay.Common;

namespace YuanTu.QDArea.QingDaoSiPay.Models.Send
{
    /// <summary>
    /// 消费撤销
    /// </summary>
    public class BillPayCancelSend
    {
        #region 构造
        public BillPayCancelSend()
        {
            for (int i = 0; i < memberLen.Length; i++)
            {
                length = length + memberLen[i];
            }
        }
        #endregion

        #region 变量
        public static int length = 0;
        private int[] memberLen = { 32, 32, 100, 10, 4 };

        /// <summary>
        /// 人员编号32	读社保卡人员编号
        /// </summary>
        public string CardNo=string.Empty;
        /// <summary>
        /// 医院支付流水号
        /// </summary>
        public string SeqNo;
        /// <summary>
        /// 交易流水号
        /// </summary>
        public string TransNo;     
        /// <summary>
        ///  交易金额	10	数字格式，当前订单交易待支付额
        /// </summary>
        public decimal TransCost;
        /// <summary>
        /// 社保卡有效期	4	可缺省
        /// </summary>
        public string Period = string.Empty;        
        #endregion

        #region 方法
        public string toMessage()
        {
           string message = string.Empty;
            message = message + Comm.rightPad(CardNo, memberLen[0]);
            message = message + Comm.rightPad(SeqNo, memberLen[1]);
            message = message + Comm.rightPad(TransNo, memberLen[2]);
            message = message + Comm.leftPad(TransCost.ToString("0.00"), memberLen[3]);
            message = message + Comm.rightPad(Period, memberLen[4]);               
            return message;
        }
        #endregion
    }
}
