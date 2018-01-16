using System;
using System.Collections.ObjectModel;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Models;
using YuanTu.Devices.CardReader;
using YuanTu.XiaoShanHealthStation.Component.Auth.Models;

namespace YuanTu.XiaoShanHealthStation.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser) : base(rfCardDispenser)
        {
        }
        [Dependency]
        public IChaKaModel ChaKaModel { get; set; }
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            TopBottom.InfoItems = null;
            IsAuth = true;
            ShowUpdatePhone = false;
            CanUpdatePhone = false;
            var patientInfo = ChaKaModel.查询建档Out;
            Name = patientInfo.病人姓名;
            Sex = patientInfo.病人性别=="1"?"男":"女";
            Birth = patientInfo.出生日期;
            //Phone = patientInfo..Mask(3, 4);
            IdNo = patientInfo.身份证号.Mask(14, 3);
            //GuardIdNo = patientInfo.guardianNo.Mask(14, 3);
        }
        public override void Confirm()
        {
            var patientInfo = ChaKaModel.查询建档Out;
            ChangeNavigationContent($"{patientInfo.病人姓名}\r\n余额{patientInfo.市民卡余额}");

            var resource =ResourceEngine;
            TopBottom.InfoItems = new ObservableCollection<InfoItem>(new[]
            {
                new InfoItem
                {
                    Title = "姓名",
                    Value = patientInfo.病人姓名,
                    Icon = resource.GetImageResourceUri("姓名图标_YellowBlue")
                },
                new InfoItem
                {
                    Title = "余额",
                    Value = patientInfo.市民卡余额,
                    Icon = resource.GetImageResourceUri("余额图标_YellowBlue")
                }
            });

            if (!ChaKaModel.HasAccount || !ChaKaModel.HasSmartHealth)
            {
                ShowAlert(false,"温馨提示",$"您的智慧医疗账户尚未开通\n\n点击\"确定\"至主界面\n\n点击\"自助充值查询\"\n\n选择\"功能开通\"进行开通");
                return;
            }
            Next();
        }
    }
}