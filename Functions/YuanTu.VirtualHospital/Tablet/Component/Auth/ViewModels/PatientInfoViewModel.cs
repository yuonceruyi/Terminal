using System.Collections.Generic;
using Prism.Regions;
using YuanTu.Consts.Gateway;
using YuanTu.Devices.CardReader;

namespace YuanTu.VirtualHospital.Tablet.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : Default.Tablet.Component.Auth.ViewModels.PatientInfoViewModel
    {
        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser) : base(rfCardDispenser)
        {
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);

            // TODO 模拟数据
            PatientModel.Res病人信息查询 = new res病人信息查询()
            {
                data = new List<病人信息>()
                {
                    new 病人信息()
                    {
                        
                    }
                }
            };
        }
    }
}