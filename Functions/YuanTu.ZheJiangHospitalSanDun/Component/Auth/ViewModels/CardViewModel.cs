using System;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Devices.CardReader;

namespace YuanTu.ZheJiangHospitalSanDun.Component.Auth.ViewModels
{
    class CardViewModel : Default.Component.Auth.ViewModels.CardViewModel
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

        protected override void OnGetInfo(string cardNo, string extendInfo = null)
        {
            Logger.Main.Info("开始获取信息");
            if (cardNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "病人信息查询", "卡片信息为空，请确认插卡方向是否正确和卡片是否有效",
                    extend: new AlertExModel { HideCallback = HideCallback });
                return;
            }
            if (!cardNo.Contains("="))
            {
                ShowAlert(false, "病人信息查询", "卡片信息有误，请确认是否插入院内就诊卡",
                    extend: new AlertExModel { HideCallback = HideCallback });
                return;
            }
            DoCommand(_ =>
            {
                var trackData = cardNo;
                cardNo = cardNo.Substring(0, cardNo.IndexOf('='));
                var req = new req病人信息查询
                {
                    Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                    cardNo = cardNo,
                    cardType = ((int)CardModel.CardType).ToString(),
                    extend = trackData
                };
                PatientModel.Req病人信息查询 = req;
                var res = DataHandlerEx.病人信息查询(req);
                PatientModel.Res病人信息查询 = res;

                if (!res.success)
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: res.msg,
                        extend: new AlertExModel { HideCallback = HideCallback });
                    return;
                }
                if (res.data == null || res.data.Count == 0)
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)",
                        extend: new AlertExModel { HideCallback = HideCallback });
                    return;
                }
                CardModel.CardNo = cardNo;
                Next();
            });
        }
    }
}
