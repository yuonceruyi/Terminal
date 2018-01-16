using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.QDArea.QingDaoSiPay.Common;

namespace YuanTu.QDArea.QingDaoSiPay.Models.DownLoad
{
    /// <summary>
    /// 门诊大病证打印信息查询
    /// </summary>
    public class DiseaseSpPrintLoop : DiseaseSpPrint, DownLoadInterface
    {
        #region 变量
        private int maxLoop = 3;

        public List<DiseaseSpPrint> listDiseaseSpPrints = new List<DiseaseSpPrint>();//医保特殊疾病编码目录单体      
        private string messBody = string.Empty;

        #endregion

        #region DownLoadInterface 成员
        //一次返回信息的数据条数
        public int MaxLoop
        {
            get { return maxLoop; }
        }
        //设置返回值
        public void setReceiveInfo(string messBody)
        {
            this.messBody = messBody.Trim();
        }
        //将数据分解插入数据库
        public bool decompData(ref string approveNo, ref bool isEnd)
        {
            DiseaseSpPrint diseaseSpPrint = new DiseaseSpPrint();
            int length = diseaseSpPrint.length;
            // length = 16;
            isEnd = false;

            //将数据分解插入数据库
            for (int i = 0; i < maxLoop; i++)
            {
                if (string.IsNullOrEmpty(messBody.Trim()))
                {
                    isEnd = true;
                    return true;
                }

                int messBodylen = Comm.GetLength(messBody);
                string messBodySub = Comm.GetSubString(messBody, 0, length);

                diseaseSpPrint = new DiseaseSpPrint(messBody);
                listDiseaseSpPrints.Add(diseaseSpPrint);

                messBody = Comm.GetSubString(messBody, length, messBodylen - length);
            }
            approveNo = string.Empty;//门诊大病证打印只有一行数据
            return true;
        }

        #endregion

    }
}
