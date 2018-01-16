using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;

namespace YuanTu.NingXiaHospital.HisService
{
   public  class Common
    {
        public static string GetNewPatientId(string klx,string cardno)
        {
            if (FrameworkConst.DoubleClick)
            {
                return "test123";
            }
            var resQueryPatientSignInfo = DllHandler.QueryPatientSignInfo(klx, cardno);
            if (resQueryPatientSignInfo)
            {
                if (resQueryPatientSignInfo.Value.redata != null)
                {
                    return resQueryPatientSignInfo.Value.redata.head.patient_id;
                }
            }
            return "未获取到";
        }
    }
}
