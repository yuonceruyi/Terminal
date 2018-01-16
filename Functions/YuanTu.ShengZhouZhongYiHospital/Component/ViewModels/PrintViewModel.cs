using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Devices.CardReader;

namespace YuanTu.ShengZhouZhongYiHospital.Component.ViewModels
{
    public class PrintViewModel : YuanTu.Default.Component.ViewModels.PrintViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            TimeOut = 10;
            base.OnEntered(navigationContext);
            ConfirmContent = CurrentStrategyType() == DeviceType.Clinic ? "确定" : "退卡";
            //ExitCard();
        }

        protected override void Confirm()
        {
            if (_doCommand)
                return;
            _doCommand = !_doCommand;
            if (CurrentStrategyType() != DeviceType.Clinic)
            {
                ExitCard();
            }
            Next();
        }

        private string _confirmContent;

        public string ConfirmContent
        {
            get => _confirmContent;
            set
            {
                _confirmContent = value;
                OnPropertyChanged();
            }
        }

        protected virtual void ExitCard()
        {
            var vm = GetInstance<IRFCpuCardReader[]>().FirstOrDefault(p => p.DeviceId == "CRT310_IC");
            if (vm != null)
            {
                if (vm.Connect().IsSuccess)
                {
                    var ret = vm.GetCardPosition();
                    var ignore = new[] { CardPos.无卡, CardPos.持卡位, CardPos.不持卡位, };
                    if (ret.IsSuccess && !ignore.Contains(ret.Value))
                    {
                        vm.MoveCard(CardPos.不持卡位);
                    }
                    vm.UnInitialize();
                    vm.DisConnect();
                }
            }
            ConstInner.ClearCacheData();
        }
    }
}
