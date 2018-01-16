using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models.UserCenter.Auth;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Default.House.HealthManager;
using YuanTu.Devices.CardReader;

namespace YuanTu.QingDao.House.Component.Auth
{
    public class PatientInfoViewModel : Default.House.Component.Auth.ViewModels.PatientInfoViewModel
    {
        [Dependency]
        public IAuthModel AuthModel { get; set; }

        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser) : base(rfCardDispenser)
        {
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            if (ChoiceModel.Business == Business.建档)
            {
                HideNavigating = false;
                Name = IdCardModel.Name;
                Sex = IdCardModel.Sex.ToString();
                Birth = IdCardModel.Birthday.ToString("yyyy-MM-dd");
                Phone = null;
                IdNo = IdCardModel.IdCardNo.Mask(14, 3);
                IsAuth = false;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;
            }
            else if (CardModel.CardType==CardType.身份证)
            {
                HideNavigating = true;
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
            else
            {
               

                HideNavigating = true;
                IsAuth = true;
                ShowUpdatePhone = false;
                CanUpdatePhone = ChoiceModel.Business == Business.健康服务 || ChoiceModel.Business == Business.体测查询;
                var patientInfo = AuthModel.当前就诊人信息;

                Name = patientInfo?.patientName;
                Sex = patientInfo?.sex == 1 ? "男" : "女";
                Birth = GetBirthDay(patientInfo?.idNo);
                Phone = patientInfo?.phoneNum.Mask(3, 4);
                IdNo = patientInfo?.idNo.Mask(14, 3);
            }
            PlaySound(SoundMapping.个人信息);
        }
        protected virtual string GetBirthDay(string idNo)
        {
            return idNo == null ? null : $"{idNo.SafeSubstring(6, 4)}-{idNo.SafeSubstring(10, 2)}-{idNo.SafeSubstring(12, 2)}";
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
                lp.ChangeText("正在更新健康档案个人信息，请稍候...");

                var req = new req修改手机号
                {
                    healthUserId = HealthModel.Res查询是否已建档?.data?.id,
                    idNo = HealthModel.Res查询是否已建档?.data?.idNo,
                    phone = NewPhone
                };
                var res = HealthDataHandlerEx.修改手机号(req);
                if (res?.success ?? false)
                {
                    ShowUpdatePhone = false;
                    if (HealthModel.Res查询是否已建档?.data != null)
                        HealthModel.Res查询是否已建档.data.phone = NewPhone;
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