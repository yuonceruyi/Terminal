using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Systems;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.YiWuArea.Dialog;
using YuanTu.YiWuArea.Insurance;
using YuanTu.YiWuArea.Insurance.Models;
using YuanTu.YiWuArea.Insurance.Models.Base;
using YuanTu.YiWuFuBao.Models;

namespace YuanTu.YiWuFuBao.Component.Auth.ViewModels
{
    public class SiCardViewModel: YuanTu.Default.Component.Auth.ViewModels.SiCardViewModel
    {
        public ICommand CancelPwdCommand { get; set; }
        public ICommand ConfirmPwdCommand { get; set; }
        public SiCardViewModel(IIcCardReader[] icCardReaders) : base(icCardReaders)
        {
            CancelPwdCommand=new DelegateCommand(CancelPwd);
            ConfirmPwdCommand=new DelegateCommand(ConfirmPwd);
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            ShowMask(false);
            return base.OnLeaving(navigationContext);

        }

        public override void Comfirm()
        {
            //var handle = YiWuNativeProvider.ic_init(100, 115200);
            //StringBuilder no = new StringBuilder(1024);
            //YiWuNativeProvider.readcardid(handle, '1', no);
            //var rest = YiWuNativeProvider.Read();
            //Logger.Main.Info($"[社保读卡]社保卡号:{no}");
            DoCommand(ctx =>
            {
                ctx.ChangeText("正在读卡，请稍后...");
                SipayHandler.Uninit();
                var ybInfoRest = GetInfoFromSi();//YiWuNativeProvider.Read();
                if (!ybInfoRest.IsSuccess||!ybInfoRest.Value.IsSuccess)
                {
                    ShowAlert(false,"读卡失败",ybInfoRest.Message??ybInfoRest.Value?.错误信息,debugInfo: ybInfoRest.Exception?.Message);
                    return;
                }


                var ptInfoRest = PatientIcData.Deserialize(ybInfoRest.Value.写卡后IC卡数据);
                if (!ptInfoRest.IsSuccess)
                {
                    ShowAlert(false,"社保读卡失败",ptInfoRest.Message);
                    return;
                }
                ctx.ChangeText("正在获取病人信息，请稍后...");
                var patientInfo = ptInfoRest.Value;
                var cardNo = patientInfo.医疗证号;
                PatientModel.Req病人信息查询 = new req病人信息查询
                {
                    cardNo = cardNo,
                    cardType = ((int)CardModel.CardType).ToString()
                };
                PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

                if (PatientModel.Res病人信息查询.success)
                {
                    if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                    {
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                       
                        return;
                    }
                    CardModel.CardNo = cardNo;
                    CardModel.ExternalCardInfo = "病人信息";
                    Next();
                }
                else
                {
                    CardModel.CardNo = cardNo;
                    CardModel.ExternalCardInfo = "建档";

                    IdCardModel.IdCardNo = patientInfo.公民身份号;
                    IdCardModel.Name = patientInfo.姓名;
                    IdCardModel.Sex = Convert.ToInt32(IdCardModel.IdCardNo[16]) % 2 == 0 ? Sex.女 : Sex.男;
                    IdCardModel.Birthday = DateTime.ParseExact(IdCardModel.IdCardNo.Substring(6, 8), "yyyyMMdd", null,
                        DateTimeStyles.None);
                    Next();
                   // ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                   
                }
            });
        }

        private Result<Res获取参保人员信息> GetInfoFromSi()
        {
            try
            {
                var cm = CardModel as CardModel;
                if (FrameworkConst.Local)
                {
                    var res = InsuranceResponseBase.BuildResponse<Res获取参保人员信息>(
                        "$$0~~~~C09AF9C81           6458086             20 12  0330621198708141528方利萍                                  2  1  1987.08.1422 义乌市妇幼保健计划生育服务中心（义乌市妇000000647.27000001172.32000000000000000002553.57000000000000000000000000000000000000000000000000000000000000000~000000001000000~~7000$$");
                    return Result<Res获取参保人员信息>.Success(cm.参保人员信息 = res);
                }
                if (CardModel.CardType == CardType.社保卡)
                {
                    SiPassword = string.Empty;
                    YiWuSiPasswordProvider.StartMonitor(() =>
                    {
                        StartTimer();
                        View.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
                            (Action) (() =>
                            {
                                var sm = GetInstance<IShellViewModel>();
                                sm.Busy.IsBusy = false;
                                ShowMask(true, new SiPasswordDialog() {DataContext = this});
                            }));
                    });
                }

                var fullYbInfo = SipayHandler.调用获取参保人员信息(new Req获取参保人员信息() {读卡方式 = "10"});
               
                cm.参保人员信息 = fullYbInfo;
                if (fullYbInfo.IsSuccess)
                {
                    cm.SiPassword = SiPassword;
                    return Result<Res获取参保人员信息>.Success(fullYbInfo);
                }
                var errMsg = fullYbInfo?.错误信息 ?? "";
                var niceMsg = "";
                if (errMsg.Contains("卡上电复位失败"))
                {
                    niceMsg = "您的社保卡没有正确插入，请按图片提示插入社保卡。";
                }
                return Result<Res获取参保人员信息>.Fail(niceMsg, new Exception(errMsg));
                // 
            }
            catch (Exception ex)
            {
                Logger.Device.Error($"[社保读卡] 读卡报错 {ex.Message}");
                return Result<Res获取参保人员信息>.Fail(ex.Message);
            }
            finally
            {
                if (CardModel.CardType == CardType.社保卡)
                {

                    YiWuSiPasswordProvider.StopMonitor();
                    ShowMask(false);
                }
            }
        }

