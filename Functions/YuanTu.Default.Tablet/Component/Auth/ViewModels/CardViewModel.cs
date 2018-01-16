using System;
using System.Linq;
using System.Threading;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Models.UserCenter.Auth;
using YuanTu.Consts.Services;
using YuanTu.Consts.UserCenter;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Devices.CardReader;

namespace YuanTu.Default.Tablet.Component.Auth.ViewModels
{
    public class CardViewModel : Default.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
        {
            _rfCardReader = rfCardReaders?.FirstOrDefault(p => p.DeviceId == "HuaDa_RF");
        }

        [Dependency]
        public IAuthModel AuthModel { get; set; }

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("读就诊卡");
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

        protected override void OnGetInfo(string cardNo, string extendInfo = null)
        {
            if (cardNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "病人信息查询", "卡片信息为空，请确认插卡方向是否正确和卡片是否有效");
                Preview();
                return;
            }
            DoCommand(ctx =>
            {
                ctx.ChangeText("正在查询就诊人信息,请稍候...");
                var req = new req查询就诊人
                {
                    cardNo = cardNo,
                    //cardType = ((int) AuthModel.CardType).ToString(),
                    cardType = "2",
                    unionId = FrameworkConst.UnionId
                };
                var res = DataHandlerEx.查询就诊人(req);

                if (res.success)
                {
                    if (res.data == null)
                    {
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息(数据为空)");
                        Preview();
                        return;
                    }
                    AuthModel.当前就诊人信息 = res.data;
                    AuthModel.CardNo = cardNo;
                    Next();
                }
                else
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: res.msg);
                    Preview();
                    
                }
            });
        }
    }
}