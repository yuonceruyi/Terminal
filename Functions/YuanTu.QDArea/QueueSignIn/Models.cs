using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.QDArea.QueueSignIn
{
    public interface IQueues : IModel
    {
        List<ResQueryQueueByDevice.Data> list { get; set; }
    }
    public class Queues : ModelBase,IQueues
    {
        public List<ResQueryQueueByDevice.Data> list
        {
            get;
            set;
        }
    }
}
