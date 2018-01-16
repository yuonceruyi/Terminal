using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Consts.Services
{
    public interface IAutoUpdateHost : IService
    {
        void Start();
        void WorkerThread();
        void Producer(string cmdText);
    }
}
