using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;

namespace YuanTu.WeiHaiZXYY.Clinic.Component.Auth.ViewModels
{
    public class CardViewModel : Default.Clinic.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
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

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试卡号");
            if (ret.IsSuccess)
                OnGetInfo(ret.Value);
        }
        //public override bool OnLeaving(NavigationContext navigationContext)
        //{
        //    _working = false;

        //    //StopRead();
        //    return true;
        //}
    }
}
