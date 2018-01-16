using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Devices.CardReader;

namespace YuanTu.WeiHaiZXYY.Component.Auth.ViewModels
{
    public class InPatientNoCardViewModel : Default.Component.Auth.ViewModels.CardViewModel
    {
        public InPatientNoCardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
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

        protected override void OnGetInfo(string cardNo, string extendInfo = null)
        {
            if (cardNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "病人信息查询", "卡片信息为空，请确认插卡方向是否正确和卡片是否有效");
                StartRead();
                return;
            }
            DoCommand(lp =>
            {
                var req = new req住院患者信息查询
                {
                    cardNo = cardNo
                };
                var res = DataHandlerEx.住院患者信息查询(req);
                if (res?.success ?? false)
                {
                    if (res?.data == null)
                    {
                        ShowAlert(false, "住院患者信息查询", "查询患者信息失败");
                        return;
                    }
                    ChangeNavigationContent($"{cardNo}");
                    PatientModel.Res住院患者信息查询 = res;
                    Next();
                }
                else
                {
                    ShowAlert(false, "住院患者信息查询", "查询患者信息失败" + res?.msg);
                }
            });
        }
    }
}