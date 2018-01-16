using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Gateway;
using YuanTu.Core.Log;
using YuanTu.Core.Services.UploadService;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Devices.UnionPay;

namespace YuanTu.Default.Component.Tools.ViewModels
{
    public class PosViewModel : ViewModelBase
    {
        public bool _hasExit;
        public bool _mustClose;
        public override string Title => "银联消费";

        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }

        [Dependency]
        public IMisposUnionService MisposUnionService { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public ITradeUploadService TradeUploadService { get; set; }

        #region Overrides of ViewModelBase

        /// <summary>
        ///     当处于IsLocal状态下时，双击工作区会引发此事件
        /// </summary>
        public override void DoubleClick()
        {
            if (FrameworkConst.VirtualThridPay)
                StartPay();
        }

        #endregion Overrides of ViewModelBase

        #region Overrides of ViewModelBase

        public override void OnSet()
        {
            FingerUri = ResourceEngine.GetImageResourceUri("动画素材_手指");
            KeyboardUri = ResourceEngine.GetImageResourceUri("动画素材_金属键盘");
            BackUri = ResourceEngine.GetImageResourceUri("插卡口");
            CardUri = ResourceEngine.GetImageResourceUri("动画素材_银行卡");
        }

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
            Business = $"{ExtraPaymentModel.CurrentBusiness}";
            BankPassword = "";
            Amount = ExtraPaymentModel.TotalMoney;
            ShowKeyboardAnimation = false;
            ShowInputPassWord = false;
            StartPosFlow();
        }

        /// <summary>
        ///     离开当前页面时触发
        /// </summary>
        /// <param name="navigationContext"></param>
        /// <returns>是否允许跳转</returns>
        public override bool OnLeaving(NavigationContext navigationContext)
        {
            if (MisposUnionService.IsBusy)
                return false;
            _mustClose = true;
            CloseDevices("取消操作");
            return base.OnLeaving(navigationContext);
        }

        /// <summary>
        ///     开始银联交易全部流程
        /// </summary>
        protected virtual void StartPosFlow()
        {
            Tips = "初始化设备...";
            DoCommand(p =>
            {
                p.ChangeText("正在初始化银联网络，请稍候...");
                return InitializePos();
            }).ContinueWith(initTask =>
            {
                if (initTask.Result && SureGetCard() && SurePassword())
                    return true;
                TryPreview();
                return false;
            }).ContinueWith(passwordTask =>
            {
                if (FrameworkConst.VirtualThridPay)
                    return;
                if (passwordTask.Result)
                {
                    StartPay();
                    return;
                }
                _mustClose = true;
                CloseDevices("异常操作，结束");
            });
        }

        protected virtual bool InitializePos()
        {
            if (FrameworkConst.VirtualThridPay)
                return true;

            var banCardMediaType = BanCardMediaType.磁条 | BanCardMediaType.IC芯片;

            if (IsQuickPay())
                banCardMediaType = BanCardMediaType.闪付;

            var ret = MisposUnionService.Initialize(ExtraPaymentModel.CurrentBusiness, "umsips\\umsapi.dll", banCardMediaType);
            if (!ret.IsSuccess)
                ShowAlert(false, "银联异常", ret.Message);
            Logger.POS.Info($"[{Title}]银联支付，初始化银联参数，结果:{ret.IsSuccess} 内容:{ret.Message}");
            return ret.IsSuccess;
        }

        protected virtual bool SureGetCard()
        {
            PlaySound(SoundMapping.银行卡支付);
            Tips = "请插入银行卡...";
            if (FrameworkConst.VirtualThridPay)
                return true;
            if (_mustClose)
                return false;
            var banCardMediaType = BanCardMediaType.磁条 | BanCardMediaType.IC芯片;

            if (IsQuickPay())
            {
                if (_mustClose)
                    return false;
                var retReq = MisposUnionService.SetReq(TransType.消费, ExtraPaymentModel.TotalMoney);
                if (!retReq.IsSuccess)
                {
                    ShowAlert(false, "银联读卡异常", retReq.Message);
                    return retReq.IsSuccess;
                }

                banCardMediaType = BanCardMediaType.闪付;
            }
            if (_mustClose)
                return false;
            var ret = MisposUnionService.ReadCard(banCardMediaType);
            Logger.POS.Info($"[{Title}]银联支付，读取银行卡信息，结果:{ret.IsSuccess} 内容:{ret.Message}");
            if (!ret.IsSuccess)
                ShowAlert(false, "银联读卡异常", ret.Message);
            return ret.IsSuccess;
        }

