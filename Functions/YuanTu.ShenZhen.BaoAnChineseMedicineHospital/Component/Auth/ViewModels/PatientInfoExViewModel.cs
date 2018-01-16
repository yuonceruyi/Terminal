using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Component.Auth.Dialog.Views;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;

namespace YuanTu.ShenZhen.BaoAnChineseMedicineHospital.Component.Auth.ViewModels
{
    public class PatientInfoExViewModel : YuanTu.Default.Component.Auth.ViewModels.PatientInfoExViewModel
    {
        private static readonly byte[] _keyA = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

        public PatientInfoExViewModel(IRFCardDispenser[] rfCardDispenser) : base(rfCardDispenser)
        {
            _rfCardDispenser = rfCardDispenser?.FirstOrDefault(p => p.DeviceId == "Act_F3_RF");
            ConfirmCommand = new DelegateCommand(Confirm);
            ModifyNameCommand = new DelegateCommand(ModifyNameCmd);
        }
        protected override bool GetNewCardNo()
        {
            try
            {
                if (FrameworkConst.DoubleClick && FrameworkConst.VirtualHardWare)
                    return Invoke(() =>
                    {
                        var ret = InputTextView.ShowDialogView("输入物理卡号");
                        if (ret.IsSuccess)
                        {
                            CardModel.CardNo = ret.Value;
                            return true;
                        }
                        return false;
                    });
                Logger.Main.Info($"[建档发卡]进入到获取卡号逻辑");
                if (!_rfCardDispenser.Connect().IsSuccess)
                {
                    ReportService.发卡器离线(null, ErrorSolution.发卡器离线);
                    ShowAlert(false, "建档发卡", "发卡机连接失败");
                    return false;
                }
                Logger.Main.Info($"[建档发卡]发卡器判断是否含有卡");
                if (!_rfCardDispenser.EnterCard().IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机获取序列号失败");
                    return false;
                }
                if (!_rfCardDispenser.MfVerifyPassword(0, true, _keyA).IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡器新卡密码验证失败");
                    return false;
                }
                byte[] data1;//序列号
                if (!_rfCardDispenser.MfReadSector(0, 0, out data1).IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡器新卡 序列号读取失败");
                    return false;
                }
                if (!_rfCardDispenser.MfVerifyPassword(2, true, _keyA).IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡器新卡 密码验证失败");
                    return false;
                }
                byte[] data2;//卡号
                if (!_rfCardDispenser.MfReadSector(2, 2, out data2).IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡器新卡 卡号读取失败");
                    return false;
                }

                byte[] bCardNo = new byte[16];
                byte[] bSerialNo = new byte[4];
                Array.Copy(data2, bCardNo, 16);
                Array.Copy(data1, bSerialNo, 4);
                Array.Reverse(bSerialNo);
                var serialNo = bSerialNo.ByteToString();
                var cardNo = bCardNo.ByteToString();

                if (string.IsNullOrEmpty(cardNo) || string.IsNullOrEmpty(serialNo))
                {
                    ShowAlert(false, "建档发卡", "获取数据失败失败");
                    return false;
                }
                if (cardNo.Contains("\0"))
                {
                    cardNo = cardNo.Substring(0, cardNo.IndexOf("\0"));
                }
                Logger.Main.Info($"[建档发卡]发卡器获取序列号完毕Default:{serialNo}");
                Logger.Main.Info($"[建档发卡]发卡器获取卡号完毕Default:{ cardNo}");
                if (cardNo.Length < 12)
                {
                    Logger.Main.Info($"[建档发卡]发卡器获取卡号完毕,卡号少于12位，读取错误Default:{ cardNo}");
                }
                else
                {
                    cardNo = cardNo.Substring(0, 12);
                }
                CardModel.CardNo = cardNo;
                CardModel.ExternalCardInfo = serialNo;
                return true;
            }
            catch (Exception ex)
            {
                ShowAlert(false, "建档发卡", "发卡机读卡失败");
                Logger.Main.Error($"[建档发卡]{ex.Message + ex.StackTrace}");
                return false;
            }
        }

        protected override void PrintCard()
        {
            if (CardModel.CardType == CardType.身份证)
                _rfCardDispenser.MoveCardOut();
        }
    }
}