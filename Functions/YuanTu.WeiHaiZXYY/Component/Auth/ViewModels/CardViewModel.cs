using System.Threading;
using System.Threading.Tasks;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;

namespace YuanTu.WeiHaiZXYY.Component.Auth.ViewModels
{
    public class CardViewModel : Default.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders) : base(rfCardReaders, magCardReaders)
        {
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
                    if (!_magCardReader.Initialize().IsSuccess)
                    {
                        ShowAlert(false, "友好提示", $"读卡器初始化失败({ret.ResultCode})", debugInfo: ret.Message);
                        return;
                    }
                    _working = true;
                    while (_working)
                    {
                        var pos = _magCardReader.GetCardPosition();
                        if (pos.IsSuccess && pos.Value == CardPos.停卡位)
                        {
                            var rest = _magCardReader.ReadTrackInfos(TrackRoad.Trace2, ReadType.ASCII);
                            if (rest.IsSuccess)
                            {
                                track = rest.Value[TrackRoad.Trace2].Substring(0, 8);
                                Logger.Main.Info($"[读取卡号成功][cardNo]{track}");
                            }
                            break;
                        }
                        Thread.Sleep(300);
                    }
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

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试卡号");
            if (ret.IsSuccess)
                OnGetInfo(ret.Value);
        }
    }
}