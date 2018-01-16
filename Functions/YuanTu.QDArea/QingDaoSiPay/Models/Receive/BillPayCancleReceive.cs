using YuanTu.QDArea.QingDaoSiPay.Common;

namespace YuanTu.QDArea.QingDaoSiPay.Models.Receive
{
    /// <summary>
    /// 消费撤销
    /// </summary>
    public class BillPayCancleReceive
    {
        #region 构造
        //构造
        public BillPayCancleReceive(string messBody)
        {
            int startChar = 0;
            messBody.Trim();

            State = Comm.GetSubString(messBody, startChar, memberLen[0]).Trim();//银联交易状态

            startChar = startChar + memberLen[0];
            Result = Comm.GetSubString(messBody, startChar, memberLen[1]).Trim();// 银联交易结果    
            startChar = startChar + memberLen[1];
            SeqNo = Comm.GetSubString(messBody, startChar, memberLen[2]).Trim();//医院支付流水号
            startChar = startChar + memberLen[2];
            TransNo = Comm.GetSubString(messBody, startChar, memberLen[3]).Trim();//交易流水号
            startChar = startChar + memberLen[3];
            string strCost = Comm.GetSubString(messBody, startChar, memberLen[4]).Trim();//交易金额
            Cost = decimal.Parse(strCost);          
            startChar = startChar + memberLen[4];
            Time = Comm.GetSubString(messBody, startChar, memberLen[5]).Trim();//退款时间
         
           
            length = 0;
            for (int i = 0; i < memberLen.Length; i++)
            {
                length = length + memberLen[i];
            }
        }
        #endregion

        #region 变量
        private int[] memberLen = { 4, 100, 32, 100, 10, 16 };
        static public int length = 0;

        public string State=string.Empty;//银联交易状态    
        public string Result=string.Empty;// 银联交易结果
        public string SeqNo=string.Empty;//医院支付流水号
        public string TransNo=string.Empty;//退款交易流水号
        public decimal Cost; //交易金额        
        public string Time=string.Empty;//退款时间       
        #endregion
    }
}
