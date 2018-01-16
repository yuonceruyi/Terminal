using System;
using System.Linq;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.YuHangSecondHospital.Component.Auth.Models;
using YuanTu.YuHangSecondHospital.NativeService;
using YuanTu.YuHangSecondHospital.NativeService.Dto;

namespace YuanTu.YuHangSecondHospital.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser, IMagCardDispenser[] magCardDispenser)
            : base(rfCardDispenser)
        {
            _magCardDispenser = magCardDispenser?.FirstOrDefault(p => p.DeviceId == "Act_F6_Mag");
        }

        protected override bool GetNewCardNo()
        {
            try
            {
                if (FrameworkConst.DoubleClick && FrameworkConst.VirtualHardWare)
                    return View.Dispatcher.Invoke(() =>
                    {
                        var ret = InputTextView.ShowDialogView("输入物理卡号");
                        if (ret.IsSuccess)
                        {
                            CardModel.CardNo = ret.Value;
                            return true;
                        }
                        return false;
                    });

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
                    //读卡失败时,回收卡片重新发卡
                    if (!_magCardDispenser.MoveCard(CardPosF6.吞入, "发卡机读卡号失败，故回收卡片").IsSuccess)
                    {
                        ShowAlert(false, "建档发卡", "发卡机读卡号失败后回收卡失败");
                        return false;
                    }
                    GetNewCardNo();
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

        protected override void PrintCard()
        {
            _magCardDispenser?.MoveCardOut();
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            if (CardModel.ExternalCardInfo == "社保_信息补全")
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
            else
            {
                base.OnEntered(navigationContext);

            }

        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            ShowMask(false);
            _magCardDispenser?.UnInitialize();
            _magCardDispenser?.DisConnect();
            return true;
        }

        public override void Confirm()
        {
            if (Phone.IsNullOrWhiteSpace())
            {
                ShowUpdatePhone = true;
                return;
            }
            if (CardModel.ExternalCardInfo == "社保_信息补全")
            {
                SiCardFillInfo();
            }
            else
            {
                base.Confirm();
            }
               
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
            if (CardModel.ExternalCardInfo == "社保_信息补全")
            {
                Phone = NewPhone;
                CreateModel.Phone = NewPhone;
                ShowUpdatePhone = false;
                return;
            }

            base.UpdateConfirm();
        }

        private void SiCardFillInfo()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在注册您的身份信息，请稍后...");
                var cm = CardModel as CardModel;
                var ret = LianZhongHisService.ExcuteHospitalAddArchive(new AddArchiveRequest()
                {
                    CardNo = CardModel.CardNo,
                    Phone = NewPhone,
                    HomeAddress = IdCardModel.Address,
                    IdNumber = IdCardModel.IdCardNo,
                    HealthCareCardContent = cm.Res读接触卡号.卡号识别码
                });
                if (!ret.IsSuccess)
                {
                    ShowAlert(false,"注册失败","你的身份信息注册失败，请到窗口处理！",debugInfo:ret.Message);
                    return;
                }
                PatientModel.Req病人信息查询 = new req病人信息查询
                {
                    Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                    cardNo = CardModel.CardNo,
                    cardType = ((int)CardModel.CardType).ToString()
                };
                PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
                if (!PatientModel.Res病人信息查询.success)
                {
                    ShowAlert(false,"注册失败", "你的身份信息注册失败，\r\n请到窗口处理！");
                    Navigate(A.Home);
                    return;
                }
                var patientInfo = PatientModel.当前病人信息;
                ChangeNavigationContent($"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}");
                Next();
            });
        }
    }
}