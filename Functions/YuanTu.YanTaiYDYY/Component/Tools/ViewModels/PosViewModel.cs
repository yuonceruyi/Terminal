using Microsoft.Practices.Unity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Gateway;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Devices.UnionPay;

namespace YuanTu.YanTaiYDYY.Component.Tools.ViewModels
{
    public class PosViewModel: YuanTu.Default.Component.Tools.ViewModels.PosViewModel
    {
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        /// <summary>
        ///     进入当期页面时触发
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            _hasExit = false;
            _mustClose = false;

            var patientInfo = ExtraPaymentModel.PatientInfo;
            Name = patientInfo.Name;
            CardNo = patientInfo.CardNo;
            Remain = patientInfo.Remain.In元();
            //if (ExtraPaymentModel.CurrentBusiness == Consts.Enums.Business.住院押金)
            //{
            //    var patientInfo = PatientModel.住院患者信息;
            //    Name = patientInfo.name;
            //    CardNo = patientInfo.patientHosId;
            //    Remain = decimal.Parse(patientInfo.accBalance).In元();
            //}
            //else
            //{
            //    var patientInfo = PatientModel.当前病人信息;
            //    Name = patientInfo.name;
            //    CardNo = CardModel.CardNo;
            //    Remain = decimal.Parse(patientInfo.accBalance).In元();
            //}
            Business = $"{ExtraPaymentModel.CurrentBusiness}";
            BankPassword = "";
            Amount = ExtraPaymentModel.TotalMoney;
            ShowKeyboardAnimation = false;
            ShowInputPassWord = false;
            if (ChoiceModel.Business == Consts.Enums.Business.充值)
            {
                ShowAlert(true, "友情提示", "拔出诊疗卡，插入银行卡");
            }
            StartPosFlow();
        }

        protected override Queue<IPrintable> BusinessFailPrintables()
        {
            var queue = PrintManager.NewQueue($"银联{ExtraPaymentModel.CurrentBusiness}单边账");
            //var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var sb = new StringBuilder();
            sb.Append($"状态：银联扣费成功，业务处理失败\n");
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo.Name}\n");
            sb.Append(ExtraPaymentModel.CurrentBusiness == Consts.Enums.Business.住院押金
                ? $"住院号：{ExtraPaymentModel.PatientInfo.PatientId}\n"
                : $"门诊号：{ExtraPaymentModel.PatientInfo.PatientId}\n");

            sb.Append($"卡号：{ExtraPaymentModel.PatientInfo.CardNo}\n");

            sb.Append($"交易类型：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"银联流水：{(ExtraPaymentModel.PaymentResult as TransResDto)?.Ref}\n");
            sb.Append($"交易金额：{ExtraPaymentModel.TotalMoney.In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

        protected override Queue<IPrintable> RefundFailPrintables(string refundReason, string refundFailReason)
        {
            var queue = PrintManager.NewQueue($"银联{ExtraPaymentModel.CurrentBusiness}单边账");

            var sb = new StringBuilder();
            sb.Append($"状态：银联冲正失败\n");
            sb.Append($"冲正原因：{refundReason}\n");
            sb.Append($"冲正失败原因：{refundFailReason}\n");
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo.Name}\n");
            sb.Append($"卡号：{ExtraPaymentModel.PatientInfo.CardNo}\n");

            sb.Append(ExtraPaymentModel.CurrentBusiness == Consts.Enums.Business.住院押金
                ? $"住院号：{ExtraPaymentModel.PatientInfo.PatientId}\n"
                : $"门诊号：{ExtraPaymentModel.PatientInfo.PatientId}\n");

            sb.Append($"交易类型：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"银联流水：{(ExtraPaymentModel.PaymentResult as TransResDto)?.Ref}\n");
            sb.Append($"交易金额：{ExtraPaymentModel.TotalMoney.In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

        protected override Queue<IPrintable> GatewayUnknowErrorPrintables(string errorMsg)
        {
            var queue = PrintManager.NewQueue($"银联{ExtraPaymentModel.CurrentBusiness}单边账");
            var sb = new StringBuilder();
            sb.Append($"状态：{errorMsg}\n");
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo.Name}\n");
            sb.Append(ExtraPaymentModel.CurrentBusiness == Consts.Enums.Business.住院押金
                ? $"住院号：{ExtraPaymentModel.PatientInfo.PatientId}\n"
                : $"门诊号：{ExtraPaymentModel.PatientInfo.PatientId}\n");

            sb.Append($"卡号：{ExtraPaymentModel.PatientInfo.CardNo}\n");
            sb.Append($"交易类型：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"交易金额：{ExtraPaymentModel.TotalMoney.In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"银联流水：{(ExtraPaymentModel.PaymentResult as TransResDto)?.Ref}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}
