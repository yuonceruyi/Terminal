using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.ShengZhouHospital.HisNative.Models
{
    public class dyd
    {
        public List<info> infos { get; set; }
    }

    public class info
    {
        public string zxks { get; set; }
        public string xmmc { get; set; }
        public string sl { get; set; }
        public string dj { get; set; }
    }
}
