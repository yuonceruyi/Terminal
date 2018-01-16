using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Core.FrameworkBase.Loadings;
using System.Threading;
using YuanTu.Core.Log;
using YuanTu.Consts.Gateway;
using YuanTu.Consts;
using YuanTu.Consts.Models.Register;
using System.Runtime.CompilerServices;
using YuanTu.Consts.Models;

namespace YuanTu.ChongQingArea.Component.Tools.ViewModels
{
    public class ScanQrCodeViewModel:YuanTu.Default.Component.Tools.ViewModels.ScanQrCodeViewModel
    {
        private int _remainHeight;

        public int RemainHeight
        {
            get { return _remainHeight; }
            set { _remainHeight = value; OnPropertyChanged();}
        }

        [Microsoft.Practices.Unity.Dependency]
        public IRegisterModel RegisterModel { get; set; }

        [Microsoft.Practices.Unity.Dependency]
        public IScheduleModel ScheduleModel { get; set; }

        [Microsoft.Practices.Unity.Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            OptDic[Business.补卡] = "8";
            if (ExtraPaymentModel.CurrentBusiness==Business.补卡)
            {
                RemainHeight = 0;
            }
            else
            {
                RemainHeight = 66;
            }
        }

        protected override bool NeedLeaving(LoadingProcesser lp, NavigationContext navigationContext)
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

                    if (ChoiceModel.Business == Business.挂号)
                    {
                        lp.ChangeText("正在解除锁号,请稍后......");
                        var scheduleInfo = ScheduleModel.所选排班;
                        var lock01 = new req挂号解锁
                        {
                            operId = FrameworkConst.OperatorId,
                            medDate = scheduleInfo.medDate,
                            scheduleId = scheduleInfo.scheduleId,
                            lockId = RegisterModel.Res挂号锁号?.data?.lockId,
                        };
                        RegisterModel.Res挂号解锁 = DataHandlerEx.挂号解锁(lock01);
                        Logger.Main.Info("挂号解锁:" + (!RegisterModel.Res挂号解锁.success ? $"失败" : "成功"));
                    }

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
    }
}
