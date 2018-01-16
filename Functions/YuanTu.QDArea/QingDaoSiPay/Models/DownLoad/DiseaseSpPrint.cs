using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.QDArea.QingDaoSiPay.Common;

namespace YuanTu.QDArea.QingDaoSiPay.Models.DownLoad
{
    /// <summary>
    ///  门诊大病证打印信息查询
    /// </summary>
    public class DiseaseSpPrint : ModelInterface
    {
        #region 构造
        public DiseaseSpPrint()
        {
            for (int i = 0; i < memberLen.Length; i++)
            {
                length = length + memberLen[i];
            }
        }
        //构造
        public DiseaseSpPrint(string messBody)
        {
            int startChar = 0;

            approveNo = Comm.GetSubString(messBody, startChar, memberLen[0]).Trim();//审批编号

            startChar = startChar + memberLen[0];
            string strDiseaseDetail = Comm.GetSubString(messBody, startChar, memberLen[1]);//病种
            for (int i = 0; i < 10; i++)
            {
                if (string.IsNullOrEmpty(strDiseaseDetail.Trim()))
                {
                    break;
                }

                int diseaseDetailLen = Comm.GetLength(strDiseaseDetail);

                string subStr = Comm.GetSubString(strDiseaseDetail, 0, maxLength);


                DiseaseDetail diseaseDetail = new DiseaseDetail();
                diseaseDetail.diseaseNo = Comm.GetSubString(subStr, 0, 20).Trim();
                string limit = Comm.GetSubString(subStr, 20, 10);
                if (string.IsNullOrEmpty(limit))
                {
                    limit = "0";
                }
                diseaseDetail.diseaseLimit = Convert.ToDecimal(limit);

                strDiseaseDetail = Comm.GetSubString(strDiseaseDetail, maxLength, diseaseDetailLen - maxLength);
                ListDiseaseDetail.Add(diseaseDetail);
            }

            startChar = startChar + memberLen[1];
            beginDate = Comm.GetSubString(messBody, startChar, memberLen[2]).Trim();//起始日期

            startChar = startChar + memberLen[2];
            endDate = Comm.GetSubString(messBody, startChar, memberLen[3]).Trim();//截止日期

            startChar = startChar + memberLen[3];
            memo = Comm.GetSubString(messBody, startChar, memberLen[4]).Trim();//备注

            for (int i = 0; i < memberLen.Length; i++)
            {
                length = length + memberLen[i];
            }
        }
        #endregion

        #region 变量        
        private static int maxLength = 30;
        private static int maxLoop = 20;
        private int[] memberSendLen = { 16, 8, 16 };
        private int[] memberLen = { 16, maxLength * maxLoop, 8, 8, 100 };

        public int length = 0;

        //发送数据
        /// <summary>
        /// 个人编号
        /// </summary>
        public string cardNo = string.Empty;
        /// <summary>
        /// 查询日期
        /// </summary>
        public DateTime searchDate;
        /// <summary>
        /// 审批编号
        /// </summary>
        public string approveNo = string.Empty;

        //接收数据
        /// <summary>
        /// 核准病种明细
        /// </summary>
        public class DiseaseDetail
        {
            /// <summary>
            /// 病种编码
            /// </summary>
            public string diseaseNo = string.Empty;
            /// <summary>
            /// 病种限额
            /// </summary>
            public decimal diseaseLimit = 0;
        }
        /// <summary>
        /// 核准病种明细
        /// </summary>
        public List<DiseaseDetail> ListDiseaseDetail = new List<DiseaseDetail>();
        /// <summary>
        /// 起始日期
        /// </summary>
        public string beginDate = string.Empty; //
        /// <summary>
        /// 截止日期
        /// </summary>
        public string endDate = string.Empty;//
        /// <summary>
        /// 备注
        /// </summary>
        public string memo = string.Empty;
        #endregion

        #region 方法
        public string toMessage()
        {
            string message = string.Empty;
            message = message + Comm.rightPad(cardNo, memberSendLen[0]);
            message = message + Comm.rightPad(Comm.dateToString(searchDate), memberSendLen[1]);
            message = message + Comm.rightPad(approveNo, memberSendLen[2]);
            return message;
        }
        #endregion

        #region ModelInterface 成员

        public string LastCode
        {
            set { throw new NotImplementedException(); }
        }

        public string LastTimeDate
        {
            set { throw new NotImplementedException(); }
        }

        public string[] toPara()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ISpell 成员

        public string SpellCode
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string UserCode
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string WBCode
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
