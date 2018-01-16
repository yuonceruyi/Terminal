using System.Linq;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Devices.CardReader;

namespace YuanTu.TaiZhouCentralHospital.Component.ViewModels
{
    public class PrintViewModel : Default.Component.ViewModels.PrintViewModel
    {
        protected IMagCardReader _magCardReader;

        public PrintViewModel(IMagCardReader[] magCardReaders)
        {
            _magCardReader = magCardReaders?.FirstOrDefault(p => p.DeviceId == "ACT_A6_Mag&IC");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            if (FrameworkConst.Strategies[0] != DeviceType.Clinic)
                Task.Factory.StartNew(() =>
                {
                    //检测并退卡
                    var result = _magCardReader.Connect();
                    if (!result.IsSuccess)
                        return;
                    result = _magCardReader.Initialize();
                    if (!result.IsSuccess)
                        return;
                    var pos = _magCardReader.GetCardPosition();
                    if (pos.IsSuccess && (pos.Value != CardPos.无卡 || pos.Value != CardPos.未知))
                        _magCardReader.UnInitialize();
                    _magCardReader.DisConnect();
                });
            base.OnEntered(navigationContext);
        }
    }
}