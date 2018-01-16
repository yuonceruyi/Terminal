using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.QDArea.QingDaoSiPay.Models.DownLoad
{
    interface ModelInterface
    {
        string LastCode
        {
            set;
        }
        string LastTimeDate
        {
            set;
        }
        //for发送消息
        string toMessage();

        //for接收消息
        string[] toPara();
    }
}
