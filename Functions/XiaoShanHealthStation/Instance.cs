using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.XiaoShanArea.CYHIS.WebService;

namespace YuanTu.XiaoShanHealthStation
{
    public class Instance
    {
        public static BASEINFO Baseinfo => new BASEINFO
        {
            CAOZUORQ = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }
}
