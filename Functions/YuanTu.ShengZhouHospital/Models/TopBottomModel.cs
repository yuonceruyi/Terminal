using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using YuanTu.Consts;
using YuanTu.Consts.EventModels;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Devices.CardReader;
using YuanTu.ShengZhouHospital.Component.Auth.Models;

namespace YuanTu.ShengZhouHospital.Models
{
    public class TopBottomModel:YuanTu.Core.Models.TopBottomModel
    {
        public TopBottomModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
            ShowSystemButtons = true;
            HomeEnable = false;
            BackEnable = false;
            OutCardEnable = DeviceType.NoFallBackStrategy[FrameworkConst.DeviceType][0]  != DeviceType.Clinic;
        }

        private bool _outCardEnable = true;

        public bool OutCardEnable
        {
            get { return _outCardEnable; }
            set
            {
                _outCardEnable = value;
                OnPropertyChanged();
            }
        }

        protected override void ViewHasChanged(ViewChangeEvent eveEvent)
        {
            var ishome = NavigationEngine.IsHome(eveEvent.To);
            if (NavigationEngine.Current.Address == "维护")
            {
                OutCardEnable = false;
            }
            if (ishome)
            {
                InfoItems = null;
            }
            ShowSystemButtons = true;
            SystemButtonCommand.RaiseCanExecuteChanged();
        }

        protected override void SystemInfoChanged(SystemInfoEvent eveEvent)
        {
            BackEnable = !eveEvent.DisablePreviewButton;
            HomeEnable = !eveEvent.DisableHomeButton;
            ShowSystemButtons = true;
            SystemButtonCommand.RaiseCanExecuteChanged();
        }

        protected override void ButtonClick(string cmd)
        {
            var engine = NavigationEngine;
            switch (cmd)
            {
                case "退卡":
                    var vm = GetInstance<IRFCpuCardReader[]>().FirstOrDefault(p => p.DeviceId == "CRT310_IC");
                    if (vm.Connect().IsSuccess)
                    {
                        vm.Initialize();
                        var pos = vm.GetCardPosition();
                        if (pos.IsSuccess && (pos.Value == CardPos.停卡位 || pos.Value == CardPos.IC位))
                        {
                            vm.MoveCard(CardPos.不持卡位);
                        }
                    }
                    if (_isNewMode)
                    {
                        ShowSystemButtons = false;
                        HomeEnable = false;
                        BackEnable = false;
                        _buttonStack.Clear();
                        _resetAction?.Invoke();
                        _eventAggregator.GetEvent<TimeOutChangeEvent>().Publish(new TimeOutChangeEvent());
                    }
                    var pm = GetInstance<IPatientModel>() as PatientInfoModel;
                    pm.Res门诊读卡 = null;
                    pm.Res病人信息查询 = null;
                    ConstInner.ClearCacheData();
                    engine.State = engine.HomeAddress;
                    ConstInner.ClearCacheData();
                    break;
                case "主页":
                    if (_isNewMode)
                    {
                        ShowSystemButtons = false;
                        HomeEnable = false;
                        BackEnable = false;
                        _buttonStack.Clear();
                        _resetAction?.Invoke();
                        _eventAggregator.GetEvent<TimeOutChangeEvent>().Publish(new TimeOutChangeEvent());
                    }
                    engine.State = engine.HomeAddress;
                    break;

                case "返回":
                {

                    var ctx = new FormContext(engine.Context, engine.State);
                    if (engine.HasPrev(ctx) && BackEnable)
                        engine.Prev(ctx);
                    else
                        engine.State = engine.HomeAddress;
                }
                    break;
            }
        }


      

    }
}
