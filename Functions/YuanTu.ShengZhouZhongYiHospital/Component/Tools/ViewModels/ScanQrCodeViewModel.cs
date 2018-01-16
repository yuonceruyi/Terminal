using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.ShengZhouZhongYiHospital.Component.Tools.ViewModels
{
    public class ScanQrCodeViewModel : YuanTu.Default.Component.Tools.ViewModels.ScanQrCodeViewModel
    {
        public override void OnSet()
        {
            base.OnSet();
            OptDic[(Business)(100)] = "2";
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var patientInfo = ExtraPaymentModel.PatientInfo;
            Name = patientInfo.Name;
            CardNo = patientInfo.CardNo;
            Remain = patientInfo.Remain.In元();

            CurrentBusiness = (int)ExtraPaymentModel.CurrentBusiness == 100 ? "体检缴费" : $"{ExtraPaymentModel.CurrentBusiness}";
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
                    idNo =string.IsNullOrEmpty(ExtraPaymentModel.PatientInfo.IdNo)? ExtraPaymentModel.PatientInfo.CardNo: ExtraPaymentModel.PatientInfo.IdNo,
                    idType = "1",
                    patientName = ExtraPaymentModel.PatientInfo.Name,
                    patientId = ExtraPaymentModel.PatientInfo.PatientId,
                    guarderId = ExtraPaymentModel.PatientInfo.GuardianNo,
                    billNo = GetInstance<IBusinessConfigManager>().GetFlowId("创建扫码订单的billNo"),
                    fee = ((int)ExtraPaymentModel.TotalMoney).ToString(),
                    optType = optType,
                    subject = (int)ExtraPaymentModel.CurrentBusiness == 100 ? "体检缴费" : ExtraPaymentModel.CurrentBusiness.ToString(),
                    deviceInfo = FrameworkConst.OperatorId,
                    feeChannel = code.ToString(),
                    source = FrameworkConst.OperatorId,
                    outId = OuterId.ToString()
                });
                Logger.Net.Info(ExtraPaymentModel.PatientInfo.ToJsonString());
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
                if (FrameworkConst.DoubleClick)
                {
                    Task.Run(() => { ExtraPaymentModel.FinishFunc.Invoke(); });
                }
                else
                {
                    Task.Factory.StartNew(AskingLoop); //创建成功则进行轮询
                }
            });
        }
    }
}
