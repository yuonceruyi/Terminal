using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;

namespace YuanTu.Consts.EventModels
{
    public class TimeOutChangeEvent : PubSubEvent<TimeOutChangeEvent>
    {
        public int TimeOut;
    }
}
