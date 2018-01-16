using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Devices.CardReader;

namespace YuanTu.YanTaiYDYY.Component.RealAuth.ViewModels
{
    public class CardViewModel:YuanTu.Default.Component.RealAuth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders): base(rfCardReaders, magCardReaders)
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
            if (cardNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "病人信息查询", "卡片信息为空，请确认插卡方向是否正确和卡片是否有效");
                StartRead();
                return;
            }
            DoCommand(ctx =>
            {
                PatientModel.Req病人信息查询 = new req病人信息查询
                {
                    Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                    cardNo = cardNo,
                    cardType = ((int)CardModel.CardType).ToString(),
                    extend = "1"
                };
                PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

                if (PatientModel.Res病人信息查询.success)
                {
                    if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                    {
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                        StartRead();
                        return;
                    }

                    //todo 临时卡校验
                    if (string.IsNullOrWhiteSpace(PatientModel.Res病人信息查询?.data[PatientModel.PatientInfoIndex]?.extend)
                        || PatientModel.Res病人信息查询?.data[PatientModel.PatientInfoIndex]?.extend == "0")
                    {
                        ShowAlert(false, "病人信息查询", "该卡为正式卡，不需要补录");
                        StartRead();
                        return;
                    }
                    CardModel.CardNo = cardNo;
                    Next();
                }
                else
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                    StartRead();
                }
            });
        }
    }
}
