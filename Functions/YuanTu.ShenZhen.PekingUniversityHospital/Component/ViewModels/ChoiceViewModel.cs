using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;
using YuanTu.Devices.PrinterCheck;
using YuanTu.Devices.PrinterCheck.CePrinterCheck;
using YuanTu.Devices.PrinterCheck.MsPrinterCheck;
using YuanTu.Default;

namespace YuanTu.ShenZhen.PekingUniversityHospital.Component.ViewModels
{
    public class ChoiceViewModel : YuanTu.Default.Component.ViewModels.ChoiceViewModel
    {
      
        protected override Task<Result<FormContext>> BillPayJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 结算");
                var billRecordModel = GetInstance<IBillRecordModel>();
                var patientModel = GetInstance<IPatientModel>();
                var cardModel = GetInstance<ICardModel>();
                lp.ChangeText("正在查询待缴费信息，请稍候...");
                billRecordModel.Req获取缴费概要信息 = new req获取缴费概要信息
                {
                    patientId = patientModel.Res病人信息查询?.data[patientModel.PatientInfoIndex].patientId,
                    cardType = ((int)cardModel.CardType).ToString(),
                    cardNo = cardModel.CardNo,
                    billType = "",
                    patientName= patientModel.Res病人信息查询?.data[patientModel.PatientInfoIndex].name,
                };
                billRecordModel.Res获取缴费概要信息 = DataHandlerEx.获取缴费概要信息(billRecordModel.Req获取缴费概要信息);
                if (billRecordModel.Res获取缴费概要信息?.success ?? false)
                {
                    if (billRecordModel.Res获取缴费概要信息?.data?.Count > 0)
                    {
                        return Result<FormContext>.Success(default(FormContext));
                    }
                    ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息，\n您没有待缴费的处方。");
                    Navigate(A.Home);
                    return Result<FormContext>.Fail("");
                }
                ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息", debugInfo: billRecordModel.Res获取缴费概要信息?.msg);
                return Result<FormContext>.Fail("");
            });
        }
    }
}