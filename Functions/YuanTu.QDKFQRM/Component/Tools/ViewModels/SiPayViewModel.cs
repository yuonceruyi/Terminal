using System;
using System.Collections.Generic;
using System.Text;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Core.Log;
using YuanTu.QDArea.QingDaoSiPay;
using YuanTu.Devices.CardReader;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Core.Navigating;

namespace YuanTu.QDKFQRM.Component.Tools.ViewModels
{
    class SiPayViewModel : YuanTu.QDKouQiangYY.Component.Tools.ViewModels.SiPayViewModel
    {
        protected Func<string, string, string, bool> SiPayDelegateIdNo;

        public SiPayViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {
         
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            SiPayDelegateIdNo = SiPay.Pay;
        }

        protected override void InitDeptCode()
        {
            switch (ChoiceModel.Business)
            {
                case Business.挂号:
                    DeptCode = ScheduleModel.所选排班.deptCode;
                    break;
                default:
                    DeptCode = "178";
                    break;
            }
        }


        public override void Confirm()
        {
            //            DoCommand(p =>
            //            {
            //                p.ChangeText("正在初始化医保网络，请稍候...");
            if (_doEnter)
                return;
            _doEnter = !_doEnter;

            Logger.POS.Info($"[社保卡]支付开始");
            var bRtn = false;
            if (FrameworkConst.VirtualThridPay)
            {
                bRtn = true;
                var pret = new TransResDto
                {
                    RespCode = "00",
                    RespInfo = "交易成功",
                    CardNo = "2323233",
                    Amount = "100",
                    TransTime = "160301", //订单支付时间
                    TransDate = "20160801", //订单支付日期
                    Trace = "000021010547313130303536383836393436", //交易流水号
                    Ref = "714ef11628724d6faadfb6bdab22e4b8", //医院支付流水号
                    TId = "002323", //银联批次号,
                    MId = "2222222",//POS流水号  
                    Memo = "888888888888"
                };
                //医保支付结算返回信息  
                ExtraPaymentModel.PaymentResult = pret;
                ExtraPaymentModel.ThridRemain = 1000m;//账户余额
            }
            else
            {
                bRtn = SiPayDelegateIdNo.Invoke(DeptCode, SiPayType,PatientModel.当前病人信息.idNo);
            }
            if (!Convert.ToBoolean(bRtn))
            {
                ShowAlert(false, "医保支付", "医保支付失败\n" + Function.ErrMsg);
                TryPreview();
                return;
            }
            var tsk = ExtraPaymentModel.FinishFunc?.Invoke();
            if (tsk != null)
            {
                tsk.ContinueWith(payRet =>
                {
                    if (!tsk?.Result.IsSuccess ?? false) //本地执行失败
                    {
                        Logger.POS.Info($"[社保卡]本地支付失败，开始回滚社保支付");
                        //回滚医保扣费
                        var lRtn = Function.Rollback();

                        if (lRtn != 0)
                        {
                            Logger.POS.Info($"[社保卡]支付回滚异常，详情：{Function.ErrMsg}");
                            //PrintModel.SetPrintInfo(false, "医保支付", "医保支付",
                            //    ConfigurationManager.GetValue("Printer:Receipt"), CreatePrintables());
                            PrintModel.SetPrintInfo(false, new PrintInfo
                            {
                                TypeMsg = "医保支付",
                                TipMsg = "医保支付",
                                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                Printables = CreatePrintables(),
                                TipImage = "提示_凭条"
                            });
                            PrintManager.Print();

                            ShowAlert(false, "扣费失败", "医保支付冲正失败，请持凭条与医院工作人员联系！\n请尝试其他支付方式！");
                        }
                        else
                        {
                            Logger.POS.Info($"[社保卡]支付回滚成功");
                            ShowAlert(false, "扣费失败", "医保支付失败！\n请尝试其他支付方式！");
                        }
                        TryPreview();
                    }
                    else
                    {
                        Logger.POS.Info($"[社保卡]支付成功");
                        Function.Commit();
                    }
                });
            }
            else
            {
                Logger.POS.Info($"[社保卡]系统异常，交易操作没有任何返回");
                //PrintModel.SetPrintInfo(false, "医保支付", "医保支付",
                //    ConfigurationManager.GetValue("Printer:Receipt"), CreatePrintables());
                PrintModel.SetPrintInfo(false, new PrintInfo
                {
                    TypeMsg = "医保支付",
                    TipMsg = "医保支付",
                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                    Printables = CreatePrintables(),
                    TipImage = "提示_凭条"
                });
                PrintManager.Print();

                ShowAlert(false, "扣费失败", "交易失败，请重试！");
                TryPreview();
            }
            //});

        }
    }
}
