using System;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Devices.FingerPrint;
using YuanTu.BJJingDuETYY.Component.Auth.ViewModels;

namespace YuanTu.BJJingDuETYY.Component.Biometric.ViewModels
{
    internal class FingerPrintViewModel : FingerPrintViewModelBase
    {
        public FingerPrintViewModel(IFingerPrintDevice[] fingerPrintDevices) : base(fingerPrintDevices)
        {
        }

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        protected override void Act(LoadingProcesser lp, string imageData)
        {
            var info = PatientModel.当前病人信息;
            var req = new req病人基本信息修改()
            {
                Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),
                patientId = info.patientId,
                platformId = info.platformId,
                biologicData = imageData,
                biologicType = "23",

                address = info.address,
                birthday = info.birthday,
                guardianNo = info.guardianNo,
                name = info.name,
                idNo = info.idNo,
                phone = info.phone,
                sex = info.sex,
            };
            PatientModel.Req病人基本信息修改 = req;
            var res = DataHandlerEx.病人基本信息修改(req);
            PatientModel.Res病人基本信息修改 = res;
            if (!res.success)
            {
                ShowAlert(false, "指纹信息录入", $"录入失败:{res.msg}", debugInfo: res.msg);
                return;
            }
            ShowAlert(false, "指纹信息录入", $"录入成功", debugInfo: res.msg);
            Next();
        }
    }
}