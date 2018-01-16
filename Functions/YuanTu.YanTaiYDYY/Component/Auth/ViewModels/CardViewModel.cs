using System;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Devices.CardReader;

namespace YuanTu.YanTaiYDYY.Component.Auth.ViewModels
{
    public class CardViewModel : Default.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
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
                    cardType = ((int) CardModel.CardType).ToString(),
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

                    //临时卡校验 2周内仅提醒不限制
                    if (PatientModel.Res病人信息查询?.data[PatientModel.PatientInfoIndex]?.extend == "1")
                    {
                        ShowAlert(false, "病人信息查询", "您的卡为临时卡，请持身份证将信息补充完整");
                    }

					//临时卡校验 2周外禁止使用
					if (PatientModel.Res病人信息查询?.data[PatientModel.PatientInfoIndex]?.extend == "2")
					{
						ShowAlert(false, "病人信息查询", "您的临时卡办理已超过两周，请持身份证将信息补充完整方可使用");
						StartRead();
						return;
					}

					CardModel.CardNo = cardNo;
                    if (string.IsNullOrWhiteSpace(PatientModel.Res病人信息查询?.data[PatientModel.PatientInfoIndex]?.patientId))
                        //他院区域卡
                        CardModel.ExternalCardInfo = "他院卡";
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