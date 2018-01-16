using YuanTu.QDArea.QingDaoSiPay.Common;

namespace YuanTu.QDArea.QingDaoSiPay.Models.Send
{
    public class RemainSend
    {
        #region 构造
        public RemainSend()
        {
            for (int i = 0; i < memberLen.Length; i++)
            {
                length = length + memberLen[i];
            }
        }
        #endregion

        #region 变量
        public static int length = 0;
        private int[] memberLen = { 32, 4 };

        /// <summary>
        /// 人员编号32	读社保卡人员编号
        /// </summary>
        public string CardNo = string.Empty;

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
            message = message + Comm.rightPad(Period, memberLen[1]);
            return message;
        }
        #endregion
    }
}
