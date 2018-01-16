using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts;

namespace YuanTu.ChongQingArea.Component.Auth.ViewModels
{
    public class CardViewModel : YuanTu.Default.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders):base(rfCardReaders, magCardReaders)
        {
            _rfCardReader = rfCardReaders?.FirstOrDefault(p => p.DeviceId == "Act_A6_RF");
            _magCardReader = magCardReaders?.FirstOrDefault(p => p.DeviceId == "ACT_A6_Mag&IC");
        }

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("提示_诊疗卡");
            CardUri = ResourceEngine.GetImageResourceUri("卡_诊疗卡");
            TipContent = "请插入就诊卡";
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
                    var pos = _rfCardReader.GetCardPosition();
                    if (pos.IsSuccess && pos.Value == CardPos.停卡位)
                    {
                        var rest = _rfCardReader.GetCardId();
                        if (rest.IsSuccess)
                        {
                            track = BitConverter.ToUInt32(rest.Value, 0).ToString();
                            Logger.Main.Info($"[读取卡号成功][cardNo]{track}");
                            //判断卡号是否与区块内卡号相同
                            WriteCardNo(track);
                        }
                        break;
                    }
                    Thread.Sleep(300);
                }
            }
            finally
            {
                _rfCardReader.UnInitialize();
                if (_working)
                    OnGetInfo(track);
            }
        }

        protected Result WriteCardNo(string cardNo)
        {
            if (FrameworkConst.DoubleClick) { return Result.Success(); }
            Logger.Main.Info("准备验证卡号|" + cardNo);

            cardNo = cardNo.PadLeft(10, '0');
            var readRet = _rfCardReader.ReadBlock(0x00, 0x01, true, Startup.SiKey);
            if (readRet && readRet.Value.ByteToString().Substring(0, 10) == cardNo)
            {
                Logger.Main.Info("卡号相同不需写卡|" + readRet.Value.ByteToString());
                return Result.Success();
            }

            var cardNoBts = cardNo.StringToByte();
            var writeRet = _rfCardReader.WirteBlock(0x00, 0x01, true, Startup.SiKey, cardNoBts);
            if (writeRet)
            {
                Logger.Main.Info("写卡成功" );
                return Result.Success();
            }
            Logger.Device.Error($"[发卡写卡]写卡失败,{writeRet.Message}");
            return writeRet;
        }
    }
}

namespace YuanTu.ChongQingArea.Clinic.Component.Auth.ViewModels
{
    public class CardViewModel : Default.Clinic.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
        {
            _rfCardReader = rfCardReaders?.FirstOrDefault(p => p.DeviceId == "HuaDa_RF");
        }

        public override string Title => "刷就诊卡";

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("就诊卡扫描处");
            CardUri = ResourceEngine.GetImageResourceUri("卡_诊疗卡");
            TipContent = "请在磁卡感应区刷卡";
            Hint = "请按提示刷卡";
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            PlaySound(SoundMapping.请刷取您的就诊卡);
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
                    var rest = _rfCardReader.GetCardId();
                    if (rest.IsSuccess)
                    {
                        track = BitConverter.ToUInt32(rest.Value, 0).ToString();
                        Logger.Main.Info($"[读取卡号成功][cardNo]{track}");
                        //判断卡号是否与区块内卡号相同
                        WriteCardNo(track);
                        break;
                    }
                    Thread.Sleep(300);
                }
            }
            finally
            {
                _rfCardReader.UnInitialize();
                if (_working)
                    OnGetInfo(track);
            }
        }

        protected Result WriteCardNo(string cardNo)
        {
            if (FrameworkConst.DoubleClick) { return Result.Success(); }
            Logger.Main.Info("准备验证卡号|" + cardNo);

            cardNo = cardNo.PadLeft(10, '0');
            var readRet = _rfCardReader.ReadBlock(0x00, 0x01, true, Startup.SiKey);
            if (readRet && readRet.Value.ByteToString().Substring(0, 10) == cardNo)
            {
                Logger.Main.Info("卡号相同不需写卡|" + readRet.Value.ByteToString());
                return Result.Success();
            }

            var cardNoBts = cardNo.StringToByte();
            var writeRet = _rfCardReader.WirteBlock(0x00, 0x01, true, Startup.SiKey, cardNoBts);
            if (writeRet)
            {
                Logger.Main.Info("写卡成功");
                return Result.Success();
            }
            Logger.Device.Error($"[发卡写卡]写卡失败,{writeRet.Message}");
            return writeRet;
        }

    }
}