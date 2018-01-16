using YuanTu.QDArea.QingDaoSiPay.Common;

namespace YuanTu.QDArea.QingDaoSiPay.Models.Send
{
    /// <summary>
    /// 卡数据格式
    /// </summary>
   public  class CardInfo
    {
            #region 构造
            public CardInfo()
            {
                cardNo = string.Empty;
                for (int i = 0; i < memberLen.Length; i++)
                {
                    length = length + memberLen[i];
                }
            }
            //构造
            public CardInfo(string messBody)
            {

            }
            #endregion

            #region 变量
            public static int length = 0;
            private int[] memberLen = { 32 };
            public string cardNo;//个人编号
            #endregion

            #region 方法
            public string toMessage()
            {
                return Comm.rightPad(cardNo,memberLen[0]);
            }
            #endregion
        
    }
}
