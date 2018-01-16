using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Devices.CardReader;

namespace YuanTu.NanYangFirstPeopleHospital.Component.Auth.ViewModels
{
    public class InCardViewModel : CardViewModel
    {
        public InCardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders) : base(rfCardReaders, magCardReaders)
        {
        }

        protected override void PasswordConfirm()
        {
            if (Password.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "温馨提示", "密码不能为空");
                return;
            }
            ShowPassword = false;
            DoCommand(ctx =>
            {
                CardModel.CardType = Consts.Enums.CardType.就诊卡;
                var req = new req住院患者信息查询
                {
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardNo,
                    extend = Password
                };
                var res = DataHandlerEx.住院患者信息查询(req);
                if (res?.success ?? false)
                {
                    if (res?.data == null)
                    {
                        ShowAlert(false, "住院患者信息查询", "查询患者信息失败");
                        StartRead();
                        return;
                    }
                    ChangeNavigationContent($"{CardNo}");
                    PatientModel.Res住院患者信息查询 = res;
                    Next();
                }
                else
                {
                    ShowAlert(false, "住院患者信息查询", "查询患者信息失败" + res?.msg);
                    StartRead();
                }
            });
        }
    }
}