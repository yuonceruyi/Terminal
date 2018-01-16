using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Commands;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Devices.CardReader;

namespace YuanTu.NanYangFirstPeopleHospital.Component.Auth.ViewModels
{
    public class PatientInfoExViewModel : YuanTu.Default.Component.Auth.ViewModels.PatientInfoExViewModel
    {
        public PatientInfoExViewModel(IMagCardDispenser[] magCardDispenser) : base(null)
        {
            _magCardDispenser = magCardDispenser.FirstOrDefault(p => p.DeviceId == "Act_F6_Mag");
            ConfirmCommand = new DelegateCommand(Confirm);
            ModifyNameCommand = new DelegateCommand(ModifyNameCmd);
        }

        protected override bool GetNewCardNo()
        {
            try
            {
                if (FrameworkConst.DoubleClick)
                {
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
                var result = _magCardDispenser.ReadTrack(TrackRoad.Trace2);
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机读卡号失败");
                    return false;
                }
                CardModel.CardNo = result.Value[TrackRoad.Trace2];
                return true;
            }
            catch (Exception ex)
            {
                ShowAlert(false, "建档发卡", "发卡机读卡失败");
                Logger.Main.Error($"[建档发卡]{ex.Message + ex.StackTrace}");
                return false;
            }
        }

        protected override void CreatePatient()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在准备就诊卡，请稍候...");
                //todo 发卡机发卡
                if (!GetNewCardNo()) return;

                CreateModel.Req病人建档发卡 = new req病人建档发卡
                {
                    operId = FrameworkConst.OperatorId,
                    cardNo = CardModel.CardNo,
                    cardType = "2",
                    name = Name.Trim(),
                    sex = IsBoy ? Sex.男.ToString() : Sex.女.ToString(),
                    birthday = DateTime.ToString("yyyy-MM-dd"),
                    idNo = IdCardModel.IdCardNo,
                    idType = "1", //测试必传
                    nation = IdCardModel.Nation,
                    address = IdCardModel.Address,
                    phone = CreateModel.Phone,
#pragma warning disable 612
                    guardianName = IdCardModel.Name,
                    guardianNo = IdCardModel.IdCardNo,
                    school = null,
#pragma warning restore 612
                    pwd = "123456",
                    tradeMode = "CA",
                    cash = null,
                    accountNo = null,
                    patientType = null,
                    //bankCardNo = pos?.CardNo,
                    //bankTime = pos?.TransTime,
                    //bankDate = pos?.TransDate,
                    //posTransNo = pos?.Trace,
                    //bankTransNo = pos?.Ref,
                    //deviceInfo = pos?.TId,
                    //sellerAccountNo = pos?.MId,
                    //setupType = ChaKa.GrardId ? "2" : "1",
                    setupType = ((int)CreateModel.CreateType).ToString()
                };
                lp.ChangeText("正在建档，请稍候...");
                CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);
                if (CreateModel.Res病人建档发卡?.success ?? false)
                {
                    lp.ChangeText("正在发卡，请及时取卡。");
                    if (!FrameworkConst.DoubleClick)
                        _magCardDispenser.MoveCardOut();
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "建档发卡成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分建档",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = CreatePrintables(),
                        TipImage = "提示_凭条和发卡"
                    });

                    ChangeNavigationContent($"{Name}\r\n卡号{CardModel.CardNo}");
                    Next();
                }
                else
                {
                    ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
                }
            });
        }
    }
}