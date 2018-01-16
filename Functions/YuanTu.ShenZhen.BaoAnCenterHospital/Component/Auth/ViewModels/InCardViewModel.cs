using YuanTu.Consts.Gateway;
using YuanTu.Devices.CardReader;

namespace YuanTu.ShenZhen.BaoAnCenterHospital.Component.Auth.ViewModels
{
    public class InCardViewModel : Auth.ViewModels.CardViewModel
    {
        public InCardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
        {
        }

        protected override void OnGetInfo(string cardNo, string extendInfo = null)
        {
            CardModel.CardNo = cardNo;
            DoCommand(ctx =>
             {
                 var req = new req住院患者信息查询
                 {
                     cardType = ((int)CardModel.CardType).ToString(),
                     cardNo = cardNo,
                    //patientId = cardNo,
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
                     ChangeNavigationContent($"{cardNo}");
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