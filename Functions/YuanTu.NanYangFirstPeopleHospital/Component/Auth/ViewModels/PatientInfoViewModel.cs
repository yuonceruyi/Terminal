using Prism.Regions;
using System;
using System.Linq;
using Prism.Commands;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Devices.CardReader;

namespace YuanTu.NanYangFirstPeopleHospital.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
       
        public PatientInfoViewModel(IMagCardDispenser[] magCardDispenser) : base(null)
        {
            _magCardDispenser = magCardDispenser.FirstOrDefault(p => p.DeviceId == "Act_F6_Mag");
            ConfirmCommand = new DelegateCommand(Confirm);
            UpdateCommand = new DelegateCommand(() =>
            {
                IsAuth = !Phone.IsNullOrWhiteSpace();
                ShowUpdatePhone = true;
            });
            UpdateCancelCommand = new DelegateCommand(() => { ShowUpdatePhone = false; });
            UpdateConfirmCommand = new DelegateCommand(UpdateConfirm);
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            if (ChoiceModel.Business == Business.建档 && CardModel.CardType == CardType.身份证)
            {

            }
            else
            {
                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];

                Phone = patientInfo.phone.Mask(3, 4);
                IdNo = patientInfo.idNo.Mask(3, 15);
            }
            CanUpdatePhone = ChoiceModel.Business == Business.建档;
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
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            ChangeNavigationContent($"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}");
            Next();
        }
        protected override bool GetNewCardNo()
        {
            try
            {
                if (FrameworkConst.DoubleClick)
                {
                    //CardModel.CardNo = "1234567890";
                    CardModel.CardNo = DateTimeCore.Now.ToString("HHmmssff");
                    return true;
                }

                if (!_magCardDispenser.Connect().IsSuccess)
                {
                    ReportService.发卡器离线(null, ErrorSolution.发卡器离线);
                    ShowAlert(false, "建档发卡", "发卡机连接失败");
                    return false;
                }
                if (!_magCardDispenser.Initialize().IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机初始化失败");
                    return false;
                }
                var result = _magCardDispenser.EnterCard(TrackRoad.Trace2);
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机读卡号失败");
                    return false;
                }
                //string cardNo;
                //if (!result.Value.TryGetValue(TrackRoad.Trace2, out cardNo))
                //{
                //    ShowAlert(false, "建档发卡", "发卡机读卡号失败");
                //    return false;
                //}
                CardModel.CardNo = result.Value[TrackRoad.Trace2];
                //CardModel.CardNo = BitConverter.ToUInt32(result.Value, 0).ToString();
                return true;
            }
            catch (Exception ex)
            {
                ShowAlert(false, "建档发卡", "发卡机读卡失败");
                Logger.Main.Error($"[建档发卡]{ex.Message + ex.StackTrace}");
                return false;
            }
        }
        protected override void PrintCard()
        {
             _magCardDispenser.MoveCardOut();
        }
        public override bool OnLeaving(NavigationContext navigationContext)
        {
            ShowMask(false);
            _magCardDispenser.UnInitialize();
            _magCardDispenser.DisConnect();
            return true;
        }
    }
}