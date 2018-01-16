using YuanTu.QDArea.QingDaoSiPay.Common;

namespace YuanTu.QDArea.QingDaoSiPay.Models.Receive
{
    /// <summary>
    /// 4.1	(YL001) 订单支付
    /// </summary>
    public class BillPayReceive
    {
        #region 构造
        public BillPayReceive()
        {
            length = 0;
            for (int i = 0; i < memberLen.Length; i++)
            {
                length = length + memberLen[i];
            }
        }
        //构造
        public BillPayReceive(string messBody)
        {
            int startChar = 0;
            decimal dCost = 0;
            decimal dRemain = 0;
            bool bRtn = false;
            messBody.Trim();

            State = Comm.GetSubString(messBody, startChar, memberLen[0]).Trim();//银联交易状态

            startChar = startChar + memberLen[0];
            Result= Comm.GetSubString(messBody, startChar, memberLen[1]).Trim();// 银联交易结果  
            startChar = startChar + memberLen[1];
            SeqNo = Comm.GetSubString(messBody, startChar, memberLen[2]).Trim();//医院支付流水号
            startChar = startChar + memberLen[2];
            string strCost= Comm.GetSubString(messBody, startChar, memberLen[3]).Trim();//交易金额
            bRtn = decimal.TryParse(strCost, out dCost);
            if (bRtn)
            {
                Cost = dCost;
            }
            startChar = startChar + memberLen[3];
            string strRemain = Comm.GetSubString(messBody, startChar, memberLen[4]).Trim();//账户余额
            bRtn = decimal.TryParse(strRemain, out dRemain);
            if (bRtn)
            {
                Remain = dRemain;
            }
            startChar = startChar + memberLen[4];
            Time = Comm.GetSubString(messBody, startChar, memberLen[5]).Trim();//订单支付时间
            startChar = startChar + memberLen[5];
            TransNo = Comm.GetSubString(messBody, startChar, memberLen[6]).Trim();//交易流水号
            startChar = startChar + memberLen[6];
            Batch = Comm.GetSubString(messBody, startChar, memberLen[7]).Trim();//银联批次号
            startChar = startChar + memberLen[7];
            PosTransNo = Comm.GetSubString(messBody, startChar, memberLen[8]).Trim();//POS流水号
            startChar = startChar + memberLen[8];
            BankReferenceNo = Comm.GetSubString(messBody, startChar, memberLen[9]).Trim();//银联交易参考号
            startChar = startChar + memberLen[9];
            IDNo = Comm.GetSubString(messBody, startChar, memberLen[10]).Trim();//身份证号

            length = 0;
            for (int i = 0; i < memberLen.Length; i++)
            {
                length = length + memberLen[i];
            }
        }
        #endregion

        #region 变量
        private int[] memberLen = { 4, 100, 32, 10, 10, 16, 100, 100, 6, 12, 18 };
        static public int length = 0;

        /// <summary>
        /// 银联交易状态
        /// </summary>
        public string State;
        /// <summary>
        /// 银联交易结果
        /// </summary>
        public string Result;
        /// <summary>
        /// 医院支付流水号
        /// </summary>
        public string SeqNo;
        /// <summary>
        /// 交易金额
        /// </summary>
        public decimal Cost;
        /// <summary>
        /// 账户余额
        /// </summary>
        public decimal Remain;
        /// <summary>
        /// 订单支付时间
        /// </summary>
        public string Time;
        /// <summary>
        /// 交易流水号
        /// </summary>
        public string TransNo;
        /// <summary>
        /// 银联批次号
        /// </summary>
        public string Batch;
        /// <summary>
        /// POS流水号
        /// </summary>
        public string PosTransNo;
        /// <summary>
        /// 银联交易参考号
        /// </summary>
        public string BankReferenceNo;
        /// <summary>
        /// 身份证号
        /// </summary>
        public string IDNo;
        #endregion
    }
}