        protected virtual bool SurePassword()
        {
            if (_mustClose)
                return false;
            Tips = "请输入密码...";
            ShowKeyboardAnimation = true;
            ShowInputPassWord = true;

            PlaySound(SoundMapping.输入银行卡密码);
            if (FrameworkConst.VirtualThridPay)
                return true;
            if (_mustClose)
                return false;
            var ret = MisposUnionService.StartKeyboard(keyText =>
            {
                BankPassword = keyText.KeyContent;
            });
            if (!ret.IsSuccess)
                ShowAlert(false, "银联读卡异常", ret.Message);
            return ret.IsSuccess;
        }

        protected virtual void TryPreview()
        {
            if (NavigationEngine.State == A.Third.PosUnion)
                Preview();
        }

        protected virtual void StartPay()
        {
            DoCommand(p =>
            {
                if (FrameworkConst.VirtualThridPay)
                {
                    var dto = new TransResDto
                    {
                        RespCode = "00",
                        RespInfo = "交易成功",
                        CardNo = "622319******7113",
                        Amount = ExtraPaymentModel.TotalMoney.ToString("0"),
                        Trace = "011132",
                        Batch = "000021",
                        TransDate = DateTimeCore.Now.ToString("yyyyMMdd"),
                        TransTime = DateTimeCore.Now.ToString("HHmmss"),
                        Ref = DateTimeCore.Now.Ticks.ToString(), //"094117503517",
                        Auth = "",
                        MId = "302053280620002",
                        TId = "00020026",
                        Memo = "",
                        Lrc = ""
                    };
                    return Result<TransResDto>.Success(dto);
                }
                return MisposUnionService.DoSale(ExtraPaymentModel.TotalMoney);
            }).ContinueWith(AfterDoSale);
        }

