using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Core.Log;
using YuanTu.BJArea.BeiJingSiPay;
using YuanTu.Devices.CardReader;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Core.Gateway;
using YuanTu.Core.Navigating;
using YuanTu.Core.Extension;
using Microsoft.Practices.Unity;
using YuanTu.Core.Services.LightBar;
using System.Threading;
using YuanTu.Core.Reporter;
using System.Threading.Tasks;
using YuanTu.Consts.Models;

namespace YuanTu.BJJingDuETYY.Component.Tools.ViewModels
{
    class SiPayViewModel : YuanTu.Default.Component.Tools.ViewModels.SiPayViewModel
    {
        [Dependency]
        public ILightBarService LightBarService { get; set; }

        protected Func<string, string, bool> SiPayDelegate;
        public string DeptCode { get; set; } //科室编码
        public string SiPayType { get; set; } //医保支付类别
                                              //protected Action<LoadingProcesser> MyAction;

        public bool _doEnter;
        protected bool _working;
        protected bool _isA6HuaDa;
        public SiPayViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {
         
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            //MyAction = Command;
            var config = GetInstance<IConfigurationManager>();
            _isA6HuaDa = (config.GetValue("A6_HuaDa") ?? "0") == "1";

            if (_isA6HuaDa)
            {
                IsShowConfirm = false;
                StartRead();
                ContentMsg = "请在左侧插卡口插入医保卡";
            }
            else
            {
                IsShowConfirm = true;
                ContentMsg = "请在读卡器黄灯亮起后点击确定读卡";

            }
            //SiPayDelegate = SiPay.Pay;
            InitDeptCode();
            SiPayType = ChoiceModel.Business.ToString();
            _doEnter = false;

            LightBarService?.PowerOn(LightItem.就诊卡社保卡);
        }

        public override void OnSet()
        {
            base.OnSet();
            BackUri = ResourceEngine.GetImageResourceUri("插社保卡");
        }
        public override bool OnLeaving(NavigationContext navigationContext)
        {
            LightBarService?.PowerOff();

            _working = false;
            StopRead();

            return base.OnLeaving(navigationContext);
        }

        protected virtual void InitDeptCode()
        {
            switch (ChoiceModel.Business)
            {
                case Business.挂号:
                    DeptCode = ScheduleModel.所选排班.deptCode;
                    break;
                default:
                    DeptCode = "0003";
                    break;
            }
        }


        protected virtual void StartRead()
        {
            Task.Run(() => StartA6HuaDa());
        }
        protected virtual void StopRead()
        {
            StopA6HuaDa();
        }

        protected virtual void StopA6HuaDa()
        {
            _rfCpuCardReader?.UnInitialize();
            _rfCpuCardReader?.DisConnect();
        }

        protected virtual void StartA6HuaDa()
        {
            try
            {
                var ret = _rfCpuCardReader.Connect();
                if (!ret.IsSuccess)
                {
                    ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                    ShowAlert(false, "友好提示", $"读卡器打开失败({ret.ResultCode})", debugInfo: ret.Message);
                    return;
                }
                _rfCpuCardReader.MoveCard(CardPos.不持卡位); //退卡

                if (!_rfCpuCardReader.Initialize().IsSuccess)
                {
                    ShowAlert(false, "友好提示", $"读卡器初始化失败({ret.ResultCode})", debugInfo: ret.Message);
                    return;
                }

                StartAdjustCardPos();
            }
            catch (Exception ex)
            {
                ShowAlert(false, "温馨提示", $"读卡失败:{ex.Message}");
                return;
            }
            finally
            {
                //DO Nothing
            }
        }
        private void StartAdjustCardPos()
        {
            _working = true;
            while (_working)
            {
                var pos = _rfCpuCardReader.GetCardPosition();
                if (pos.IsSuccess && (pos.Value == CardPos.停卡位 || pos.Value == CardPos.IC位))
                {
                    //有卡，启动判卡流程
                    _working = false;

                    if (pos.Value == CardPos.停卡位)
                    {
                        _rfCpuCardReader.MoveCard(CardPos.IC位); //移动到IC卡位
                        Thread.Sleep(200); //等卡停稳
                    }

                    View.Dispatcher.Invoke(() =>
                    {
                        StartReadA6HuaDa();
                    });
                    break;
                }
                Thread.Sleep(300);
            }

        }

