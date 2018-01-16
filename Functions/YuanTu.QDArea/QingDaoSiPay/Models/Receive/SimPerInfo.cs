using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.QDArea.QingDaoSiPay.Common;

namespace YuanTu.QDArea.QingDaoSiPay.Models.Receive
{
    /// <summary>
    /// 个人简单信息
    /// </summary>
    public class SimPerInfo
    {
        #region 构造
        public SimPerInfo()
        {
            personNo = string.Empty;
            IDCard = string.Empty;
            Name = string.Empty;

            length = 0;
            for (int i = 0; i < memberLen.Length; i++)
            {
                length = length + memberLen[i];
            }
        }
        //构造
        public SimPerInfo(string messBody)
        {
            int startChar = 0;
            messBody.Trim();

            personNo = Comm.GetSubString(messBody, startChar, memberLen[0]).Trim();

            startChar = startChar + memberLen[0];
            IDCard = Comm.GetSubString(messBody, startChar, memberLen[1]).Trim();

            startChar = startChar + memberLen[1];
            Name = Comm.GetSubString(messBody, startChar, memberLen[2]).Trim();

            length = 0;
            for (int i = 0; i < memberLen.Length; i++)
            {
                length = length + memberLen[i];
            }
        }
        #endregion

        #region 变量
        private int[] memberLen = { 16, 18, 20 };
        static public int length = 0;
        public string personNo;//个人编号
        public string IDCard;//身份证
        public string Name;//姓名
        #endregion

        #region 方法
        public string toMessage()
        {
            return null;
        }
        #endregion  
    }
}