        private void AfterDoSale(Task<Result<TransResDto>> ret)
        {
            var doSaleResult = ret.Result;
            if (!doSaleResult.IsSuccess)
            {
                ShowAlert(false, "扣费失败", $"{doSaleResult.Message}");
                CloseDevices("扣费失败");
                TryPreview();
                return;
            }
            //上送交易信息
            var rechargeb = new[] {Consts.Enums.Business.充值, Consts.Enums.Business.住院押金};
            var isrecharge = rechargeb.Contains(ExtraPaymentModel.CurrentBusiness);
            var printerName = ConfigurationManager.GetValue("Printer:Receipt");

            TradeUploadService.UploadAsync(new TradeInfo
            {
                TradeName = ExtraPaymentModel.CurrentBusiness.ToString(),
                PatientId = ExtraPaymentModel.PatientInfo.PatientId,
                PatientName = ExtraPaymentModel.PatientInfo.Name,
                CardNo = ExtraPaymentModel.PatientInfo.CardNo,
                CardType = ExtraPaymentModel.PatientInfo.CardType,
                IdNo = ExtraPaymentModel.PatientInfo.IdNo,
                GuardianIdNo = ExtraPaymentModel.PatientInfo.GuardianNo,
                PayMethod = ExtraPaymentModel.CurrentPayMethod,
                TradeType = isrecharge ? TradeType.充值成功 : TradeType.交易成功,
                TradeId = doSaleResult.Value.Ref,
                OriginTradeId = "",
                AccountNo = doSaleResult.Value.CardNo,
                Amount = decimal.Parse(doSaleResult.Value.Amount),
                TradeDetail = doSaleResult.Value.ToJsonString()
            });
            ExtraPaymentModel.PaymentResult = doSaleResult.Value;
            var task = ExtraPaymentModel.FinishFunc?.Invoke();
            if (task == null)
            {
                PrintManager.QuickPrint(printerName, BusinessFailPrintables());
                ShowAlert(false, "扣费失败", "交易失败，请重试！");
                CloseDevices("系统异常，交易操作没有任何返回！");
                TryPreview();
                return;
            }
            task.ContinueWith(payRet =>
            {
                var payResult = payRet.Result;
                if (payResult.IsSuccess)
                {
                    CloseDevices("消费结束");
                    return;
                }
                if (FrameworkConst.VirtualThridPay)
                {
                    ShowAlert(false, "扣费失败", "交易失败，请重试！" + payResult.Message);
                    return;
                }
                var code = payResult.ResultCode;
                if (DataHandler.UnKnowErrorCode.Contains(code))
                {
                    var errorMsg = $"{payResult.Message.BackNotNullOrEmpty("银联消费成功，网关返回未知结果")}\r\n交易返回码:{code}\r\n请执凭条到人工服务台咨询此交易结果！";
                    PrintManager.QuickPrint(printerName, GatewayUnknowErrorPrintables(errorMsg));
                    ShowAlert(false, "业务处理异常", errorMsg);
                    CloseDevices(errorMsg);
                    Navigate(A.Home);
                    return;
                }
                if (IsQuickPay())
                {
                    var success = InitializePos();
                    if (!success)
                    {
                        PrintManager.QuickPrint(printerName, RefundFailPrintables(payResult.Message, "冲正初始化失败"));
                        ShowAlert(false, "扣费失败", "银联冲正失败，请持凭条与医院工作人员联系！\n请尝试其他支付方式！");
                    }
                    var retReq = MisposUnionService.SetReq(TransType.冲正, ExtraPaymentModel.TotalMoney);
                    if (!retReq.IsSuccess)
                    {
                        PrintManager.QuickPrint(printerName, RefundFailPrintables(payResult.Message, "冲正设置入参失败"));
                        ShowAlert(false, "扣费失败", "银联冲正失败，请持凭条与医院工作人员联系！\n请尝试其他支付方式！");
                    }
                }
                var refundResult = MisposUnionService.Refund(payResult.Message);
                if (!refundResult.IsSuccess)
                {
                    PrintManager.QuickPrint(printerName, RefundFailPrintables(payResult.Message, refundResult.Message));
                    ShowAlert(false, "扣费失败", "银联冲正失败，请持凭条与医院工作人员联系！\n请尝试其他支付方式！");
                }
                else
                {
                    //上送交易信息
                    TradeUploadService.UploadAsync(new TradeInfo
                    {
                        TradeName = ExtraPaymentModel.CurrentBusiness.ToString(),
                        PatientId = ExtraPaymentModel.PatientInfo.PatientId,
                        PatientName = ExtraPaymentModel.PatientInfo.Name,
                        CardNo = ExtraPaymentModel.PatientInfo.CardNo,
                        CardType = ExtraPaymentModel.PatientInfo.CardType,
                        IdNo = ExtraPaymentModel.PatientInfo.IdNo,
                        GuardianIdNo = ExtraPaymentModel.PatientInfo.GuardianNo,
                        PayMethod = ExtraPaymentModel.CurrentPayMethod,
                        TradeType = isrecharge ? TradeType.充值撤销成功 : TradeType.交易撤销成功,
                        TradeId = doSaleResult.Value.Ref,
                        OriginTradeId = refundResult.Value.Ref,
                        AccountNo = doSaleResult.Value.CardNo,
                        Amount = decimal.Parse(doSaleResult.Value.Amount),
                        TradeDetail = refundResult.Value.ToJsonString()
                    });
                    ShowAlert(false, "扣费失败", "交易失败！\n" + payResult.Message);
                }
                CloseDevices("消费结束");
                TryPreview();
            });
        }

        private bool IsQuickPay()
        {
            return CurrentStrategyType() == DeviceType.Clinic ||
                                ExtraPaymentModel.CurrentPayMethod == PayMethod.银联闪付 ||
                                ExtraPaymentModel.CurrentPayMethod == PayMethod.苹果支付;
        }

