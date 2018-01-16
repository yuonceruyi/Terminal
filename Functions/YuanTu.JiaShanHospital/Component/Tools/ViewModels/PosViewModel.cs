using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.Gateway;
using YuanTu.Core.Log;
using YuanTu.JiaShanHospital.ISO8583;
using YuanTu.JiaShanHospital.ISO8583.External;

namespace YuanTu.JiaShanHospital.Component.Tools.ViewModels
{
    public class PosViewModel : Default.Component.Tools.ViewModels.PosViewModel
    {
        private static Manager _manager;
        public bool Running;
        private Input _input;
        private Visibility _remainVisibility;
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }
        [Dependency]
        public IPatientModel PatientModel { get; set; }

        public Visibility RemainVisibility
        {
            get { return _remainVisibility; }
            set
            {
                _remainVisibility = value;
                OnPropertyChanged();
            }
        }

        public override void OnSet()
        {
            FingerUri = ResourceEngine.GetImageResourceUri("动画素材_手指");
            KeyboardUri = ResourceEngine.GetImageResourceUri("动画素材_金属键盘");
            BackUri = ResourceEngine.GetImageResourceUri("就诊卡银行卡入口");
            CardUri = ResourceEngine.GetImageResourceUri("动画素材_银行卡");
            RemainVisibility = ChoiceModel.Business == Consts.Enums.Business.住院押金
                ? Visibility.Visible
                : Visibility.Hidden;

        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            _hasExit = false;
            _mustClose = false;

            var patientInfo = ExtraPaymentModel.PatientInfo;
            Name = patientInfo.Name;
            CardNo = ChoiceModel.Business == Consts.Enums.Business.住院押金 ? PatientModel.Res住院患者信息查询.extend : patientInfo.CardNo;
            Remain = patientInfo.Remain.In元();
            Business = $"{ExtraPaymentModel.CurrentBusiness}";
            BankPassword = "";
            Amount = ExtraPaymentModel.TotalMoney;
            ShowKeyboardAnimation = false;
            ShowInputPassWord = false;
            StartPosFlow();
        }
        protected override void StartPosFlow()
        {
            Tips = "初始化设备...";
            DoCommand(p =>
            {
                p.ChangeText("正在初始化银联网络，请稍候...");
                var success = InitializePos();
                return success;
            }).ContinueWith(ctx =>
            {
                if (!(ctx.Result && SureGetCard()))
                {
                    TryPreview();
                    return false;
                }
                return true;
            }).ContinueWith(p =>
            {
                if (FrameworkConst.VirtualThridPay)
                    return;
                if (p.Result)
                {
                    SurePassword();
                }
                else
                {
                    _mustClose = true;
                    CloseDevices("异常操作，结束");
                }
            });
        }
        protected override bool InitializePos()
        {
            if (FrameworkConst.VirtualThridPay)
                return true;
            try
            {
                //加载配置
                Config.Configs[""] = new Config
                {
                    TerminalId = Instance.AbcTerminalId,
                    MerchantId = Instance.AbcMerchantId,
                    AcqInst = "231400",
                    Field_2F01 = "086123456789012316",
                    TPDU = Instance.AbcTPDU
                };
                Manager.Address = IPAddress.Parse(Instance.AbcIp);
                Manager.Port = int.Parse(Instance.AbcPort);
                Manager.LoadConfig();
                _manager = new Manager
                {
                    CalcMacFunc = KeyBoard_ZT.CalcMAC,
                    MacKeyEncrptyFunc = KeyBoard_ZT.EncrptyByMacKey
                };

                //初始化金属键盘
                if (!KeyBoard_ZT.InitKeyboard())
                {
                    ShowAlert(false, "银联环境初始化", "键盘初始化失败");
                    Logger.POS.Error($"键盘初始化失败");
                    return false;
                }

                //银联签到
                if (!_manager.IsLogon)
                {
                    var res = _manager.DoLogon();
                    if (!res.IsSuccess)
                    {
                        ShowAlert(false, "银联环境初始化", $"银联签到失败，原因：{res.Message}");
                        Logger.POS.Error($"银联签到失败，原因：{res.Message}");
                        return false;
                    }
                    var bytes = res.Value.Bytes2Hex();

                    Logger.POS.Debug("【61域】" + bytes);
                    var pinkey = res.Value.Skip(1).Take(16).ToArray().Bytes2Hex();
                    var mackey = res.Value.Skip(17).ToArray().Bytes2Hex();

                    if (!KeyBoard_ZT.LoadWorkKey(pinkey, "", mackey, ""))
                    {
                        ShowAlert(false, "银联环境初始化", "加载支付秘钥失败");
                        Logger.POS.Error("验证MAC/PIN失败");
                        return false;
                    }
                    _manager.IsLogon = true;
                }

                //下载参数
                var initrest = _manager.Initialize();
                if (!initrest.IsSuccess)
                {
                    ShowAlert(false, "银联环境初始化", $"下载银联参数失败，原因：{initrest.Message}");
                    Logger.POS.Error($"下载银联参数失败，原因：{initrest.Message}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.POS.Error($"{ex.Message} {ex.StackTrace}");
                ShowAlert(false, "银联环境初始化", "银联初始化失败");
                return false;
            }
        }
        protected override bool SureGetCard()
        {
            PlaySound(SoundMapping.银行卡支付);
            Tips = "请插入银行卡...";
            if (FrameworkConst.VirtualThridPay)
                return true;
            Running = true;
            var configurationManager = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            var port = configurationManager.GetValueInt("Act_A6:Port");
            var baud = configurationManager.GetValueInt("Act_A6:Baud");
            //进行读卡
            if (!_manager.Wrapper.Init(port, baud))
            {
                Logger.POS.Error($"读卡器初始化失败");
                ShowAlert(false, "读银联卡", "读卡器初始化失败");
                return false;
            }
            if (!_manager.Wrapper.EnterCard(true))
            {
                Logger.POS.Error($"读卡器设置进卡失败");
                ShowAlert(false, "读银联卡", "读卡器设置进卡失败");
                return false;
            }
            while (Running)
            {
                CardPos cardPos;
                if (!_manager.Wrapper.CheckCard(out cardPos) || cardPos != CardPos.停卡位 && cardPos != CardPos.IC位)
                {
                    Logger.POS.Debug("未检测到卡");
                    Thread.Sleep(200);
                    continue;
                }
                Logger.POS.Debug("检测到有卡");
                Running = false;
                _input = new Input
                {
                    Amount = (int)ExtraPaymentModel.TotalMoney
                };
                ShowWaitContent(true, "正在读卡，请稍候...");
                try
                {
                    var rest = _manager.ReadCard(_input);
                    if (!rest.IsSuccess)
                    {
                        Logger.POS.Debug($"读卡失败，原因:{rest.Message}");
                        ShowAlert(false, "读银联卡", $"读卡失败，原因:{rest.Message}");
                        ShowWaitContent(false);
                        return false;
                    }
                    ShowWaitContent(false);
                    return true;

                }
                catch (Exception e)
                {
                    ShowWaitContent(false);
                    return false;
                }

            }
            return false;
        }
        protected virtual void ShowWaitContent(bool show = true, string content = null)
        {
            var sm = GetInstance<IShellViewModel>();
            if (show)
            {
                sm.Busy.IsBusy = true;
                sm.Busy.BusyContent = content;
            }
            else
            {
                sm.Busy.IsBusy = false;
            }
        }
        protected override bool SurePassword()
        {
            Tips = "请输入密码...";
            ShowKeyboardAnimation = true;
            ShowInputPassWord = true;

            PlaySound(SoundMapping.输入银行卡密码);
            if (FrameworkConst.VirtualThridPay)
                return true;
            var action = (DelegateCollection.KeyPressDelegate)(key =>
            {
                var c = key;
                Logger.POS.Info("Pin结果:" + c);
                if (c.Equals("exit") || c.Equals("timeout") || c.Equals("cancel"))
                {
                    TryPreview();
                }
                if (c.Equals("workkey"))
                {
                }
                else if (c.Equals("finish"))
                {
                    //输入完成时
                    StartPay();
                }
                else if (c.Equals("clear"))
                {
                    BankPassword = "";
                }
                else
                {
                    BankPassword = c;
                }
            });

            KeyBoard_ZT.keyPressDelegate = action;
            Task.Factory.StartNew(() => KeyBoard_ZT.BeforeAddPin(_input.BankNo));
            return true;
        }
        protected override void StartPay()
        {
            DoCommand(p =>
            {
                if (FrameworkConst.VirtualThridPay)
                {
                    var pret = new Output
                    {
                        Ret = "00",
                        Message = "交易成功",
                        BankNo = "622319******7113",
                        Amount = (int)ExtraPaymentModel.TotalMoney,
                        TransSeq = 111132,
                        TransTime = DateTimeCore.Now,
                        CenterSeq = "094117503517",
                        MerchantID = "302053280620002",
                        TerminalID = "00020026"
                    };
                    return Result<Output>.Success(pret);
                }
                KeyBoard_ZT.StopKeypress = true;
                _input.PIN = KeyBoard_ZT.passBin.Hex2Bytes();
                return _manager.DoSale(_input);
            }).ContinueWith(ret =>
            {
                if (ret.Result.IsSuccess)
                {
                    ExtraPaymentModel.PaymentResult = OutPut2TransRes(ret.Result.Value);
                    if (ExtraPaymentModel.PaymentResult == null)
                        return;
                    var tsk = ExtraPaymentModel.FinishFunc?.Invoke();
                    if (tsk != null)
                    {
                        tsk.ContinueWith(payRet =>
                        {
                            if (!payRet?.Result.IsSuccess ?? false)
                            {
                                if (FrameworkConst.VirtualThridPay)
                                {
                                    ShowAlert(false, "扣费失败", "交易失败，请重试！" + payRet?.Result.Message);
                                    return;
                                }
                                var code = payRet?.Result.ResultCode ?? 0;
                                if (DataHandler.UnKnowErrorCode.Contains(code))
                                {
                                    var errorMsg = $"银联消费成功，未知结果{code}，打印凭条结束交易！\n请执凭条到人工咨询此交易结果！";
                                    PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"),
                                        GatewayUnknowErrorPrintables(errorMsg));
                                    ShowAlert(false, "业务处理异常", errorMsg);
                                    CloseDevices(errorMsg);
                                    Navigate(A.Home);
                                }
                                else
                                {
                                    Logger.Net.Info("开始冲正");
                                    var refundRet = _manager.DoRefund();
                                    Logger.Net.Info($"冲正结束{refundRet.IsSuccess}");
                                    if (!refundRet.IsSuccess)
                                    {
                                        PrintModel.SetPrintInfo(false, new PrintInfo
                                        {
                                            TypeMsg = $"银联{ExtraPaymentModel.CurrentBusiness}单边帐",
                                            TipMsg = $"银联冲正失败，请持凭条与医院工作人员联系！",
                                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                            Printables = RefundFailPrintables(payRet?.Result.Message, refundRet.Message),
                                            TipImage = "提示_凭条"
                                        });
                                        PrintManager.Print();
                                        ShowAlert(false, "扣费失败", "银联冲正失败，请持凭条与医院工作人员联系！\n请尝试其他支付方式！");
                                    }
                                    TryPreview();
                                }
                            }
                            CloseDevices("消费结束");
                        });
                    }
                    else
                    {
                        PrintModel.SetPrintInfo(false, new PrintInfo
                        {
                            TypeMsg = $"银联{ExtraPaymentModel.CurrentBusiness}单边帐",
                            TipMsg = $"银联扣费成功，业务处理失败，请持凭条与医院工作人员联系",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = BusinessFailPrintables(),
                            TipImage = "提示_凭条"
                        });
                        PrintManager.Print();

                        ShowAlert(false, "扣费失败", "交易失败，请重试！");
                        CloseDevices("系统异常，交易操作没有任何返回！");
                        TryPreview();
                    }
                }
                else
                {
                    ShowAlert(false, "扣费失败", $"{ret.Result.Message}");
                    CloseDevices("扣费失败");
                    TryPreview();
                }
            });
        }
        protected override void CloseDevices(string reason)
        {
            if (FrameworkConst.VirtualThridPay)
                return;

            if (ExtraPaymentModel.Complete || _mustClose)
            {
                if (_hasExit)
                    return;
                _hasExit = true;
                try
                {
                    _manager.Wrapper.EjectCard();
                    _manager.Wrapper.EnterCard(false);
                    _manager.Wrapper.Uninit();
                }
                catch (Exception ex)
                {
                }
            }
        }

        protected virtual TransResDto OutPut2TransRes<T>(T res)
        {
            var outPut = res as Output;
            return new TransResDto
            {
                RespCode = outPut?.Ret,
                RespInfo = outPut?.Message,
                CardNo = outPut?.BankNo,
                Amount = outPut?.Amount.ToString(),
                Trace = outPut?.TransSeq.ToString(),
                //Batch = outPut.BatchNo.ToString(),
                Batch = null,
                TransDate = outPut?.TransTime.ToString("MMdd"),
                TransTime = outPut?.TransTime.ToString("HHmmss"),
                Ref = outPut?.CenterSeq,
                Auth = null,
                MId = outPut?.MerchantID,
                TId = outPut?.TerminalID
            };
        }

        protected override Queue<IPrintable> BusinessFailPrintables()
        {
            var queue = PrintManager.NewQueue($"银联{ExtraPaymentModel.CurrentBusiness}单边账");
            var sb = new StringBuilder();
            sb.Append($"状态：银联扣费成功，业务处理失败\n");
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo.Name}\n");
            sb.Append($"卡号：{ExtraPaymentModel.PatientInfo.CardNo}\n");
            sb.Append($"交易类型：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"交易金额：{ExtraPaymentModel.TotalMoney.In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

        protected override Queue<IPrintable> RefundFailPrintables(string refundReason, string refundFailReason)
        {
            var queue = PrintManager.NewQueue($"银联{ExtraPaymentModel.CurrentBusiness}单边账");

            var sb = new StringBuilder();
            sb.Append($"状态：银联冲正失败\n");
            sb.Append($"冲正原因：{refundReason}\n");
            sb.Append($"冲正失败原因：{refundFailReason}\n");
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo.Name}\n");

            sb.Append(ExtraPaymentModel.CurrentBusiness == Consts.Enums.Business.住院押金
                ? $"住院号：{ExtraPaymentModel.PatientInfo.PatientId}\n"
                : $"卡号：{ExtraPaymentModel.PatientInfo.CardNo}\n");
            sb.Append($"交易类型：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"交易金额：{ExtraPaymentModel.TotalMoney.In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

        protected override Queue<IPrintable> GatewayUnknowErrorPrintables(string errorMsg)
        {
            var queue = PrintManager.NewQueue($"银联{ExtraPaymentModel.CurrentBusiness}单边账");
            var sb = new StringBuilder();
            sb.Append($"状态：{errorMsg}\n");
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo.Name}\n");
            sb.Append($"卡号：{ExtraPaymentModel.PatientInfo.CardNo}\n");
            sb.Append($"交易类型：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"交易金额：{ExtraPaymentModel.TotalMoney.In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"银联流水：{(ExtraPaymentModel.PaymentResult as TransResDto)?.Ref}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            if (MisposUnionService.IsBusy)
            {
                ShowAlert(false, "温馨提示", "请把业务执行完再返回,如果密码没输入完毕,请按密码键盘的取消按钮");
                return false;
            }
            KeyBoard_ZT.CloseKeyboard();
            Running = false;
            _mustClose = true;
            CloseDevices("取消操作");
            ShowWaitContent(false);
            return base.OnLeaving(navigationContext);
        }
    }
}
