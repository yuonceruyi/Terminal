using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Navigating;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Enums;
using YuanTu.Consts;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Gateway;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.ViewModels
{
    public class PrintViewModel : YuanTu.Default.Component.ViewModels.PrintViewModel
    {


        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }
        [Dependency]
        public IBillRecordModel BillRecordModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            // 阻止导航栏点击
            GetInstance<INavigationModel>().PreventClick = true;
            var choiceModel = GetInstance<IChoiceModel>();

            Success = PrintModel.Success;
            TypeMsg = PrintModel.PrintInfo.TypeMsg;
            TipMsg = PrintModel.PrintInfo.TipMsg;

            if (!Success)
            {
                TipMsg = PrintModel.PrintInfo.TipMsg + "\r\n详情:" + PrintModel.PrintInfo.DebugInfo;
            }

            var resource = ResourceEngine;

            Source = resource.GetImageResourceUri(PrintModel.PrintInfo.TipImage ?? (Success ? "提示_正确" : "提示_感叹号"));



            if (PrintModel.NeedPrint)
            {
                PlaySound(SoundMapping.取走卡片及凭条);
                PrintManager.Print();
            }
            else
            {
                PlaySound(SoundMapping.请取走卡片);
            }
            _doCommand = false;

            switch (choiceModel.Business)
            {
                case Business.建档:
                case Business.挂号:
                case Business.预约:
                case Business.取号:
                case Business.充值:
                case Business.住院押金:
                    break;
                case Business.缴费:
                    //如果有多个缴费单。。。。提示并跳转到选择缴费单的页面
                    if (this.BillRecordModel.Res获取缴费概要信息?.data?.Count > 1)
                    {
                        ShowAlert(true, "继续缴费", "还有待缴费的处方单" + (this.BillRecordModel.Res获取缴费概要信息?.data.Count - 1) + "张", 20);
                        this.BillRecordModel.Req获取缴费概要信息 = new req获取缴费概要信息
                        {
                            patientId = PatientModel.Res病人信息查询?.data[PatientModel.PatientInfoIndex].patientId,
                            cardType = ((int)CardModel.CardType).ToString(),
                            cardNo = CardModel.CardNo,
                            billType = ""
                        };
                        this.BillRecordModel.Res获取缴费概要信息 = DataHandlerEx.获取缴费概要信息(this.BillRecordModel.Req获取缴费概要信息);

                        Navigate(A.JF.BillRecord);
                    }
                    else
                    {
                    }
                    return;
            }
        }
    }
}