        protected virtual void CloseDevices(string reason)
        {
            if (FrameworkConst.VirtualThridPay)
                return;
            MisposUnionService.DisConnect(reason);
            if (ExtraPaymentModel.Complete || _mustClose)
            {
                if (_hasExit)
                    return;
                _hasExit = true;
                MisposUnionService.UnInitialize(reason);
            }
        }

        protected virtual Queue<IPrintable> BusinessFailPrintables()
        {
            var queue = PrintManager.NewQueue($"银联{ExtraPaymentModel.CurrentBusiness}单边账");
            //var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var sb = new StringBuilder();
            sb.Append($"状态：银联扣费成功，业务处理失败\n");
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo.Name}\n");
            sb.Append($"门诊号：{ExtraPaymentModel.PatientInfo.PatientId}\n");
            sb.Append($"交易类型：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"交易金额：{ExtraPaymentModel.TotalMoney.In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText {Text = sb.ToString()});
            return queue;
        }

        protected virtual Queue<IPrintable> RefundFailPrintables(string refundReason, string refundFailReason)
        {
            var queue = PrintManager.NewQueue($"银联{ExtraPaymentModel.CurrentBusiness}单边账");

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
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText {Text = sb.ToString()});
            return queue;
        }

        protected virtual Queue<IPrintable> GatewayUnknowErrorPrintables(string errorMsg)
        {
            var queue = PrintManager.NewQueue($"银联{ExtraPaymentModel.CurrentBusiness}单边账");
            var sb = new StringBuilder();
            sb.Append($"状态：{errorMsg}\n");
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo.Name}\n");
            sb.Append($"门诊号：{ExtraPaymentModel.PatientInfo.PatientId}\n");
            sb.Append($"交易类型：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"交易金额：{ExtraPaymentModel.TotalMoney.In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"银联流水：{(ExtraPaymentModel.PaymentResult as TransResDto)?.Ref}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText {Text = sb.ToString()});
            return queue;
        }

        #endregion Overrides of ViewModelBase

        #region 信息绑定

        private string _hint = "提示信息";
        private string _name;
        private string _cardNo;
        private string _remain;
        private string _business;
        private string _bankPassword;
        private string _tips;
        private decimal _amount;
        private bool _showKeyboardAnimation;
        private bool _showInputPassWord;
        private Uri _backUri;
        private Uri _cardUri;
        private Uri _fingerUri;
        private Uri _keyboardUri;

        public Uri CardUri
        {
            get => _cardUri;
            set
            {
                _cardUri = value;
                OnPropertyChanged();
            }
        }

        public Uri BackUri
        {
            get => _backUri;
            set
            {
                _backUri = value;
                OnPropertyChanged();
            }
        }

        public Uri KeyboardUri
        {
            get => _keyboardUri;
            set
            {
                _keyboardUri = value;
                OnPropertyChanged();
            }
        }

        public Uri FingerUri
        {
            get => _fingerUri;
            set
            {
                _fingerUri = value;
                OnPropertyChanged();
            }
        }

        public string Hint
        {
            get => _hint;
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string CardNo
        {
            get => _cardNo;
            set
            {
                _cardNo = value;
                OnPropertyChanged();
            }
        }

        public string Remain
        {
            get => _remain;
            set
            {
                _remain = value;
                OnPropertyChanged();
            }
        }

        public string Business
        {
            get => _business;
            set
            {
                _business = value;
                OnPropertyChanged();
            }
        }

        public string BankPassword
        {
            get => _bankPassword;
            set
            {
                _bankPassword = value;
                OnPropertyChanged();
            }
        }

        public string Tips
        {
            get => _tips;
            set
            {
                _tips = value;
                OnPropertyChanged();
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged();
            }
        }

        public bool ShowKeyboardAnimation
        {
            get => _showKeyboardAnimation;
            set
            {
                _showKeyboardAnimation = value;
                OnPropertyChanged();
            }
        }

        public bool ShowInputPassWord
        {
            get => _showInputPassWord;
            set
            {
                _showInputPassWord = value;
                OnPropertyChanged();
            }
        }

        #endregion 信息绑定
    }
}