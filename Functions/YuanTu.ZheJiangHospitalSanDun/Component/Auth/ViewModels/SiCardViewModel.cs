using YuanTu.Devices.CardReader;

namespace YuanTu.ZheJiangHospitalSanDun.Component.Auth.ViewModels
{
    internal class SiCardViewModel : Default.Component.Auth.ViewModels.SiCardViewModel
    {
        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders,
            rfCpuCardReaders)
        {
        }

        public override void Confirm()
        {
            ShowAlert(false, "医保读卡", "暂不支持读医保卡");
        }
    }
}