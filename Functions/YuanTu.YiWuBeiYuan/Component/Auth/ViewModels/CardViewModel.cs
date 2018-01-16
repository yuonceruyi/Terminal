using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Core.Reporter;
using YuanTu.Core.Services.LightBar;
using YuanTu.Core.Systems;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.YiWuArea.Dialog;
using YuanTu.YiWuArea.Insurance;
using YuanTu.YiWuArea.Insurance.Models;
using YuanTu.YiWuArea.Insurance.Models.Base;
using YuanTu.YiWuBeiYuan.Component.Auth.Dialog;
using YuanTu.YiWuBeiYuan.Models;

namespace YuanTu.YiWuBeiYuan.Component.Auth.ViewModels
{
    public class CardViewModel : YuanTu.Default.Component.Auth.ViewModels.CardViewModel
    {
        private string _hospitalCardNo;
        private bool _hospitalInputFocus;
        private string _siPassword;

        [Dependency]
        public IIdCardModel IdCardModel { get; set; }

        private readonly IRFCpuCardReader _rfCpuCardReader;
        public CardViewModel(IRFCpuCardReader[] rfCpuCardReader) : base(null, null)
        {
            _rfCpuCardReader = rfCpuCardReader?.FirstOrDefault(p => p.DeviceId == "ACT_A630_RFIC");

            ShowInputMaskCommand = new DelegateCommand(ShowInputMask);
            CancelHospitalCardNoCommand = new DelegateCommand(CancelHospitalCardNo);
            ConfirmHospitalCardNoCommand = new DelegateCommand(ConfirmHospitalCardNo);

            CancelPwdCommand = new DelegateCommand(CancelPwd);
            ConfirmPwdCommand = new DelegateCommand(ConfirmPwd);

            InfoFixCommand=new DelegateCommand<string>(InfoFix);
        }

        public Visibility ShowSiCardAnimation
        {
            get { return _showSiCardAnimation; }
            set { _showSiCardAnimation = value;OnPropertyChanged(); }
        }

        public Visibility ShowBarCodeCardAnimation
        {
            get { return _showBarCodeCardAnimation; }
            set { _showBarCodeCardAnimation = value;OnPropertyChanged(); }
        }

