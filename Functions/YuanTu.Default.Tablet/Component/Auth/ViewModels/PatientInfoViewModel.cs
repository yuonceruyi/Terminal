using System;
using System.Collections.ObjectModel;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Models.UserCenter.Auth;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Models;
using YuanTu.Devices.CardReader;

namespace YuanTu.Default.Tablet.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser) : base(rfCardDispenser)
        {
        }

        [Dependency]
        public IAuthModel AuthModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            TopBottom.InfoItems = null;

            IsAuth = true;
            ShowUpdatePhone = false;
            CanUpdatePhone = false;
            var patientInfo = AuthModel.当前就诊人信息;
            Name = patientInfo.patientName;
            Sex = patientInfo.sex == 1 ? "男" : "女";
            Birth = GetBirthDay(patientInfo.idNo);
            Phone = patientInfo.phoneNum.Mask(3, 4);
            IdNo = patientInfo.idNo.Mask(14, 3);
            GuardIdNo = patientInfo.guarderIdNo.Mask(14, 3);
        }

        public override void Confirm()
        {
            var patientInfo = AuthModel.当前就诊人信息;
            ChangeNavigationContent($"{patientInfo.patientName}\r\n余额{Convert.ToDecimal(patientInfo.balance).In元()}");

            var resource = ResourceEngine;
            TopBottom.InfoItems = new ObservableCollection<InfoItem>(new[]
            {
                new InfoItem
                {
                    Title = "姓名",
                    Value = patientInfo.patientName,
                    Icon = resource.GetImageResourceUri("姓名图标_YellowBlue")
                },
                new InfoItem
                {
                    Title = "余额",
                    Value = Convert.ToDecimal(patientInfo.balance).In元(),
                    Icon = resource.GetImageResourceUri("余额图标_YellowBlue")
                }
            });

            Next();
        }

        protected virtual string GetBirthDay(string IdNo)
        {
            return IdNo == null ? null : $"{IdNo.SafeSubstring(6,4)}-{IdNo.SafeSubstring(10,2)}-{IdNo.SafeSubstring(12, 2)}";
        }

    }
}