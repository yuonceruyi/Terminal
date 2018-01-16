using Microsoft.Practices.Unity;
using YuanTu.Consts.Models.Auth;
using YuanTu.PanYu.House.PanYuGateway;
using YuanTu.PanYu.House.PanYuService;

namespace YuanTu.PanYu.House.Component.Register.ViewModels
{
    public class ChoiceHospitalViewModel:Default.House.Component.Register.ViewModels.ChoiceHospitalViewModel
    {
        [Dependency]
        public IHisService HisService { get; set; }
        [Dependency]
        public IPatientModel PatientModel { get; set; }
        [Dependency]
        public ICardModel CardModel { get; set; }
        protected override void Confirm(HospitalButtonInfo param)
        {
            DataHandler.HospitalId = param.HospitalId;
            //TODO 查询医院病人信息
            QueryPatientInfo();
        }
      

        public virtual void QueryPatientInfo()
        {
            DoCommand(p =>
            {
                p.ChangeText("正在查询病人信息，请稍候...");
                HisService.CardNo = PatientModel.CardNo;
                HisService.CardType = CardModel.CardType == Consts.Enums.CardType.就诊卡
                    ? CardType.MagCard
                    : CardType.IDCard;
                var result = HisService.病人信息查询();
                if (!result.IsSuccess)
                {
                    if (!HisService.NeedCreat)
                    {
                        ShowAlert(false,"温馨提示","病人查询失败");
                        return;
                    }
                    //todo 建档
                    HisService.Name = PatientModel.Name;
                    HisService.Gender = PatientModel.Gender;
                    HisService.Birthday = PatientModel.Birthday;
                    HisService.IDNo = PatientModel.IDNo;
                    HisService.Nation = PatientModel.Nation;
                    HisService.Address = PatientModel.Address;
                    HisService.Phone = PatientModel.Phone;
                    p.ChangeText("正在医院建档，请稍候...");
                    result = HisService.病人建档发卡();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", "医院建档失败");
                        return;
                    }
                    HisService.PatientId = HisService.Res病人建档发卡?.data?.patientId;
                    Next();
                }
                HisService.PatientId = HisService.Res病人信息查询?.data[0]?.patientId;
                Next();
            });
        }
    }
}