        private void StartReadA6HuaDa()
        {
            if (!CheckSiCard())
            {
                ShowAlert(false, "医保读卡", "未检测到社保卡片，请确认朝向是否正确，重新插入社保卡", extend: new AlertExModel()
                {
                    HideCallback = tp =>
                    {
                        if (tp == AlertHideType.ButtonClick)
                        {
                            if (NavigationEngine.State == A.Third.SiPay) //确定还在当前页面
                            {
                                _rfCpuCardReader.MoveCard(CardPos.不持卡位);
                                StartRead();
                            }
                        }
                    }
                });
                return;
            }
            DoCharge();
        }

        private bool CheckSiCard()
        {
            try
            {
                _icCardReader.Connect();
                var result = _icCardReader.PowerOn(SlotNo.大卡座);
                if (!result.IsSuccess)
                {
                    return false;
                }
            }
            finally
            {
                _icCardReader.DisConnect();
            }
            return true;
        }

        public override void Confirm()
        {
            //            DoCommand(p =>
            //            {
            //                p.ChangeText("正在初始化医保网络，请稍候...");
            if (_doEnter)
                return;
            _doEnter = !_doEnter;

            DoCharge();

        }

        protected virtual void DoCharge()
        {
            //Logger.POS.Info($"[社保卡]支付开始");
            //var bRtn = false;
            //if (FrameworkConst.VirtualThridPay)
            //{
            //    bRtn = true;
            //    var pret = new TransResDto
            //    {
            //        RespCode = "00",
            //        RespInfo = "交易成功",
            //        CardNo = "2323233",
            //        Amount = "100",
            //        TransTime = "160301", //订单支付时间
            //        TransDate = "20160801", //订单支付日期
            //        Trace = "000021010547313130303536383836393436", //交易流水号
            //        Ref = "714ef11628724d6faadfb6bdab22e4b8", //医院支付流水号
            //        TId = "002323", //银联批次号,
            //        MId = "2222222",//POS流水号  
            //        Memo = "888888888888"
            //    };
            //    //医保支付结算返回信息  
            //    ExtraPaymentModel.PaymentResult = pret;
            //    ExtraPaymentModel.ThridRemain = 1000m;//账户余额
            //}
            //else
            //{
            //    bRtn = SiPayDelegate.Invoke(DeptCode, SiPayType);
            //}
            //if (!Convert.ToBoolean(bRtn))
            //{
            //    ShowAlert(false, "医保支付", "医保支付失败\n" + SiFunction.ErrMsg);
            //    TryPreview();
            //    return;
            //}
            //var tsk = ExtraPaymentModel.FinishFunc?.Invoke();
            //if (tsk != null)
            //{
            //    tsk.ContinueWith(payRet =>
            //    {
            //        if (!tsk?.Result.IsSuccess ?? false) //本地执行失败
            //        {
            //            var code = payRet?.Result.ResultCode ?? 0;
            //            if (DataHandler.UnKnowErrorCode.Contains(code))
            //            {
            //                var errorMsg = $"医保消费成功，网关返回未知结果{code}，打印凭条结束交易！\n请执凭条到人工咨询此交易结果！";

            //                PrintModel.SetPrintInfo(false, new PrintInfo
            //                {
            //                    TypeMsg = $"医保{ExtraPaymentModel.CurrentBusiness}单边账",
            //                    TipMsg = errorMsg,
            //                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
            //                    Printables = GatewayUnknowErrorPrintables(errorMsg),
            //                    TipImage = "提示_凭条"
            //                });
            //                PrintManager.Print();
            //                ShowAlert(false, "业务处理异常", errorMsg);
            //                SiFunction.Commit();//清除回滚队列
            //                Navigate(A.Home);
            //            }
            //            else
            //            {
            //                Logger.POS.Info($"[社保卡]本地支付失败，开始回滚社保支付");
            //                //回滚医保扣费
            //                var lRtn = SiFunction.Rollback();

            //                if (lRtn != 0)
            //                {
            //                    Logger.POS.Info($"[社保卡]支付回滚异常，详情：{SiFunction.ErrMsg}");
            //                    //PrintModel.SetPrintInfo(false, "医保支付", "医保支付",
            //                    //    ConfigurationManager.GetValue("Printer:Receipt"), CreatePrintables());
            //                    PrintModel.SetPrintInfo(false, new PrintInfo
            //                    {
            //                        TypeMsg = "医保支付",
            //                        TipMsg = "医保支付",
            //                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
            //                        Printables = CreatePrintables(),
            //                        TipImage = "提示_凭条"
            //                    });
            //                    PrintManager.Print();

            //                    ShowAlert(false, "扣费失败", "医保支付冲正失败，请持凭条与医院工作人员联系！\n请尝试其他支付方式！");
            //                }
            //                else
            //                {
            //                    Logger.POS.Info($"[社保卡]支付回滚成功");
            //                    ShowAlert(false, "扣费失败", $"医保支付失败！\n{tsk?.Result.Message}");
            //                }
            //                TryPreview();
            //            }
            //        }
            //        else
            //        {
            //            var patientInfo = PatientModel.当前病人信息;
            //            ChangeNavigationContent(A.CK.Info, A.ChaKa_Context, $"{patientInfo.name}\r\n医保余额{ExtraPaymentModel.ThridRemain.In元()}");
            //            Logger.POS.Info($"[社保卡]支付成功");
            //            SiFunction.Commit();
            //        }
            //    });
            //}
            //else
            //{
            //    Logger.POS.Info($"[社保卡]系统异常，交易操作没有任何返回");
            //    //PrintModel.SetPrintInfo(false, "医保支付", "医保支付",
            //    //    ConfigurationManager.GetValue("Printer:Receipt"), CreatePrintables());
            //    PrintModel.SetPrintInfo(false, new PrintInfo
            //    {
            //        TypeMsg = "医保支付",
            //        TipMsg = "医保支付",
            //        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
            //        Printables = CreatePrintables(),
            //        TipImage = "提示_凭条"
            //    });
            //    PrintManager.Print();

            //    ShowAlert(false, "扣费失败", "交易失败，请重试！");
            //    TryPreview();
            //}
            //});
        }
        private void Command(LoadingProcesser ctx)
        {


        }
        protected virtual void TryPreview()
        {
            if (NavigationEngine.State == A.Third.SiPay)
            {
                Preview();
            }
        }


