using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Consts.Models.Recharge;
using YuanTu.Core.Gateway;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Consts.Services;
using System.Threading;
using System.Windows.Data;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.DB;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Services.UploadService;

namespace YuanTu.Default.Component.Tools.ViewModels
{
    public class ScanQrCodeViewModel : ViewModelBase
    {
        public override string Title => "扫码支付";


        //[Dependency]
        //public ICardModel CardModel { get; set; }

        //[Dependency]
        //public IPatientModel PatientModel { get; set; }

        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public IPrintModel PrintModel { get; set; }
        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }
        [Dependency]
        public ITradeUploadService TradeUploadService { get; set; }
        public ScanQrCodeViewModel()
        {
            TimeOut = 0; //设置不超时
        }

        protected static readonly Dictionary<PayMethod, string> PayLogoMapping = new Dictionary<PayMethod, string>
        {
            [PayMethod.支付宝] = "图标_支付宝",
            [PayMethod.微信支付] = "图标_微信"
        };

        protected static readonly Dictionary<Business, string> OptDic = new Dictionary<Business, string>
        {
            [Business.充值] = "1",
            [Business.缴费] = "2",
            [Business.挂号] = "3",
            [Business.住院押金] = "4",
            //5、退费
            [Business.预约] = "6",
            [Business.取号] = "7",
            [Business.建档] = "8",
            [Business.出院结算] = "9"
        };

        protected bool Exitloop; //表示已经退出轮询，保证轮询查到的数据是最新的
        protected bool Looping; //表示正在进行轮询，退出页面前必须保证已经退出轮询

       
        protected bool Procuding; //该字段标识该业务已经进入His环节，退出页面前，不需要进行取消订单
        protected 订单扫码 订单扫码;
        protected 订单状态 订单状态;
        protected long OuterId;

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var patientInfo = ExtraPaymentModel.PatientInfo;
            Name = patientInfo.Name;
            CardNo = patientInfo.CardNo;
            Remain = patientInfo.Remain.In元();

            CurrentBusiness = $"{ExtraPaymentModel.CurrentBusiness}";
            Amount = ExtraPaymentModel.TotalMoney;


            订单扫码 = null;
            订单状态 = null;
            QrCodeImage = null;
            Looping = false;
            //OuterId = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
            OuterId = DateTimeCore.Now.Ticks;

