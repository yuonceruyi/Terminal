using System;
using System.Linq;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.FrameworkBase.Loadings;

namespace YuanTu.BJJingDuETYY.Component.Auth.ViewModels
{
    public class FaceRecViewModel : FaceRecViewModelBase
    {
        public override string Title => "刷脸登录";

        protected override void Act(LoadingProcesser lp, string imageData)
        {
            var req = new req病人信息查询
            {
                Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                cardNo = imageData,
                cardType = ((int)CardType.刷脸).ToString()
            };
            PatientModel.Req病人信息查询 = req;
            var res = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
            PatientModel.Res病人信息查询 = res;

            if (!res.success)
            {
                ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: res.msg);
                return;
            }
            if (res.data == null || res.data.Count == 0)
            {
                ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                return;
            }

            var info = res.data.First();
            CardModel.CardType = (CardType)int.Parse(info.cardType);
            CardModel.CardNo = info.seqNo;
            Next();
        }

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }
    }
}