        protected virtual Queue<IPrintable> CreatePrintables()
        {
            var queue = PrintManager.NewQueue("医保支付冲正失败");
            var transRes = ExtraPaymentModel.PaymentResult as TransResDto;
            var sb = new StringBuilder();
            sb.Append($"姓名:{IdCardModel.Name}\n");
            sb.Append($"就诊卡号:{CardModel.CardNo}\n");
            sb.Append($"交易流水号:{transRes.Trace}\n");
            sb.Append($"医院支付流水号:{transRes.Ref}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

        protected virtual Queue<IPrintable> GatewayUnknowErrorPrintables(string errorMsg)
        {
            var transRes = ExtraPaymentModel.PaymentResult as TransResDto;
            var queue = PrintManager.NewQueue($"医保{ExtraPaymentModel.CurrentBusiness}单边账");
            var sb = new StringBuilder();
            sb.Append($"状态：{errorMsg}\n");
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo.Name}\n");
            sb.Append($"门诊号：{ExtraPaymentModel.PatientInfo.PatientId}\n");
            sb.Append($"交易类型：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"交易金额：{ExtraPaymentModel.TotalMoney.In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"交易流水号：{transRes.Trace}\n");
            sb.Append($"医院支付流水号:{transRes.Ref}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
        public override void DoubleClick()
        {
            ShowAlert(false, "医保支付", "医保无法双击模拟");
        }

        #region Bindings
        private string _contentMsg;
        public string ContentMsg
        {
            get { return _contentMsg; }
            set
            {
                _contentMsg = value;
                OnPropertyChanged();
            }
        }
        private bool _isShowConfirm;
        public bool IsShowConfirm
        {
            get { return _isShowConfirm; }
            set
            {
                _isShowConfirm = value;
                OnPropertyChanged();
            }
        }

        #endregion Bindings
    }

}
