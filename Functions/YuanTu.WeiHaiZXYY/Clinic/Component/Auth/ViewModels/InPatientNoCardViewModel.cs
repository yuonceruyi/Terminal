using System.Threading.Tasks;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Devices.CardReader;
using System.Linq;

namespace YuanTu.WeiHaiZXYY.Clinic.Component.Auth.ViewModels
{
    public class InPatientNoCardViewModel : Default.Component.Auth.ViewModels.CardViewModel
    {
        public InPatientNoCardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
        {
            _magCardReader = magCardReaders?.FirstOrDefault(p => p.DeviceId == "HuaDa_Mag");
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
                    Logger.Main.Info($"[华大连接成功]");
                    if (!_magCardReader.Initialize().IsSuccess)
                    {
                        ShowAlert(false, "友好提示", $"读卡器初始化失败({ret.ResultCode})", debugInfo: ret.Message);
                        return;
                    }
                    Logger.Main.Info($"[华大序列化成功]");
                    _working = true;
                    while (_working)
                    {
                        Logger.Main.Info($"[华大开始读取卡号]");
                        var rest = _magCardReader.ReadTrackInfos(2);
                        if (!rest.IsSuccess)
                        {
                            Logger.Main.Info($"[华大开始读取卡号失败 false]");
                            continue;
                        }
                        if (rest.Value.IsNullOrWhiteSpace())
                        {
                            Logger.Main.Info($"[华大开始读取卡号失败 空值]");
                            ShowAlert(false, "友好提示", $"读卡失败，请刷正确的就诊卡");
                            continue;
                        }
                        track = rest.Value.Substring(1, 8);
                        Logger.Main.Info($"[读取卡号成功][cardNo]{track}");
                        break;
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