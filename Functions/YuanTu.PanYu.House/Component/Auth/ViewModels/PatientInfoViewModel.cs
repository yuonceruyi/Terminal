using System;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Core.Extension;
using YuanTu.Default.House.HealthManager;
using YuanTu.Devices.CardReader;
using YuanTu.PanYu.House.CardReader;
using YuanTu.PanYu.House.PanYuService;

namespace YuanTu.PanYu.House.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : Default.House.Component.Auth.ViewModels.PatientInfoViewModel
    {
        [Dependency]
        public ICardService CardService { get; set; }

        [Dependency]
        public IMscService MscService { get; set; }

        [Dependency]
        public new IPatientModel PatientModel { get; set; }

     

        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser) : base(rfCardDispenser)
        {

        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            HideNavigating = true;
            ChangeNavigationContent("");
            if (ChoiceModel.Business == Business.建档 && CardModel.CardType == Consts.Enums.CardType.身份证)
            {
                Name = IdCardModel.Name;
                Sex = IdCardModel.Sex.ToString();
                Birth = IdCardModel.Birthday.ToString("yyyy-MM-dd");
                Phone = null;
                IdNo = IdCardModel.IdCardNo.Mask(14, 3);
                IsAuth = false;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;
            }
            else if (ChoiceModel.Business == Business.建档 && CardModel.CardType == Consts.Enums.CardType.社保卡)
            {
                //todo 社保卡建档处理
            }
            else
            {
                IsAuth = true;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;

                var patientInfo = HealthModel.Res查询是否已建档?.data;

                Name = patientInfo?.name;
                Sex = patientInfo?.sex;
                Birth = patientInfo?.birthday;
                Phone = patientInfo?.phone.Mask(3, 4);
                IdNo = patientInfo?.idNo.Mask(14, 3);
               
            }
        }

        public override void Confirm()
        {
            if (Phone.IsNullOrWhiteSpace())
            {
                ShowUpdatePhone = true;
                return;
            }
            if (ChoiceModel.Business == Business.建档)
            {
                switch (CreateModel.CreateType)
                {
                    case CreateType.成人:
                        CreatePatient();
                        break;
                    case CreateType.儿童:
                        Navigate(A.CK.InfoEx);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return;
            }
            ChangeNavigationContent($"{PatientModel.Name}");
            Next();
        }
        public override void UpdateConfirm()
        {
            if (string.IsNullOrWhiteSpace(NewPhone))
            {
                ShowAlert(false, "温馨提示", "请输入手机号");
                return;
            }
            if (!NewPhone.IsHandset())
            {
                ShowAlert(false, "温馨提示", "请输入正确的手机号");
                return;
            }
            if (ChoiceModel.Business == Business.建档)
            {
                Phone = NewPhone;
                CreateModel.Phone = NewPhone;
                ShowUpdatePhone = false;
                return;
            }

            //todo Update
            DoCommand(lp =>
            {
                if(CardModel.CardType==Consts.Enums.CardType.就诊卡)
                {
                    lp.ChangeText("正在更新民生卡个人信息，请稍候...");
                    MscService.CardNo = PatientModel.CardNo;
                    MscService.CardUid = CardService.CardInfo?.SeqNo;
                    MscService.PhoneNum = NewPhone;
                    MscService.CardSeq = CardService.CardInfo?.CardSeq;
                    var result = MscService.民生卡客户信息更新();
                    if (result.IsSuccess)
                    {
                        ShowUpdatePhone = false;
                        PatientModel.Phone = NewPhone;
                        Phone = NewPhone.Mask(3, 4);
                        ShowAlert(true, "个人信息", "民生卡客户信息更新成功");
                    }
                    else
                    {
                        ShowAlert(false, "个人信息", "民生卡客户信息更新失败");
                    }
                }
               
               

                lp.ChangeText("正在更新健康档案个人信息，请稍候...");

                var req = new req修改手机号
                {
                    healthUserId=HealthModel.Res查询是否已建档?.data?.id,
                    idNo=HealthModel.Res查询是否已建档?.data?.idNo,
                    phone=NewPhone
                };
                var res = HealthDataHandlerEx.修改手机号(req);
                if (res?.success??false)
                {
                    ShowUpdatePhone = false;
                    Phone = NewPhone.Mask(3, 4);
                    ShowAlert(true, "个人信息", "健康档案个人信息更新成功");
                }
                else
                {
                    ShowAlert(false, "个人信息", "健康档案个人信息更新失败");
                }
            });
        }
       
    }
}