using System;
using System.Linq;
using System.Threading;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Devices.CardReader;

namespace YuanTu.Default.Clinic.Component.Auth.ViewModels
{
    public class CardViewModel : Default.Component.Auth.ViewModels.CardViewModel
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

        //    {
        //    if (cardNo.IsNullOrWhiteSpace())
        //{

        //protected override void OnGetInfo(string cardNo, string extendInfo = null)
        //        ShowAlert(false, "病人信息查询", "卡片信息为空，请确认插卡方向是否正确和卡片是否有效", extend:
        //            new Consts.Models.AlertExModel()
        //            {
        //                HideCallback = p =>
        //                {
        //                    if (p == Consts.Models.AlertHideType.ButtonClick)
        //                    {
        //                        StartRead();
        //                    }
        //                }
        //            }
        //            );
        //        Preview();
        //        return;
        //    }
        //    DoCommand(ctx =>
        //    {
        //        PatientModel.Req病人信息查询 = new req病人信息查询
        //        {
        //            Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
        //            cardNo = cardNo,
        //            cardType = ((int)CardModel.CardType).ToString()
        //        };
        //        PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

        //        if (PatientModel.Res病人信息查询.success)
        //        {
        //            if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
        //            {
        //                ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)", extend:
        //            new Consts.Models.AlertExModel()
        //            {
        //                HideCallback = p =>
        //                {
        //                    if (p == Consts.Models.AlertHideType.ButtonClick)
        //                    {
        //                        StartRead();
        //                    }
        //                }
        //            });
        //                Preview();
        //                return;
        //            }
        //            CardModel.CardNo = cardNo;
        //            Next();
        //        }
        //        else
        //        {
        //            ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg, extend:
        //            new Consts.Models.AlertExModel()
        //            {
        //                HideCallback = p =>
        //                {
        //                    if (p == Consts.Models.AlertHideType.ButtonClick)
        //                    {
        //                        StartRead();
        //                    }
        //                }
        //            });
        //            Preview();
        //        }
        //    });
        //}
    }
}