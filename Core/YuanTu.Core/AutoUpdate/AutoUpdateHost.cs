using Microsoft.Practices.ServiceLocation;
using System;
using System.IO.Pipes;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Core.Navigating;

namespace YuanTu.Core.AutoUpdate
{
    public class AutoUpdateHost: IAutoUpdateHost
    {
        private static readonly NamedPipeServerStream PipeServer;

        static AutoUpdateHost()
        {
            var count = CommandLineSwitches.AllowMulitipleInstances ? 2 : 1;
            PipeServer = new NamedPipeServerStream("AutoUpdatePipe", PipeDirection.InOut, count);
        }

        public string ServiceName => "自动更新通信";

        private readonly NavigationEngine _engine;
        public AutoUpdateHost()
        {
            _engine = ServiceLocator.Current.GetInstance<NavigationEngine>();
        }

        public void Start()
        {
            Task.Factory.StartNew(WorkerThread);
        }
        public virtual void WorkerThread()
        {
            try
            {
                Log.Logger.Update.Info("自动更新命名管道通信:连接成功");
                while (true)
                {
                    PipeServer.WaitForConnection(); 
                    var ss = new StreamString(PipeServer);
                    ss.WriteString("I am the one true server!");
                    var cmdText = ss.ReadString();
                    if (cmdText == "Are you Free?")
                    {
                        var result = _engine.IsHome(_engine.State);
                        ss.WriteString(result.ToString());
                    }
                    else
                    {
                        Producer(cmdText);
                    }
                    PipeServer.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Update.Error($"自动更新命名管道通信异常：{ex.Message}");
            }
        }

        public virtual void Producer(string cmdText)
        {
            Log.Logger.Update.Error($"自动更新命名管道通信：收到其他信息-{cmdText}");
        }
    }

}
