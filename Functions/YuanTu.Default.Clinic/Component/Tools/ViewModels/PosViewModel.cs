using System.Threading;
using YuanTu.Consts;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Log;
using YuanTu.Devices.UnionPay;
using YuanTu.Consts.Enums;

namespace YuanTu.Default.Clinic.Component.Tools.ViewModels
{
    public class PosViewModel : Default.Component.Tools.ViewModels.PosViewModel
    {
        /// <summary>
        /// 本函数重写目的：Tips提示内容 @2017-04-09
        /// </summary>
        /// <returns></returns>
        protected override bool SureGetCard()
        {
            PlaySound(SoundMapping.银行卡支付);
            Tips = "请插入银行卡或插卡或放置于感应区...";
            if (FrameworkConst.VirtualThridPay)
            {
                Thread.Sleep(2000);
                return true;
            }
            if (_mustClose)
            {
                return false;
            }

            var banCardMediaType = BanCardMediaType.磁条 | BanCardMediaType.IC芯片;
            if (CurrentStrategyType() == DeviceType.Clinic ||
                ExtraPaymentModel.CurrentPayMethod == PayMethod.银联闪付 ||
                ExtraPaymentModel.CurrentPayMethod == PayMethod.苹果支付)
            {
                var retReq = MisposUnionService.SetReq(TransType.消费, ExtraPaymentModel.TotalMoney);
                if (!retReq.IsSuccess)
                {
                    ShowAlert(false, "银联读卡异常", retReq.Message);
                    return retReq.IsSuccess;
                }
            }

            var ret = MisposUnionService.ReadCard(banCardMediaType);
            if (!ret.IsSuccess)
            {
                ShowAlert(false, "银联读卡异常", ret.Message);

                Logger.POS.Info($"银联支付，读取银行卡信息，结果:{ret.IsSuccess} 内容:{ret.Message}");
            }
            return ret.IsSuccess;
        }
    }
}