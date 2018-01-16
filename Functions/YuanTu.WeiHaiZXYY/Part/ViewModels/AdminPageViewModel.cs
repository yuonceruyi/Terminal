using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Gateway;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Part.ViewModels.AdminSub;
using SerialPrinter = YuanTu.Devices.Printer;
using YuanTu.Devices.CardReader;
using YuanTu.Default.Part.ViewModels;
using YuanTu.Consts.Enums;
using System.ComponentModel;

namespace YuanTu.WeiHaiZXYY.Part.ViewModels
{
    public class AdminPageViewModel : YuanTu.Default.Part.ViewModels.AdminPageViewModel
    {

        public AdminPageViewModel():base()
        {
            //CardReaderMoveOutCardCommand = new DelegateCommand(CardReaderMoveOutCard);
            //CardSenderMoveOutCardCommand = new DelegateCommand(CardSenderMoveOutCard);
        }

        private void CardReaderMoveOutCard()
        {
            var config = GetInstance<IConfigurationManager>();
            var magCardReader = new A6MagCardReader(config);
            try
            {
                if (!magCardReader.Connect().IsSuccess || !magCardReader.Initialize().IsSuccess 
                    || !magCardReader.UnInitialize().IsSuccess|| !magCardReader.DisConnect().IsSuccess)
                {
                    ShowAlert(false, "温馨提示", $"读卡器退卡失败");
                }
            }
            catch (Exception ex)
            {
                ShowAlert(false, "温馨提示", $"读卡器退卡失败，异常错误：{ex.Message}");
            }

        }

        private void CardSenderMoveOutCard()
        {
            var config = GetInstance<IConfigurationManager>();
            var magCardDispenser = new MagCardDispenser(config);
            var result = magCardDispenser.MoveCardOut();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "温馨提示", $"发卡器退卡失败，异常错误：{result.Message}");
            }
        }

        //public DelegateCommand CardReaderMoveOutCardCommand { get; set; }
        //public DelegateCommand CardSenderMoveOutCardCommand { get; set; }

        protected override bool OnValidatePassword(string pwd)
        {
            AuthTypes.Clear();
            if (pwd == DateTimeCore.Now.ToString("yyMMdd"))
            {
                AuthTypes.Add(AdminType.清钱箱);
                AuthTypes.Add(AdminType.退出系统);
                AuthTypes.Add(AdminType.进入维护);
                AuthTypes.Add(AdminType.发卡器退卡);
                AuthTypes.Add(AdminType.读卡器退卡);
                Data = AuthTypes.Select(p => new AdminButtonInfo
                {
                    Name = p.GetEnumDescription(),
                    AdminType = p,
                    Order = (int)p,
                    IsEnabled = true,
                }).OrderBy(p => p.Order).ToList();

                return true;
            }
            return false;
        }

        protected override void Confirm(AdminButtonInfo param)
        {
            switch (param.AdminType)
            {
                case AdminType.清钱箱:
                    DoClearCashBox();
                    break;

                case AdminType.自动更新:
                    OnAutoUpdate();
                    break;

                case AdminType.设置卡数量:
                    break;

                case AdminType.进入维护:
                    OnMaintenance();
                    break;

                case AdminType.退出系统:
                    OnExitCommand();
                    break;
                case AdminType.发卡器退卡:
                    CardSenderMoveOutCard();
                    break;
                case AdminType.读卡器退卡:
                    CardReaderMoveOutCard();
                    break;

                default:
                    ShowAlert(false, "温馨提示", "功能未开通！");
                    break;
            }
        }


    }
}
