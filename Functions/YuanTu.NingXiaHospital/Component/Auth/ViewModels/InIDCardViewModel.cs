using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;
using YuanTu.Devices.CardReader;

namespace YuanTu.NingXiaHospital.Component.Auth.ViewModels
{
    public class InIDCardViewModel:IDCardViewModel
    {
        public InIDCardViewModel(IIdCardReader[] idCardReaders) : base(idCardReaders)
        {
        }

        protected override void OnGetInfo(string idCardNo)
        {
            DoCommand(ctx =>
            {
                PatientModel.Req住院患者信息查询 = new req住院患者信息查询
                {
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = idCardNo
                };
                var res = DataHandlerEx.住院患者信息查询(PatientModel.Req住院患者信息查询);
                if (res?.success ?? false)
                {
                    if (res?.data == null)
                    {
                        ShowAlert(false, "住院患者信息查询", "查询患者信息失败");
                        return;
                    }
                    ChangeNavigationContent($"{idCardNo}");
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
