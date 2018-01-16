using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.Core.Advertisement.Base
{
    public abstract class ResBase
    {
        public string resultCode { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
        public long startTime { get; set; }
        public long timeConsum { get; set; }

       
    }
}
