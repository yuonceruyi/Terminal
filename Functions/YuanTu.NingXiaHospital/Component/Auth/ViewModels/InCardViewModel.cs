using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;
using YuanTu.Devices.CardReader;

namespace YuanTu.NingXiaHospital.Component.Auth.ViewModels
{
    public class InCardViewModel : CardViewModel
    {
        public InCardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders,
            IIcCardReader[] icCardReaders) : base(rfCardReaders, magCardReaders, icCardReaders)
        {
        }

        protected override void OnGetInfo(string cardNo, string extendInfo = null)
        {
            if (cardNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "病人信息查询", "卡片信息为空，请确认插卡方向是否正确和卡片是否有效", extend: new AlertExModel
                {
                    HideCallback =
                        p =>
                        {
                            if (p == AlertHideType.ButtonClick)
                                StartRead();
                        }
                });
                return;
            }
            if (CardModel.CardType == CardType.银行卡)
                CardModel.CardNo = cardNo.Split('=')[0];

            DoCommand(ctx =>
            {
                PatientModel.Req住院患者信息查询 = new req住院患者信息查询
                {
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo
                };
                var res = DataHandlerEx.住院患者信息查询(PatientModel.Req住院患者信息查询);
                if (res?.success ?? false)
                {
                    if (res?.data == null)
                    {
                        ShowAlert(false, "住院患者信息查询", "查询患者信息失败");
                        return;
                    }
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
