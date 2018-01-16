using System.Linq;
using System.Text;
using System.Threading;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Devices.CardReader;

namespace YuanTu.FuYangRMYY.Component.Auth.ViewModels
{
    public class CardViewModel : Default.Clinic.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
        {
            _rfCardReader = rfCardReaders?.FirstOrDefault(p => p.DeviceId == "DK_dk");
        }

        protected override void StartRF()
        {
            string track = null;
            try
            {
                var ret = _rfCardReader.Connect();
                if (!ret.IsSuccess)
                {
                    ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                    ShowAlert(false, "友好提示", $"读卡器打开失败({ret.ResultCode})", debugInfo: ret.Message);
                    return;
                }
                if (!_rfCardReader.Initialize().IsSuccess)
                {
                    ShowAlert(false, "友好提示", $"读卡器初始化失败({ret.ResultCode})", debugInfo: ret.Message);
                    return;
                }
                _working = true;
                while (_working)
                {
                    Thread.Sleep(300);
                    var rest = _rfCardReader.GetCardId();
                    if (!rest.IsSuccess) continue;
                    track = Encoding.Default.GetString(rest.Value).Substring(0,12);
                    Logger.Main.Info($"[读取卡号成功][cardNo]{track}");
                    break;
                }
            }
            finally
            {
                _rfCardReader.UnInitialize();
                if (_working)
                    OnGetInfo(track);
            }
        }

        

    }
}