        private string _siPassword;

        public string SiPassword
        {
            get { return _siPassword; }
            set { _siPassword = value;
                OnPropertyChanged(); }
        }
        
        

        private void CancelPwd()
        {
            var winPtr = WindowHelper.FindWindow(null, YiWuSiPasswordProvider.PasswordWinTitle);
            var btn = WindowHelper.FindWindowEx(winPtr, IntPtr.Zero, "Button", "取消");
            if (btn != IntPtr.Zero)
            {
                WindowHelper.PostMessage(btn, (uint)(WindowHelper.WindowMessage.CLICK), IntPtr.Zero, IntPtr.Zero);
            }
            else
            {
                WindowHelper.PostMessage(winPtr, (uint)(WindowHelper.WindowMessage.CLOSE), IntPtr.Zero,
                           IntPtr.Zero);
            }
            ShowMask(false);
            Preview();
        }

        private void ConfirmPwd()
        {
            if (string.IsNullOrEmpty(SiPassword))
            {
                ShowAlert(false,"社保密码","请输入您的社保密码!");
                return;
            }
            StartTimer();
            if (CardModel.CardType == CardType.社保卡)
            {
                var sm = GetInstance<IShellViewModel>();
                sm.Busy.IsBusy = true;
            }
            var winPtr = WindowHelper.FindWindow(null, YiWuSiPasswordProvider.PasswordWinTitle);
            var inputtext = WindowHelper.FindWindowEx(winPtr, IntPtr.Zero, "Edit", null);
            foreach (var pchar in SiPassword)
            {
                WindowHelper.SendKey(inputtext, pchar);
            }
            Thread.Sleep(100);
            var btn = WindowHelper.FindWindowEx(winPtr, IntPtr.Zero, "Button", "确认");
            if (btn != IntPtr.Zero)
            {
                WindowHelper.PostMessage(btn, (uint) (WindowHelper.WindowMessage.CLICK), IntPtr.Zero, IntPtr.Zero);
            }
            else
            {
                WindowHelper.SendKey(inputtext, 0x0d); //回车

            }
            ShowMask(false);
        }

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入医保卡号");
            if (!ret.IsSuccess)
            {
                return;
            }
            DoCommand(ctx =>
            {
                var ybInfo = new YiBaoCardContent()
                {
                    CardNumber = ret.Value,
                    IDNumber = "220300197611154252",
                    Name = "江毅",
                    Sex = "1",
                    Birthday= "0D0419911031"
                };
                var cardNo = ybInfo.CardNumber.Trim();
                PatientModel.Req病人信息查询 = new req病人信息查询
                {
                    cardNo = cardNo,
                    cardType = ((int)CardModel.CardType).ToString()
                };
                PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

                if (PatientModel.Res病人信息查询.success)
                {
                    if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                    {
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");

                        return;
                    }
                    CardModel.CardNo = cardNo;
                    CardModel.ExternalCardInfo = "病人信息";
                    Next();
                }
                else
                {
                    CardModel.CardNo = cardNo;
                    CardModel.ExternalCardInfo = "建档";

                    IdCardModel.IdCardNo = ybInfo.IDNumber;
                    IdCardModel.Name = ybInfo.Name;
                    IdCardModel.Sex = Convert.ToInt32(ybInfo.IDNumber[16]) % 2 == 0 ? Sex.女 : Sex.男;
                    IdCardModel.Birthday = DateTime.ParseExact(ybInfo.IDNumber.Substring(6, 8), "yyyyMMdd", null,
                        DateTimeStyles.None);
                    Next();
                   // ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);

                }
            });
        }
    }
}
