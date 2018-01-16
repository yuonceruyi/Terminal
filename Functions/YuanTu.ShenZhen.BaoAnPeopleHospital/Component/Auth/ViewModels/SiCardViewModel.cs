using Prism.Regions;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Devices.CardReader;
using YuanTu.ShenZhenArea.CardReader;
using YuanTu.ShenZhenArea.Models;
using YuanTu.ShenZhenArea.Services;
using Microsoft.Practices.Unity;
using System.Windows;
using System;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Consts.Models;
using YuanTu.Core.Log;
using YuanTu.Consts.Enums;
using YuanTu.Default.Tools;
using YuanTu.Consts;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.Auth.ViewModels
{
    public class SiCardViewModel : Default.Component.Auth.ViewModels.SiCardViewModel
    {
        private bool _running;

        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {
        }

        [Dependency]
        public IYBService YBServices { get; set; }

        [Dependency]
        public IYBModel YBModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            _running = true;
            ReadHICard();
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            _running = false;
            return base.OnLeaving(navigationContext);
        }

        protected void ReadHICard()
        {
            Task.Run(() =>
            {
                while (_running)
                {
                    string ylzh = null;
                    if (!SB_T6.rdInfo_SSCID(out ylzh)) //读卡失败，则继续
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    _running = false;

                    YBModel.医疗证号 = ylzh;
                    Next();
                }
            });
        }

      
        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试卡号");
            if (ret.IsSuccess)
            {
                YBModel.医疗证号 = ret.Value;
                Next();
            }
        }
    }
}