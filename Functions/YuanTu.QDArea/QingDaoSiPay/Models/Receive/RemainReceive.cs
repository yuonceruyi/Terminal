using YuanTu.QDArea.QingDaoSiPay.Common;

namespace YuanTu.QDArea.QingDaoSiPay.Models.Receive
{
    public class RemainReceive
    {
        #region 构造
        public RemainReceive()
        {
            length = 0;
            for (int i = 0; i < memberLen.Length; i++)
            {
                length = length + memberLen[i];
            }
        }
        //构造
        public RemainReceive(string messBody)
        {
            int startChar = 0;
            messBody.Trim();

            State = Comm.GetSubString(messBody, startChar, memberLen[0]).Trim();//银联交易状态

            startChar = startChar + memberLen[0];
            Result = Comm.GetSubString(messBody, startChar, memberLen[1]).Trim();// 银联交易结果    
            startChar = startChar + memberLen[1];
            string strRemain = Comm.GetSubString(messBody, startChar, memberLen[2]).Trim();//医院支付流水号
            if (string.IsNullOrWhiteSpace(strRemain))
            {
                Remain = 0;
            }
            else
            {
                Remain = decimal.Parse(strRemain);
            }
            length = 0;
            for (int i = 0; i < memberLen.Length; i++)
            {
                length = length + memberLen[i];
            }
        }
        #endregion

        #region 变量
        private int[] memberLen = { 4, 100, 10 };
        static public int length = 0;

        public string State = string.Empty;//银联交易状态    
        public string Result = string.Empty;// 银联交易结果
        public decimal Remain; //社保卡余额      
        #endregion

    }
}
