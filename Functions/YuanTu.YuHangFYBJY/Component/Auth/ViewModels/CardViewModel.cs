using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Devices.CardReader;

namespace YuanTu.YuHangFYBJY.Component.Auth.ViewModels
{
   public class CardViewModel : Default.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders) : base(rfCardReaders, magCardReaders)
        {
            _magCardReader = magCardReaders.FirstOrDefault(p => p.DeviceId == "HuaDa_Mag");
        }
        protected override void StartRead()
        {
            Task.Run(() => StartMag());
        }

        protected override void StopRead()
        {
            StopMag();
        }
        protected override void StartMag()
        {
            Task.Run(() =>
            {
                string track = null;
                try
                {
                    var ret = _magCardReader.Connect();
                    if (!ret.IsSuccess)
                    {
                        ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                        ShowAlert(false, "友好提示", $"读卡器打开失败({ret.ResultCode})", debugInfo: ret.Message);
                        return;
                    }
                    Logger.Device.Info($"[华大连接成功]");
                    if (!_magCardReader.Initialize().IsSuccess)
                    {
                        ShowAlert(false, "友好提示", $"读卡器初始化失败({ret.ResultCode})", debugInfo: ret.Message);
                        return;
                    }
                    Logger.Device.Info($"[华大序列化成功]");
                    _working = true;
                    while (_working)
                    {
                        Logger.Device.Info($"[华大开始读取卡号]");
                        var rest = _magCardReader.ReadTrackInfos(2);
                        if (!rest.IsSuccess)
                        {
                            Logger.Device.Info($"[华大开始读取卡号失败 false]");
                            continue;
                        }
                        if (rest.Value.IsNullOrWhiteSpace())
                        {
                            Logger.Device.Info($"[华大开始读取卡号失败 空值]");
                            ShowAlert(false, "友好提示", $"读卡失败，请刷正确的就诊卡");
                            continue;
                        }
                        track = rest.Value.Substring(1, rest.Value.IndexOf("?") - 1);
                        Logger.Device.Info($"[读取卡号成功][cardNo]{track}");
                        break;
                    }
                }
                catch (Exception e)
                {
                    Logger.Device.Error($"[华大读卡]{e}");
                }
                finally
                {
                    _magCardReader.UnInitialize();
                    if (_working)
                    {
                        OnGetInfo(track);
                    }
                }
            });
        }
    }
}
