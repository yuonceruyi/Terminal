using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.JiaShanHospital.Component.Tools.ViewModels
{
    public class ScanQrCodeViewModel:YuanTu.Default.Component.Tools.ViewModels.ScanQrCodeViewModel
    {
        private Visibility _remainVisibility;
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }
        [Dependency]
        public IPatientModel PatientModel { get; set; }

        public Visibility RemainVisibility
        {
            get { return _remainVisibility; }
            set
            {
                _remainVisibility = value;
                OnPropertyChanged();
            }
        }
        public override void OnSet()
        {
            base.OnSet();
            RemainVisibility = ChoiceModel.Business == Consts.Enums.Business.住院押金
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var patientInfo = ExtraPaymentModel.PatientInfo;
            Name = patientInfo.Name;
            CardNo = ChoiceModel.Business == Consts.Enums.Business.住院押金 ? PatientModel.Res住院患者信息查询.extend : patientInfo.CardNo;
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

        /// <summary>
        ///     初始化支付二维码，并调用轮询接口
        /// </summary>
        protected override void InitQrCode()
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
                    patientId = ExtraPaymentModel.PatientInfo.CardNo?? ExtraPaymentModel.PatientInfo.PatientId,//嘉善要求统一cardNo,平台无法改
                    guarderId = ExtraPaymentModel.PatientInfo.GuardianNo,
                    billNo = GetInstance<IBusinessConfigManager>().GetFlowId("创建扫码订单的billNo"),
                    fee = ((int)ExtraPaymentModel.TotalMoney).ToString(),
                    optType = optType,
                    subject = ExtraPaymentModel.CurrentBusiness.ToString(),
                    deviceInfo = FrameworkConst.OperatorId,
                    feeChannel = code.ToString(),
                    source = FrameworkConst.OperatorId,
                    outId = OuterId.ToString()
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

        private static readonly Dictionary<PayMethod, string> PayLogoMapping = new Dictionary<PayMethod, string>
        {
            [PayMethod.支付宝] = "图标_支付宝",
            [PayMethod.微信支付] = "图标_微信"
        };

        private void AskingLoop()
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

                    ExtraPaymentModel.PaymentResult = new 订单状态
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
    }
}
