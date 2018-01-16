using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.XiaoShanArea.CYHIS.DLL;
using YuanTu.XiaoShanHealthStation.Component.Auth.Models;
using YuanTu.XiaoShanHealthStation.Component.BillPay.Models;

namespace YuanTu.XiaoShanHealthStation.Component.BillPay.ViewModels
{
    public class BillRecordViewModel : Default.Component.BillPay.ViewModels.BillRecordViewModel
    {
        [Dependency]
        public IBillModel BillModel { get; set; }

        [Dependency]
        public IChaKaModel ChaKaModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var billInfo = BillModel.门诊费用明细Out;
            Collection = new List<PageData>
            {
                new PageData
                {
                    CatalogContent = null,
                    List = billInfo.FEIYONGMX,
                    Tag = billInfo
                }
            };

            BillCount = $"{billInfo.FEIYONGMX.Count}条费用";
            TotalAmount = (billInfo.FEIYONGMX.Sum(p => decimal.Parse(p.JINE)) * 100).In元();
            
            
            PlaySound(SoundMapping.选择待缴费处方);
        }

        protected override void Do()
        {
            ChangeNavigationContent(TotalAmount);

            //todo 缴费预结算
            var result = PreSettle();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "温馨提示", result.Message);
                return;
            }

            var preRes = BillModel.缴费预结算Out;

            PaymentModel.Self = decimal.Parse(preRes.应付金额.SafeToSplit(':')[1]) * 100;
            PaymentModel.Insurance = decimal.Parse(preRes.医保报销金额.SafeToSplit(':')[1]) * 100;
            PaymentModel.Total = decimal.Parse(preRes.单据总金额.SafeToSplit(':')[1]) * 100;
            PaymentModel.NoPay = true;
            PaymentModel.ConfirmAction = Confirm;

            PaymentModel.MidList = new List<PayInfoItem>
            {
                new PayInfoItem("用户姓名：", preRes.病人姓名.SafeToSplit(':')[1]),
                new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
            };
            Next();
        }

        protected virtual Result PreSettle()
        {
            缴费预结算_OUT res;
            var req = new Req缴费预结算
            {
                卡号 = ChaKaModel.查询建档Out?.就诊卡号,
                结算类型 = "03",
                调用接口ID = "",
                调用类型 = "",
                病人类别 = ChaKaModel.PatientType,
                结算方式 = "1", //1 预结算 2 结算  ?
                应付金额 = "",
                就诊序号 = "",
                操作工号 = FrameworkConst.OperatorId,
                系统序号 = "",
                收费类别 = "",
                科室代码 = "",
                医生代码 = "",
                诊疗费加收 = "",
                诊疗费 = "",
                挂号类别 = "",
                排班类别 = "",
                取号密码 = ""
            };
            if (!DataHandler.缴费预结算(req, out res))
                return Result.Fail($"缴费预结算失败");
            if (res?.结算结果 != "00")
                return Result.Fail($"缴费预结算失败");
            BillModel.缴费预结算Out = res;
            return Result.Success();
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在进行缴费，请稍候...");
                缴费结算_OUT res;
                var req = new Req缴费结算
                {
                    卡号 = ChaKaModel.查询建档Out?.就诊卡号,
                    结算类型 = "03",
                    调用接口ID = "",
                    调用类型 = "",
                    病人类别 = ChaKaModel.PatientType,
                    结算方式 = "2", //1 预结算 2 结算  ?
                    应付金额 = BillModel.缴费预结算Out.应付金额,
                    就诊序号 = "",
                    操作工号 = FrameworkConst.OperatorId,
                    系统序号 = "",
                    收费类别 = "",
                    科室代码 = "",
                    医生代码 = "",
                    诊疗费加收 = "",
                    诊疗费 = "",
                    挂号类别 = "",
                    排班类别 = "",
                    取号密码 = ""
                };
                if (!DataHandler.缴费结算(req, out res))
                    return Result.Fail("缴费结算失败");
                BillModel.缴费结算Out = res;
                ExtraPaymentModel.Complete = true;

                PrintModel.SetPrintInfo(true, new PrintInfo
                {
                    TypeMsg = "缴费成功",
                    TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分缴费",
                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                    PrintablesList = new List<Queue<IPrintable>>
                    {
                        BillPayPrintables()
                    },
                    TipImage = "提示_凭条"
                });
                Navigate(A.JF.Print);
                return Result.Success();
            }).Result;
        }

        protected override Queue<IPrintable> BillPayPrintables()
        {
            var queue = PrintManager.NewQueue("门诊费用缴费");
            var billPay = BillModel.缴费结算Out;
            var patientInfo = ChaKaModel.查询建档Out;
            var record = BillModel.门诊费用明细Out;
            var sb = new StringBuilder();
            sb.Append($"状态：缴费成功\n");
            sb.Append($"姓名：{patientInfo.病人姓名}\n");
            sb.Append($"门诊号：{patientInfo.就诊卡号}\n");
            sb.Append($"交易类型：自助缴费\n");
            sb.Append($"交易金额：{billPay.单据总金额.SafeToSplit(':')[1]}元\n");
            sb.Append($"自费金额：{billPay.应付金额.SafeToSplit(':')[1]}元\n");
            sb.Append($"医保金额：{billPay.医保报销金额.SafeToSplit(':')[1]}元\n");
            sb.Append($"结算单号：{billPay.电脑号.SafeToSplit(':')[1]}\n");
            sb.Append($"取药地址：{billPay.取药窗口.SafeToSplit(':')[1]}\n");
            sb.Append($"收费项目明细：\n");
            queue.Enqueue(new PrintItemText {Text = sb.ToString()});
            sb.Clear();
            queue.Enqueue(new PrintItemTriText("名称", "数量", "金额"));
            if (record?.FEIYONGMX != null)
                foreach (var detail in record.FEIYONGMX)
                    queue.Enqueue(new PrintItemTriText(detail.XIANGMUMC, detail.SHULIANG, $"{detail.JINE}元"));
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的缴费凭证\n");
            queue.Enqueue(new PrintItemText {Text = sb.ToString()});

            return queue;
        }
    }
}