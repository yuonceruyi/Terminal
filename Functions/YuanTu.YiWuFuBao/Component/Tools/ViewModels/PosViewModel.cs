using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.Gateway;
using YuanTu.Core.Log;
using YuanTu.Core.Systems.Ini;
using YuanTu.Devices.CardReader;
using YuanTu.Devices.UnionPay;
using YuanTu.YiWuFuBao.Device;

namespace YuanTu.YiWuFuBao.Component.Tools.ViewModels
{
    public class PosViewModel:YuanTu.Default.Component.Tools.ViewModels.PosViewModel
    {
        public override bool OnLeaving(NavigationContext navigationContext)
        {
            if (MisposUnionService.IsBusy)
            {
                ShowAlert(false,"禁止退出","系统设备忙碌中，请等待...",10);
                return false;
            }
          
            return true;
        }

        public override Task<bool> OnLeavingAsync(NavigationContext navigationContext)
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在关闭银联通讯，请稍后...");
                _mustClose = true;
                CloseDevices("取消操作");
                Thread.Sleep(300);
                var reader = GetInstance<IMagCardReader[]>().FirstOrDefault(p => p.DeviceId == "ACT_A6_Mag&IC");
                if (reader != null)
                {
                    int retry = 3;
                    while (retry-->0)
                    {
                        if (reader.Connect().IsSuccess)
                        {
                            reader.UnInitialize();
                            reader.DisConnect();
                            break;
                        }
                        Thread.Sleep(4*1000);
                    }
                    
                }
                return true;
            },false);
        }


        protected override Queue<IPrintable> RefundFailPrintables(string refundReason, string refundFailReason)
        {
            var queue = PrintManager.NewQueue($"银联{ExtraPaymentModel.CurrentBusiness}单边账");
            var dto = ExtraPaymentModel.PaymentResult as TransResDto;
            var sb = new StringBuilder();
            sb.Append($"状态：银联冲正失败\n");
            sb.Append($"冲正原因：{refundReason}\n");
            sb.Append($"冲正失败原因：{refundFailReason}\n");
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo.Name}\n");

            sb.Append(ExtraPaymentModel.CurrentBusiness == Consts.Enums.Business.住院押金
                ? $"住院号：{ExtraPaymentModel.PatientInfo.PatientId}\n"
                : $"门诊号：{ExtraPaymentModel.PatientInfo.PatientId}\n");
            sb.Append($"交易类型：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"交易金额：{ExtraPaymentModel.TotalMoney.In元()}\n");
            sb.Append($"商户号：{dto?.MId}\n");
            sb.Append($"终端号：{dto?.TId}\n");
            sb.Append($"授权号：{dto?.Auth}\n");
            sb.Append($"批次号：{dto?.Batch}\n");
            sb.Append($"流水号：{dto?.Ref}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }


    }
}
