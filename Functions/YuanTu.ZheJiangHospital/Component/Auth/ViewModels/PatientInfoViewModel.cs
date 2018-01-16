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
using YuanTu.ZheJiangHospital.Component.Auth.Models;
using YuanTu.ZheJiangHospital.Component.Recharge.Models;
using YuanTu.ZheJiangHospital.HIS;

namespace YuanTu.ZheJiangHospital.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser) : base(rfCardDispenser)
        {
        }

        [Dependency]
        public IAuthModel Auth { get; set; }
        [Dependency]
        public IAccountModel Account { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            TopBottom.InfoItems = null;

            ShowUpdatePhone = false;
            CanUpdatePhone = false;

            if (ChoiceModel.Business == Business.建档)
            {
                if (CardModel.CardType != CardType.身份证)
                {
                    var cardInfo = Auth.Res建档读卡;
                    string idNo = cardInfo.身份证号;

                    IdCardModel.Name = cardInfo.姓名;
                    IdCardModel.Sex = (int.Parse(cardInfo.性别) % 2 == 0) ? Consts.Enums.Sex.女 : Consts.Enums.Sex.男;
                    IdCardModel.Birthday = DateTime.Parse(cardInfo.生日);
                    IdCardModel.IdCardNo = idNo;
                    IdCardModel.Address = cardInfo.地址;
                }

                Name = IdCardModel.Name;
                Sex = IdCardModel.Sex.ToString();
                Birth = IdCardModel.Birthday.ToString("yyyy-MM-dd");
                Phone = null;
                IdNo = IdCardModel.IdCardNo.Mask(14, 3);
                IsAuth = false;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;
            }
            else
            {
                IsAuth = true;
                var patientInfo = Auth.Info;
                Name = patientInfo.NAME;
                Sex = patientInfo.SEX;
                Birth = patientInfo.BIRTHDAY?.ToString("yyyy-MM-dd");
                Phone = patientInfo.PHONE.Mask(3, 4);
                IdNo = patientInfo.IDNO.Mask(14, 3);
            }
        }

        public override void Confirm()
        {
            if (ChoiceModel.Business == Business.建档)
            {
                if (Phone.IsNullOrWhiteSpace())
                {
                    ShowUpdatePhone = true;
                    return;
                }
                CreatePatient();
                return;
            }
            var patientInfo = Auth.Info;
            if(Account.HasAccount && Account.Balance >= 0)
                ChangeNavigationContent($"{patientInfo.NAME}\n{Account.Balance.In元()}");
            else
                ChangeNavigationContent(patientInfo.NAME);

            var resource =ResourceEngine;
            TopBottom.InfoItems = new ObservableCollection<InfoItem>(new[]
            {
                new InfoItem
                {
                    Title = "姓名",
                    Value = patientInfo.NAME,
                    Icon = resource.GetImageResourceUri("姓名图标_YellowBlue")
                },
                new InfoItem()
                {
                    Title= "余额",
                    Value = Account.Balance.In元(),
                    Icon = resource.GetImageResourceUri("余额图标_YellowBlue")
                }, 
            });

            Next();
        }

        protected override void CreatePatient()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在建档，请稍候...");
                var req = new Req建档()
                {
                    姓名 = IdCardModel.Name,
                    性别 = ((int)IdCardModel.Sex).ToString(),
                    民族 = "1",
                    身份证号 = IdCardModel.IdCardNo,
                    地址 = IdCardModel.Address,
                    生日 = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                    电话 = Phone,
                };
                switch (CardModel.CardType)
                {
                    case CardType.身份证:
                        req.医保类别 = "1";
                        req.卡号 = IdCardModel.IdCardNo;
                        break;

                    case CardType.市医保卡:
                        req.医保类别 = "2";
                        req.卡号 = Auth.Res建档读卡.卡号;
                        break;

                    case CardType.省医保卡:
                        req.医保类别 = "3";
                        req.卡号 = Auth.Res建档读卡.卡号;
                        break;
                }
                var res = DataHandler.RunExe<Res建档>(req);
                if (!res.Success)
                {
                    ShowAlert(false, "建档", "建档失败", debugInfo: res.Message);
                    return;
                }
                ShowAlert(true, "建档", "建档成功");
                Next();
            });
        }
    }
}