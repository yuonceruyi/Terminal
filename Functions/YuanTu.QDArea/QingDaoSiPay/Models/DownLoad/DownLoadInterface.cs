using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.QDArea.QingDaoSiPay.Models.DownLoad
{
    interface DownLoadInterface : ModelInterface
    {
        void setReceiveInfo(string messBody);
        bool decompData(ref string diseaseCode, ref bool isEnd);
        int MaxLoop
        {
            get;
        }
    }
}
