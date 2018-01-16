using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Devices.CardReader;

namespace YuanTu.NingHaiXiangDYYY.Component.Auth.ViewModels
{
    public class CardViewModel : Default.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
        {
            if (FrameworkConst.Strategies[0] == DeviceType.Clinic)
                _magCardReader = magCardReaders.FirstOrDefault(p => p.DeviceId == "HuaDa_Mag");
        }
        public override string Title => "刷就诊卡";

        public override void OnSet()
        {
            base.OnSet();
            TipContent = "请在磁卡感应区刷卡";
            Hint = "请按提示刷卡";
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
            if (FrameworkConst.Strategies[0] != DeviceType.Clinic)
            {
                base.StartMag();
            }
            else
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
                        var rest = _magCardReader.ReadTrackInfos(2);
                        if (!rest.IsSuccess)
                            continue;
                        if (rest.Value.IsNullOrWhiteSpace())
                        {
                            ShowAlert(false, "友好提示", $"读卡失败，请刷正确的就诊卡");
                            continue;
                        }
                        track = rest.Value;
                        track = Regex.Match(track, @"(\.*\d+)(\w)").Groups[0].Value;
                        Logger.Main.Info($"[读取卡号成功][cardNo]{track}");
                        break;
                    }
                }
                finally
                {
                    _magCardReader.UnInitialize();
                    if (_working)
                        OnGetInfo(track);
                }
            }
        }
    }
}