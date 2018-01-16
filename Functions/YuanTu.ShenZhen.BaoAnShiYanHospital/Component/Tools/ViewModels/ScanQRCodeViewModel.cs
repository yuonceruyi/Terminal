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

namespace YuanTu.ShenZhen.BaoAnShiYanHospital.Component.Tools.ViewModels
{
    public class ScanQrCodeViewModel : Default.Component.Tools.ViewModels.ScanQrCodeViewModel
    {
        /// <summary>
        ///     初始化支付二维码，并调用轮询接口
        /// </summary>
        protected override void InitQrCode()
        {
            base.InitQrCode();
            return;
            //var code = -1;
            //switch (ExtraPaymentModel.CurrentPayMethod)
            //{
            //    case PayMethod.微信支付:
            //        Tips = "请使用微信扫一扫支付";
            //        code = 2;
            //        break;

            //    case PayMethod.支付宝:
            //        Tips = "请使用支付宝扫一扫支付";
            //        code = 1;
            //        break;

            //    default:
            //        ShowAlert(false, "温馨提示", "不支持该支付方式!");
            //        return;
            //}
            //DoCommand(p =>
            //{
            //    p.ChangeText("正在创建扫码订单，请稍候...");
            //    var optType = OptDic.ContainsKey(ExtraPaymentModel.CurrentBusiness)
            //        ? OptDic[ExtraPaymentModel.CurrentBusiness]
            //        : string.Empty;
            //    string newIdNo = ExtraPaymentModel.PatientInfo.IdNo;
            //    if (string.IsNullOrEmpty(newIdNo))
            //    {
            //        if (string.IsNullOrEmpty(ExtraPaymentModel.PatientInfo.GuardianNo))
            //        {
            //            newIdNo = ConfigBaoAnShiYanHospital.DefaultIdCardNo;
            //        }
            //    }

            //    var rest = DataHandlerEx.创建扫码订单(new req创建扫码订单
            //    {
            //        idNo = newIdNo,
            //        idType = "1",
            //        patientName = ExtraPaymentModel.PatientInfo.Name,
            //        patientId = ExtraPaymentModel.PatientInfo.PatientId,
            //        guarderId = ExtraPaymentModel.PatientInfo.GuardianNo,
            //        billNo = GetInstance<IBusinessConfigManager>().GetFlowId("创建扫码订单的billNo"),
            //        fee = ((int)ExtraPaymentModel.TotalMoney).ToString(),
            //        optType = optType,
            //        subject = ExtraPaymentModel.CurrentBusiness.ToString(),
            //        deviceInfo = FrameworkConst.OperatorId,
            //        feeChannel = code.ToString(),
            //        source = FrameworkConst.OperatorId,
            //        outId = OuterId.ToString()
            //    });
            //    if (!rest.success)
            //    {
            //        ShowAlert(false, "温馨提示", "获取支付二维码失败\r\n" + rest.msg);
            //        Exitloop = true;
            //        Preview(); //回退
            //        return;
            //    }
            //    Logger.Main.Info(
            //        $"病人[{ExtraPaymentModel.PatientInfo.Name} {ExtraPaymentModel.PatientInfo.PatientId}] 开始[{ExtraPaymentModel.CurrentPayMethod}]");
            //    订单扫码 = rest.data;

            //    var barQrCodeGenerater = GetInstance<IBarQrCodeGenerator>();
            //    QrCodeImage = barQrCodeGenerater.QrcodeGenerate(订单扫码.qrCode, Image.FromFile(ResourceEngine.GetResourceFullPath(PayLogoMapping[ExtraPaymentModel.CurrentPayMethod])));

            //    Looping = true;
            //    Procuding = false;
            //    Exitloop = false;
            //    Task.Factory.StartNew(AskingLoop); //创建成功则进行轮询
            //});
        }
    }
}