            ExtraPaymentModel.Complete = false;
            InitQrCode();

        }

        public override Task<bool> OnLeavingAsync(NavigationContext navigationContext)
        {
            return DoCommand(lp =>
            {

                lp.ChangeText("正在确认当前操作结果，请稍后...");
                return NeedLeaving(lp, navigationContext);
            }, false);
        }

        //public override bool OnLeaving(NavigationContext navigationContext)
        //{
        //    return NeedLeaving(null,navigationContext);
        //}

        protected virtual bool NeedLeaving(LoadingProcesser lp,NavigationContext navigationContext)
        {
            //if (Procuding)
            //{
            //    //e.Cancel = true;
            //    return false;
            //}
            Looping = false;
            var seed = 0;
            while ((!Exitloop) && (!Procuding)) //确认已经退出循环
            {
                if (seed++ > 10) //重试超过10次，退出失败，则返回原状态
                {
                    //e.Cancel = true;
                    Looping = true;
                    return false;
                }
                Thread.Sleep(500);
            }
            if (!ExtraPaymentModel.Complete && 订单扫码 != null)
            {
                try
                {
                    Logger.Main.Info($"扫码页面退出");
                    var reportResult = DataHandlerEx.操作成功状态上传(new req操作成功状态上传
                    {
                        outTradeNo = 订单扫码?.outTradeNo,
                        status = "101" //失败

                    });
                    if (!reportResult?.success ?? true)
                        Logger.Main.Error($"操作状态上传失败 用户Id:{ExtraPaymentModel.PatientInfo.PatientId} 平台支付流水:{订单扫码?.outTradeNo} 二维码:{订单扫码?.qrCode} 信息:{reportResult?.msg}");
                }
                catch (Exception ex)
                {
                    Logger.Main.Error($"操作状态上传异常 用户Id:{ExtraPaymentModel.PatientInfo.PatientId} 平台支付流水:{订单扫码?.outTradeNo} 二维码:{订单扫码?.qrCode} 异常:{ex.Message}");
                }
            }

            订单扫码 = null;
            return true;
        }

        /// <summary>
        ///     初始化支付二维码，并调用轮询接口
        /// </summary>
        protected virtual void InitQrCode()
        {
            var code = -1;
            switch (ExtraPaymentModel.CurrentPayMethod)
            {
                case PayMethod.微信支付:
                    Tips = "请使用微信扫一扫支付";
                    code = 2;
                    break;

                case PayMethod.支付宝:
                    Tips = "请使用支付宝扫一扫支付";
                    code = 1;
                    break;

                default:
                    ShowAlert(false, "温馨提示", "不支持该支付方式!");
                    return;
            }
            DoCommand(p =>
            {
                p.ChangeText("正在创建扫码订单，请稍候...");
                var optType = OptDic.ContainsKey(ExtraPaymentModel.CurrentBusiness)
                    ? OptDic[ExtraPaymentModel.CurrentBusiness]
                    : string.Empty;
                var rest = DataHandlerEx.创建扫码订单(new req创建扫码订单
                {
                    idNo = ExtraPaymentModel.PatientInfo.IdNo,
                    idType = "1",
                    patientName = ExtraPaymentModel.PatientInfo.Name,
                    patientId = ExtraPaymentModel.PatientInfo.PatientId,
                    guarderId = ExtraPaymentModel.PatientInfo.GuardianNo,
                    billNo = GetInstance<IBusinessConfigManager>().GetFlowId("创建扫码订单的billNo"),
                    fee = ((int)ExtraPaymentModel.TotalMoney).ToString(),
                    optType = optType,
                    subject = ExtraPaymentModel.CurrentBusiness.ToString(),
                    deviceInfo = FrameworkConst.OperatorId,
                    feeChannel = code.ToString(),
                    source = FrameworkConst.OperatorId,
                    outId=OuterId.ToString()
                });
                if (!rest.success)
                {
                    ShowAlert(false, "温馨提示", "获取支付二维码失败\r\n" + rest.msg);
                    Exitloop = true;
                    Preview(); //回退
                    return;
                }
                Logger.Main.Info(
                    $"病人[{ExtraPaymentModel.PatientInfo.Name} {ExtraPaymentModel.PatientInfo.PatientId}] 开始[{ExtraPaymentModel.CurrentPayMethod}]");
                订单扫码 = rest.data;

                var barQrCodeGenerater = GetInstance<IBarQrCodeGenerator>();
                QrCodeImage = barQrCodeGenerater.QrcodeGenerate(订单扫码.qrCode,
                    Image.FromFile(
                       ResourceEngine
                            .GetResourceFullPath(PayLogoMapping[ExtraPaymentModel.CurrentPayMethod])));


                Looping = true;
                Procuding = false;
                Exitloop = false;
                Task.Factory.StartNew(AskingLoop); //创建成功则进行轮询
            });
        }


        protected virtual void AskingLoop()
        {
            var req = new req查询订单状态
            {
                outTradeNo = 订单扫码.outTradeNo
            };
            while (Looping)
            {
                try
                {
                    Thread.Sleep(1000);
                    var rest = DataHandlerEx.查询订单状态(req);
                    if (rest == null || !rest.success || rest.data == null)
                    {
                        Logger.Main.Error(
                            $"用户Id:{ExtraPaymentModel.PatientInfo.PatientId} 平台支付流水:{req.outTradeNo} 二维码:{订单扫码?.qrCode} 结果:{rest?.ToJsonString()}");
                        continue;
                    }
                    订单状态 = rest.data;

                    if (!Looping)
                        break;

                    if (rest.data.status == "101") //获取的应该有:101支付中 200支付成功 201支付失败
                        continue;

                    ExtraPaymentModel.PaymentResult =  new 订单状态
                    {
                        outTradeNo = req.outTradeNo,
                        fee = rest.data.fee,
                        status = rest.data.status,
                        paymentTime = rest.data.paymentTime,
                        outRefundNo = rest.data.outRefundNo,
                        statusDes = rest.data.statusDes,
                        outPayNo = rest.data.outPayNo,
                        buyerAccount = rest.data.buyerAccount,
                    };

                    ExtraPaymentModel.Complete = rest.data.status == "200";
                    if (!ExtraPaymentModel.Complete)
                    {
                        //ShowMsg("付款异常，状态码："+ rest.data.status);
                        continue;
                    }
                    StartPay();
                }
                catch (Exception ex)
                {
                    Logger.Main.Error(
                        $"用户Id:{ExtraPaymentModel.PatientInfo.PatientId} 平台支付流水:{req.outTradeNo} 二维码:{订单扫码?.qrCode} 异常:{ex.Message}"
                    );
                }
                
            }
            Exitloop = true;
        }

        protected virtual void StartPay()
        {
            Looping = false;
            Procuding = true;
            var rechargeb = new[] {Business.充值, Business.住院押金};
            var isrecharge = rechargeb.Contains(ExtraPaymentModel.CurrentBusiness);
            //上送交易信息
            TradeUploadService.UploadAsync(new TradeInfo()
            {
                TradeName = ExtraPaymentModel.CurrentBusiness.ToString(),
                PatientId = ExtraPaymentModel.PatientInfo.PatientId,
                PatientName = ExtraPaymentModel.PatientInfo.Name,
                CardNo = ExtraPaymentModel.PatientInfo.CardNo,
                CardType = ExtraPaymentModel.PatientInfo.CardType,
                IdNo = ExtraPaymentModel.PatientInfo.IdNo,
                GuardianIdNo = ExtraPaymentModel.PatientInfo.GuardianNo,
                PayMethod = ExtraPaymentModel.CurrentPayMethod,
                TradeType = isrecharge?TradeType.充值成功 : TradeType.交易成功,
                TradeId = 订单状态?.outTradeNo,
                OriginTradeId = "",
                AccountNo = 订单状态?.buyerAccount,
                Amount = ExtraPaymentModel.TotalMoney,
                TradeDetail = 订单状态.ToJsonString(),
            });
            ExtraPaymentModel.FinishFunc?.Invoke().ContinueWith(rt =>
            {
                var actRest = rt.Result;
                var code = actRest.ResultCode;
                if (DataHandler.UnKnowErrorCode.Contains(code))
                {
                    var errorMsg =
                        $"{actRest.Message.BackNotNullOrEmpty("扫码消费成功，网关返回未知结果")}\r\n交易返回码:{code}\r\n请执凭条到人工服务台咨询此交易结果！";
                    // var errorMsg = $"扫码消费成功，网关返回未知结果{code}，打印凭条结束交易！\n请执凭条到人工咨询此交易结果！";
                    ShowAlert(false, "温馨提示", errorMsg);
                    打印网关未知异常凭证(errorMsg);
                }
                else
                {
                    if (actRest.IsSuccess)
                    {
                        try
                        {
                            Logger.Main.Info($"扫码支付成功流程");
                            var reportResult = DataHandlerEx.操作成功状态上传(new req操作成功状态上传
                            {
                                outTradeNo = 订单状态?.outTradeNo,
                                status = "200" //成功

                            });
                            if (!reportResult?.success ?? true)
                                Logger.Main.Error(
                                    $"操作状态上传失败 用户Id:{ExtraPaymentModel.PatientInfo.PatientId} 平台支付流水:{订单扫码?.outTradeNo} 二维码:{订单扫码?.qrCode} 信息:{reportResult?.msg}");
                        }
                        catch (Exception ex)
                        {
                            Logger.Main.Error(
                                $"操作状态上传异常 用户Id:{ExtraPaymentModel.PatientInfo.PatientId} 平台支付流水:{订单扫码?.outTradeNo} 二维码:{订单扫码?.qrCode} 异常:{ex.Message}");
                        }

                        Procuding = false;
                        return;
                    }
                    else
                    {
                        try
                        {
                            Logger.Main.Info($"扫码支付失败流程");
                            var reportResult = DataHandlerEx.操作成功状态上传(new req操作成功状态上传
                            {
                                outTradeNo = 订单状态?.outTradeNo,
                                status = "101" //失败

                            });
                            if (!reportResult?.success ?? true)
                                Logger.Main.Error(
                                    $"操作状态上传失败 用户Id:{ExtraPaymentModel.PatientInfo.PatientId} 平台支付流水:{订单扫码?.outTradeNo} 二维码:{订单扫码?.qrCode} 信息:{reportResult?.msg}");

                            //上送交易信息
                            TradeUploadService.UploadAsync(new TradeInfo()
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
                                TradeId = 订单状态?.outTradeNo,
                                OriginTradeId = "",
                                AccountNo = 订单状态?.buyerAccount,
                                Amount = ExtraPaymentModel.TotalMoney,
                                TradeDetail = reportResult.ToJsonString()
                            });
                        }
                        catch (Exception ex)
                        {
                            Logger.Main.Error(
                                $"操作状态上传异常 用户Id:{ExtraPaymentModel.PatientInfo.PatientId} 平台支付流水:{订单扫码?.outTradeNo} 二维码:{订单扫码?.qrCode} 异常:{ex.Message}");
                        }

                        Procuding = false;
                        return;
                    }
                }

                Procuding = false;
                if (NavigationEngine.State == A.ThirdPay) //只有在当前页，才允许Leaving
                {
                    Preview();
                    return;
                }
                Procuding = false;
            });
        }
        protected virtual void 打印退费失败凭证(string 退费原因,string 失败原因)
        {
            var queue = PrintManager.NewQueue($"{ExtraPaymentModel.CurrentPayMethod}订单取消失败");

            var sb = new StringBuilder();
      
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo?.Name}\n");
            sb.Append($"病案号：{ExtraPaymentModel.PatientInfo?.PatientId}\n");
            sb.Append($"当前业务：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"订单号：{订单扫码?.outTradeNo}\n");
            sb.Append($"交易金额：{ExtraPaymentModel?.TotalMoney.In元()}\n");
            sb.Append($"支付账户：{订单状态?.buyerAccount}\n");
            sb.Append($"退费原因：{退费原因}\n");
            sb.Append($"失败原因：{失败原因}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请联系导诊处理退费事宜，祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), queue);
        }
        protected virtual void 打印网关未知异常凭证(string 异常描述)
        {
            var queue = PrintManager.NewQueue($"{ExtraPaymentModel.CurrentPayMethod}单边账");
            var sb = new StringBuilder();
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo?.Name}\n");
            sb.Append($"病案号：{ExtraPaymentModel.PatientInfo?.PatientId}\n");
            sb.Append($"当前业务：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"订单号：{订单扫码?.outTradeNo}\n");
            sb.Append($"交易金额：{ExtraPaymentModel?.TotalMoney.In元()}\n");
            sb.Append($"支付账户：{订单状态?.buyerAccount}\n");
            sb.Append($"异常描述：{异常描述}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请联系导诊处理，祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), queue);
        }


        #region Overrides of ViewModelBase

        /// <summary>
        ///     当处于IsLocal状态下时，双击工作区会引发此事件
        /// </summary>
        public override void DoubleClick()
        {
            if (FrameworkConst.VirtualThridPay&& Looping)
            {
                Looping = false;
                StartPay();
            }
        }

        #endregion

        #region 信息绑定
        private string _hint = "提示信息";
        private string _name;
        private string _cardNo;
        private string _remain;
        private string _currentBusiness;
        private Image _qrCodeIamge;
        private string _tips;
        private decimal _amount;

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string CardNo
        {
            get { return _cardNo; }
            set
            {
                _cardNo = value;
                OnPropertyChanged();
            }
        }

        public string Remain
        {
            get { return _remain; }
            set
            {
                _remain = value;
                OnPropertyChanged();
            }
        }

        public string CurrentBusiness
        {
            get { return _currentBusiness; }
            set
            {
                _currentBusiness = value;
                OnPropertyChanged();
            }
        }

       
      

        public string Tips
        {
            get { return _tips; }
            set
            {
                _tips = value;
                OnPropertyChanged();
            }
        }

        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                OnPropertyChanged();
            }
        }
        public Image QrCodeImage
        {
            get { return _qrCodeIamge; }
            set
            {
                _qrCodeIamge = value;
                OnPropertyChanged();
            }
        }
        #endregion
    }
}
