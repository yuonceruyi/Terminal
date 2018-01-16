using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Services;
using YuanTu.Core.Systems;
using YuanTu.Devices.CardReader;
using YuanTu.XiaoShanZYY.Component.Auth.Models;

namespace YuanTu.XiaoShanZYY.Component.Auth.ViewModels
{
    public class SiCardViewModel : Default.Component.Auth.ViewModels.SiCardViewModel
    {
        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders)
            : base(icCardReaders, rfCpuCardReaders)
        {
        }

        [Dependency]
        public IAuthModel Auth { get; set; }

        [Dependency]
        public IAuthService AuthService { get; set; }

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("插卡口");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            CardUri = ResourceEngine.GetImageResourceUri(CardModel.CardType == CardType.社保卡 ? "卡_社保卡" : "卡_健康卡");
        }

        private bool _reading;

        private void CheckWindow()
        {
            while (_reading)
            {
                Thread.Sleep(200);
                var windowPtr = WindowHelper.FindWindow(null, "读市民卡错误提示:");
                if (windowPtr == IntPtr.Zero)
                    continue;
                WindowHelper.SendMessage(windowPtr, (uint)WindowHelper.WindowMessage.CLOSE, 0, 0);
                break;
            }
        }

        public override void Confirm()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在读卡，请稍候...");
                _reading = true;
                Task.Run(() => CheckWindow());
                try
                {
                    var result = AuthService.读市民卡健康卡();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "读市民卡健康卡", $"读市民卡健康卡失败:\n{result.Message}");
                        return;
                    }
                    var result2 = AuthService.账户查询();
                    if (!result2.IsSuccess)
                    {
                        ShowAlert(false, "账户查询", $"账户查询失败:\n{result2.Message}");
                        return;
                    }
                    var result3 = AuthService.人员信息查询();
                    if (!result3.IsSuccess)
                    {
                        ShowAlert(false, "人员信息查询", $"人员信息查询失败:\n{result3.Message}");
                        return;
                    }
                    Next();
                }
                finally
                {
                    _reading = false;
                }
            });
        }
    }
}