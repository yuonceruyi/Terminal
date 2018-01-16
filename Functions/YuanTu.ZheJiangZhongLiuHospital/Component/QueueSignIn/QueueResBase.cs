using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.ZheJiangZhongLiuHospital.Component.QueueSignIn
{
    public class QueueResBase<T>
    {
        public bool success { get; set; }
        public string resultCode { get; set; }
        public string msg { get; set; }
        public T data { get; set; }
        public long startTime { get; set; }
        public int timeConsum { get; set; }
    }
}
