using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;
using YuanTu.Devices.CardReader;

namespace YuanTu.NingXiaHospital.Component.Auth.ViewModels
{
    public class InSiCardViewModel : SiCardViewModel
    {
        public InSiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {
        }

        protected override void OnGetInfo()
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
        }
    }
}