        public Uri JiuZhenCard
        {
            get { return _jiuZhenCard; }
            set
            {
                _jiuZhenCard = value;
                OnPropertyChanged();
            }
        }
        private ConfirmExModel confirmExModel = null;
        public override void OnSet()
        {
            base.OnSet();
            CardUri = ResourceEngine.GetImageResourceUri("卡_社保卡");
            JiuZhenCard= ResourceEngine.GetImageResourceUri("卡_条码卡");
            View.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() =>
            {
                confirmExModel = ConfirmExModel.Build("好的，我要全自费", "不好，人工窗口试试");

            }));
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            _siPwdShowing = false;
            YiWuSiPasswordProvider.StartMonitor(ShowSiPasswordCallback); //用于爆掉错误框/弹出密码输入框
            base.OnEntered(navigationContext);
            ShowSiCardAnimation = Visibility.Visible;
            ShowBarCodeCardAnimation = Visibility.Visible;
            //HospitalInputFocus = false;
            HospitalInputFocus = true;
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            YiWuSiPasswordProvider.StopMonitor();
            ShowMask(false);
            return base.OnLeaving(navigationContext);

        }

        protected override void StartRead()
        {
            Task.Run(() =>
            {
                try
                {
                    SipayHandler.Uninit();
                    HospitalInputFocus = false;
                    HospitalInputFocus = true;
                    var ret = _rfCpuCardReader.Connect();
                    if (!ret.IsSuccess)
                    {
                        ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                        ShowAlert(false, "友好提示", $"读卡器打开失败({ret.ResultCode})", debugInfo: ret.Message);
                        return;
                    }
                    _rfCpuCardReader.MoveCard(CardPos.不持卡位); //退卡
                    if (!_rfCpuCardReader.Initialize().IsSuccess)
                    {
                        ShowAlert(false, "友好提示", $"读卡器初始化失败({ret.ResultCode})", debugInfo: ret.Message);
                        return;
                    }
                    _working = true;
                    while (_working)
                    {
                        var pos = _rfCpuCardReader.GetCardPosition();
                        if (pos.IsSuccess && (pos.Value == CardPos.停卡位 || pos.Value == CardPos.IC位))
                        {
                            //有卡，启动判卡流程
                            _working = false;
                            StartAdjustCardType();
                            break;
                        }
                        Thread.Sleep(300);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Main.Error($"[读卡异常]{ex}");
                }
            });
        }

        //读卡器内有卡后，启动判卡流程
        private void StartAdjustCardType()
        {
            _rfCpuCardReader.MoveCard(CardPos.IC位); //移动到IC卡位
            Thread.Sleep(200); //等卡停稳
            var redRet = YiWuNativeProvider.ReadId();
            Logger.Main.Info($"[读卡]尝试获取社保卡号 结果:{redRet.IsSuccess} 卡号:{redRet.Value} 错误消息:{redRet.Message}");
            if (redRet.IsSuccess&&!redRet.Value.IsNullOrWhiteSpace()) //确定为社保卡，进入社保读卡流程
            {
                StartReadInsuranceCard(redRet.Value);
            }
            else
            {
                //临时屏蔽平台卡
                //StartReadAreaCard();
                ShowAlert(false, "读卡失败", "未能识别您的卡片，请确认是否正确插入！", extend: new AlertExModel()
                {
                    HideCallback = tp =>
                    {
                        if (tp == AlertHideType.ButtonClick)
                        {
                            if (NavigationEngine.State == A.CK.Card) //确定还在当前页面
                            {
                                StartRead();//出错，那就继续读卡
                            }
                        }
                    }
                });
            }
        }

        //阅读区域平台卡
        private void StartReadAreaCard()
        {
            DoCommand(lp =>
            {
                _rfCpuCardReader.MoveCard(CardPos.停卡位); //移动到射频卡位
                var rest = _rfCpuCardReader.GetCardId();
                if (!rest.IsSuccess)
                {
                    ShowAlert(false, "友好提示", $"读卡器获取卡序列号失败", debugInfo: rest.Message);
                    return Result<string>.Fail("");
                }
                var cardid = BitConverter.ToString(rest.Value).Replace("-", "");
                Logger.Main.Info($"[读取卡序列号成功][cardNo]{cardid}");

                var appOrder = new byte[]{0x00, 0xA4, 0x04, 0x00, 0x09, 0xA0, 0x00, 0x00, 0x00, 0x03, 0x86, 0x98, 0x07, 0x01};
                rest = _rfCpuCardReader.CpuTransmit(appOrder);
                if (!rest.IsSuccess)
                {
                    ShowAlert(false, "友好提示", $"读卡器指令发送失败", debugInfo: rest.Message);
                    _rfCpuCardReader.MoveCard(CardPos.不持卡位); //退卡
                    return Result<string>.Fail("");
                }
                var cardIdOrder = new byte[] { 0x00, 0xB0, 0x95, 0x0C, 0x0A };
                rest = _rfCpuCardReader.CpuTransmit(cardIdOrder);
                if (!rest.IsSuccess)
                {
                    ShowAlert(false, "友好提示", $"读卡器指令发送失败", debugInfo: rest.Message);
                    _rfCpuCardReader.MoveCard(CardPos.不持卡位); //退卡
                    return Result<string>.Fail("");
                }
                if (rest.Value.Length<8)
                {
                    ShowAlert(false, "友好提示", "卡号读取失败！", debugInfo: rest.Message);
                    _rfCpuCardReader.MoveCard(CardPos.不持卡位); //退卡
                    return Result<string>.Fail("");
                }
                var track = BitConverter.ToString(rest.Value, 0, 8).Replace("-", "");
                return Result<string>.Success(track);
            }).ContinueWith(ret =>
            {
                if (NavigationEngine.State == A.CK.Card) //确定还在当前页面
                {
                    if (ret.Result.IsSuccess)
                    {
                        OnGetInfo(ret.Result.Value);
                        return;
                    }
                    StartRead();//出错，那就继续读卡
                }
            });

        }
        //阅读社保卡
        private void StartReadInsuranceCard(string siCardNo)
        {
            DoCommand(ctx =>
            {
                var cm = (CardModel as CardModel);
                cm.SiCardUseSiNetWork = true;
                CardModel.CardType = CardType.社保卡;
                ctx.ChangeText("正在读卡，请稍后...");

                var ybInfoRest = GetInfoFromSi();//
                if (!ybInfoRest.IsSuccess || !ybInfoRest.Value.IsSuccess)
                {
                    //$$-400~对不起,该人员处于人员黑名单冻结状态,不能在本地医疗机构使用IC卡消费!%%Sim_Operation.F_Orap22%%ORA-0000: normal, successful completion~~~$$
                    //$$-400~此卡状态不正常,此卡状态为挂失%%SIM_TRANSPACK.F_OrafGetPsseno%%ORA-0000: normal, successful completion~~~$$
                    //$$-400~编码为11388940参保人待遇享受时间信息不存在或还未到待遇享受开始时间%%SIM_MEDPUBLIC.P_GetQualification%%ORA-0000: normal, successful completion~~~$$
                    //$$-400~人员缴费状态不正常%%Sim_Operation.F_Orap22%%ORA-0000: normal, successful completion~~~$$
                    //$$-400~找不到该卡号(W33724743)的有关信息%%SIM_TRANSPACK.F_OrafGetPsseno%%ORA-01403: no data found~~~$$
                    if (ybInfoRest.Message.Contains("卡上电复位失败"))
                    {
                        ShowAlert(false, "社保读卡", "您的社保卡没有正确插入，请按图片提示插入社保卡。");
                        return false;
                    }
                    if (ybInfoRest.Message.Contains("此卡状态为挂失"))
                    {
                        ShowAlert(false, "社保读卡", "您的社保卡处于挂失状态");
                        return false;
                    }
                    if (ybInfoRest.Message.Contains("找不到该卡号"))
                    {
                        ShowAlert(false, "社保读卡", "暂时不支持您的社保卡，请到窗口办理业务。");
                        return false;
                    }


                    var errorMsg = "由于社保网络原因，系统将不能使用社保支付，请问继续操作吗?";
                    if (ybInfoRest.Message.Contains("黑名单"))
                    {
                        errorMsg = $"由于您处于社保黑名单冻结状态，系统将不能使用社保支付，请问继续操作吗?";
                    }
                    else if (ybInfoRest.Message.Contains("人员缴费状态不正常"))
                    {
                        errorMsg = $"您的社保卡状态不正常，系统将不能使用社保支付，请问继续操作吗?";
                    }
                    else if (ybInfoRest.Message.Contains("待遇享受时间信息不存在或还未到待遇享受开始时间"))
                    {
                        errorMsg = $"由于您待遇享受时间信息不存在或还未到待遇享受开始时间，系统将不能使用社保支付，请问继续操作吗?";
                    }
                    ShowConfirm("社保操作失败", errorMsg, cbk =>
                    {
                        if (cbk)
                        {
                            cm.SiCardUseSiNetWork = false;
                            OnGetInfo(siCardNo);
                        }
                        else
                        {
                            Navigate(A.Home);
                        }
                    }, 30, confirmExModel);
                    return true;
                }


                var ptInfoRest = PatientIcData.Deserialize(ybInfoRest.Value.写卡后IC卡数据);
                if (!ptInfoRest.IsSuccess)
                {
                    ShowAlert(false, "社保读卡失败", ptInfoRest.Message);
                    return false;

                  
                    //return false;//此处会触发重试
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

                        return false;
                    }
                    CardModel.CardNo = cardNo;
                    CardModel.ExternalCardInfo = "病人信息";
                    Next();
                    return true;
                }
                else if (PatientModel.Res病人信息查询.msg == "请到窗口补全身份证和电话号码信息")
                {
                    ShowAlert(false, "信息不完全", PatientModel.Res病人信息查询.msg);
                    Navigate(A.Home);
                    return false;
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
                    return true;
                    // ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);

                }
            }).ContinueWith(ret =>
            {
                if (!ret.Result) //失败了，重来
                {
                    if (NavigationEngine.State == A.CK.Card)//确定还在当前页面
                    {
                        StartRead();
                    }
                }
            });
        }

        private Result<Res获取参保人员信息> GetInfoFromSi()
        {
            try
            {
                var cm = CardModel as CardModel;
                SiPassword = string.Empty;
                var fullYbInfo = SipayHandler.调用获取参保人员信息(new Req获取参保人员信息() { 读卡方式 = "10" });

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
                ShowMask(false);
            }
        }

        protected override void OnGetInfo(string cardNo, string extendInfo = null)
        {
            if (cardNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "病人信息查询", "卡片信息为空，请确认插卡方向是否正确和卡片是否有效");
                StartRead();
                return;
            }
            CardModel.CardNo = cardNo;
            var choiceModel = GetInstance<IChoiceModel>();
            if (choiceModel.Business == Business.查询)
            {
                var queryChoiceModel = GetInstance<IQueryChoiceModel>();
                if (queryChoiceModel.InfoQueryType == InfoQueryTypeEnum.检验结果)
                {
                    NavigationEngine.Next(new FormContext(A.ChaKa_Context, A.CK.Info));
                    return;
                }

            }
            DoCommand(ctx =>
            {
              
                PatientModel.Req病人信息查询 = new req病人信息查询
                {
                    Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                    cardNo = cardNo,
                    cardType = ((int)CardModel.CardType).ToString()
                };
                PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

                if (PatientModel.Res病人信息查询.success)
                {
                    if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                    {
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)", extend: new AlertExModel()
                        {
                            HideCallback = tp =>
                            {
                                if (tp == AlertHideType.ButtonClick)
                                {
                                    StartRead();

                                }
                            }
                        });
                        return;
                    }
                    CardModel.CardNo = cardNo;
                    var shellvm = GetInstance<IShellViewModel>();
                    shellvm.Busy.IsBusy = false;
                    Next();
                }
                else if (PatientModel.Res病人信息查询.msg.Contains("身份证"))
                {
                    ShowInfoFix(PatientModel.Res病人信息查询.data.FirstOrDefault());
                }
                else
                {
                    ShowAlert(false,
                        "病人信息查询",
                        "未查询到病人的信息",
                        debugInfo: PatientModel.Res病人信息查询.msg,
                        extend: new AlertExModel()
                        {
                            HideCallback = tp =>
                            {
                                if (tp == AlertHideType.ButtonClick)
                                {
                                    StartRead();

                                }
                            }
                        });
                }
            });
        }

        public ICommand InfoFixCommand { get; set; }
        private void ShowInfoFix()
        {
            View.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
                (Action) (() =>
                {
                    ShowMask(true, new PatientTypeDialog() {DataContext = this}
                    ,0.7D
                    , pt => {
                        ShowConfirm("取消补充","取消在自助机上补全身份信息吗？\r\n您也可以在人工窗口补全身份信息", cbk =>
                        {
                            if (cbk)
                            {
                                ShowMask(false);
                                Navigate(A.Home);
                            }
                        });
                       
                    });
                }));
        }
        private void ShowInfoFix(病人信息 info)
        {
            if (info == null)
            {
                ShowAlert(false, "查询失败", "未能查到您的病人信息，请到窗口处理！");
                return;
            }
            var isself = info.guardianNo.IsNullOrWhiteSpace();
            var tips = isself
                ? $"该卡身份信息不完整，需要您持【{info.name}】的身份证进行补全"
                : $"该卡身份信息不完整，需要您持【{info.name}】监护人的身份证进行补全"
                ;
            BeginInvoke(DispatcherPriority.ContextIdle, (() =>
            {
                ShowConfirm("信息补全", tips, cbk =>
                {
                    if (cbk)
                    {
                        InfoFix(isself ? "补全信息_成人" : "补全信息_监护人");
                    }
                    else
                    {
                        Navigate(A.Home);
                    }
                });
            }));

        }

        private void InfoFix(string type)
        {
            //if (type== "补全信息_监护人")
            //{
            //    ShowAlert(false,"暂未实现","儿童信息补全正在测试中，请到窗口办理！");
            //    return;
            //}
            CardModel.ExternalCardInfo = type;
            Navigate(A.CK.IDCard);
        }

        #region[密码输入框]
        private bool _siPwdShowing = false;
        private Uri _jiuZhenCard;
        private Visibility _showSiCardAnimation;
        private Visibility _showBarCodeCardAnimation;

        public string SiPassword
        {
            get { return _siPassword; }
            set
            {
                _siPassword = value;
                OnPropertyChanged();
            }
        }
        //取消社保密码框
        public ICommand CancelPwdCommand { get; set; }
        //密码输入确认
        public ICommand ConfirmPwdCommand { get; set; }
        private void ShowSiPasswordCallback()
        {
            if (_siPwdShowing)
            {
                return;
            }
            _siPwdShowing = true;
            StartTimer();
            View.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle,(Action) (() =>
                {
                    var sm = GetInstance<IShellViewModel>();
                    sm.Busy.IsBusy = false;
                    ShowMask(true, new SiPasswordDialog() {DataContext = this});
                }));
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
                WindowHelper.PostMessage(winPtr, (uint)(WindowHelper.WindowMessage.CLOSE), IntPtr.Zero,IntPtr.Zero);
            }
            ShowMask(false);
            Preview();
        }

        private void ConfirmPwd()
        {
            if (string.IsNullOrEmpty(SiPassword))
            {
                ShowAlert(false, "社保密码", "请输入您的社保密码!");
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
                WindowHelper.PostMessage(btn, (uint)(WindowHelper.WindowMessage.CLICK), IntPtr.Zero, IntPtr.Zero);
            }
            else
            {
                WindowHelper.SendKey(inputtext, 0x0d); //回车

            }
            ShowMask(false);
            SiPassword = string.Empty;
            //100ms后设置允许发现密码弹窗
            Thread.Sleep(100);
            _siPwdShowing = false;
        }
        #endregion

        #region[手动输入卡号]

        public string HospitalCardNo
        {
            get { return _hospitalCardNo; }
            set
            {
                _hospitalCardNo = value;
                OnPropertyChanged();
            }
        }

        public bool HospitalInputFocus
        {
            get { return _hospitalInputFocus; }
            set
            {
                _hospitalInputFocus = value;
                OnPropertyChanged();
            }
        }

        public Visibility ShowHospitalCardButton => YiWuFuBaoYuanConnst.IsEnable ? Visibility.Visible : Visibility.Collapsed;

        public Visibility ShowHospitalCardKeyboard => YiWuFuBaoYuanConnst.CanScreenInput ? Visibility.Visible : Visibility.Collapsed;

        public string ShowHospitalCardButtonText => YiWuFuBaoYuanConnst.ScreenInputText;
        //取消手动输入卡号
        public ICommand CancelHospitalCardNoCommand { get; set; }
        //确认输入的卡号
        public ICommand ConfirmHospitalCardNoCommand { get; set; }
        //弹出手输卡号框
        public ICommand ShowInputMaskCommand { get; set; }
        private void ShowInputMask()
        {
            HospitalCardNo = null;
            HospitalInputFocus = true;
            ShowMask(true, new HospitalCardDialog() { DataContext = this });
            HospitalInputFocus = true;
        }

        private void CancelHospitalCardNo()
        {
            ShowMask(false);
        }
        private void ConfirmHospitalCardNo()
        {
            CardModel.CardType = CardType.就诊卡;
            OnGetInfo(HospitalCardNo);
            HospitalCardNo = String.Empty;
        }
        #endregion

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试卡号");
            if (ret.IsSuccess)
            {
                var cardNo = "";
                PatientIcData patientInfo = null;
                if (ret.Value.StartsWith("$$"))//社保
                {
                    CardModel.CardType = CardType.社保卡;
                    var cm = CardModel as CardModel;
                    cm.参保人员信息 = InsuranceResponseBase.BuildResponse<Res获取参保人员信息>(ret.Value);
                    var ptInfoRest = PatientIcData.Deserialize(cm.参保人员信息.写卡后IC卡数据);
                    cardNo = ptInfoRest.Value.医疗证号;
                    patientInfo = ptInfoRest.Value;
                }
                else
                {
                    CardModel.CardType = CardType.就诊卡;
                    cardNo = ret.Value;
                }
                DoCommand(lp =>
                {

                    PatientModel.Req病人信息查询 = new req病人信息查询
                    {
                        cardNo = cardNo,
                        cardType = ((int) CardModel.CardType).ToString()
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
                        return;
                    }
                    else if (PatientModel.Res病人信息查询.msg == "请到窗口补全身份证和电话号码信息")
                    {
                        ShowAlert(false, "信息不完全", PatientModel.Res病人信息查询.msg);
                        Navigate(A.Home);
                    }
                    else if(CardModel.CardType == CardType.社保卡)
                    {
                        CardModel.CardNo = cardNo;
                        CardModel.ExternalCardInfo = "建档";

                        IdCardModel.IdCardNo = patientInfo.公民身份号;
                        IdCardModel.Name = patientInfo.姓名;
                        IdCardModel.Sex = Convert.ToInt32(IdCardModel.IdCardNo[16]) % 2 == 0 ? Sex.女 : Sex.男;
                        IdCardModel.Birthday = DateTime.ParseExact(IdCardModel.IdCardNo.Substring(6, 8), "yyyyMMdd", null,
                            DateTimeStyles.None);
                        Next();
                        return ;
                        // ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);

                    }
                    else
                    {
                        ShowAlert(false, "测试卡号", PatientModel.Res病人信息查询.msg);
                    }
                });
            }
        }
    }
}
