using YuanTu.QDArea.QingDaoSiPay.Common;

namespace YuanTu.QDArea.QingDaoSiPay.Models.Send
{
    /// <summary>
    /// 4.1	(YL001) 订单支付
    /// </summary>
    public class BillPaySend
    {
        #region 构造
        public BillPaySend()
        {
            for (int i = 0; i < memberLen.Length; i++)
            {
                length = length + memberLen[i];
            }
        }
        #endregion

        #region 变量
        public static int length = 0;
        private int[] memberLen = { 32, 4, 10, 30, 100, 10, 10, 10 };

        /// <summary>
        /// 人员编号32	读社保卡人员编号
        /// </summary>
        public string CardNo=string.Empty;
        /// <summary>
        /// 科室编码4
        /// </summary>
        public string DeptCode = string.Empty;
        /// <summary>
        /// 主治医师	10	可缺省
        /// </summary>
        public string DoctCode = string.Empty;
        /// <summary>
        /// 就诊卡号	30	医院就诊卡卡号，可缺省
        /// </summary>
        public string CaseNo = string.Empty;
        /// <summary>
        /// 交易名称	100	例如：体检费用、购买药品等
        /// </summary>
        public string Name = string.Empty;
        /// <summary>
        ///  总金额	10	数字格式，本次费用发生总金额
        /// </summary>
        public string TotCost = string.Empty;
        /// <summary>
        ///  交易金额	10	数字格式，当前订单交易待支付额
        /// </summary>
        public string TransCost = string.Empty;
        /// <summary>
        /// 个人账户支付额	10	数字格式，社保卡支付金额
        /// </summary>
        public string PayCost = string.Empty;

        //非发送字段
        /// <summary>
        /// 科室编码4
        /// </summary>
        public string HisDeptCode = string.Empty;
        #endregion

        #region 方法
        public string toMessage()
        {
           string message = string.Empty;
            message = message + Comm.rightPad(CardNo, memberLen[0]);
            message = message + Comm.rightPad(DeptCode, memberLen[1]);
            message = message + Comm.rightPad(DoctCode, memberLen[2]);
            message = message + Comm.rightPad(CaseNo, memberLen[3]);
            message = message + Comm.rightPad(Name, memberLen[4]);
            message = message + Comm.leftPad(TotCost, memberLen[5]);
            message = message + Comm.leftPad(TransCost, memberLen[6]);
            message = message + Comm.leftPad(PayCost, memberLen[7]);       
            return message;
        }
        #endregion
    }
}
