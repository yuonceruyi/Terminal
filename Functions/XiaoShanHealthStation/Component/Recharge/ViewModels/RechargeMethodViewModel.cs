using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Default.Component.Auth.Dialog.Views;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.XiaoShanArea.CitizenCard;
using YuanTu.XiaoShanArea.Consts.Extensions;
using YuanTu.XiaoShanArea.Consts.Enums;
using YuanTu.XiaoShanHealthStation.Component.Auth.Models;
using YuanTu.XiaoShanHealthStation.Component.Recharge.Dialog.Views;
using YuanTu.XiaoShanHealthStation.Component.Recharge.Models;

namespace YuanTu.XiaoShanHealthStation.Component.Recharge.ViewModels
{
    public class RechargeMethodViewModel : Default.Component.Recharge.ViewModels.RechargeMethodViewModel
    {
        private ObservableCollection<InfoIcon> _dataEx;
        private bool _isAuth;
        private string _newPhone;
        private string _passWord;
        private string _passwordTips = "请输入密码";
        private Result<DataHandlerEx.Response> _result;
        private bool _runningCheckPassword;
        private bool _userCancelInputPassword;

        public RechargeMethodViewModel()
        {
          
            UpdateCancelCommand = new DelegateCommand(() => ShowMask(false));
            UpdateConfirmCommand = new DelegateCommand(UpdateConfirm);
            PasswordCancelCommand = new DelegateCommand(() => ShowMask(false));
        }

        public override string Title => "输入密码";

        [Dependency]
        public IChaKaModel ChaKaModel { get; set; }

        [Dependency]
        public IRechargeModel RechargeModel { get; set; }

     
        public ICommand UpdateCancelCommand { get; set; }
        public ICommand UpdateConfirmCommand { get; set; }
        public ICommand PasswordCancelCommand { get; set; }

        public string NewPhone
        {
            get { return _newPhone; }
            set
            {
                _newPhone = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get { return _passWord; }
            set
            {
                _passWord = value;
                OnPropertyChanged();
            }
        }

        public bool IsAuth
        {
            get { return _isAuth; }
            set
            {
                _isAuth = value;
                OnPropertyChanged();
            }
        }

        public string PasswordTips
        {
            get { return _passwordTips; }
            set
            {
                _passwordTips = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<InfoIcon> DataEx
        {
            get { return _dataEx; }
            set
            {
                _dataEx = value;
                OnPropertyChanged();
            }
        }

        public Func<Result> CallBack { get; set; }

        public override void OnSet()
        {
            base.OnSet();
            var payButtonCmd = new DelegateCommand<Info>(OnExtButtonClick);

            var list = new List<InfoIcon>
            {
                new InfoIcon
                {
                    Title = "功能开通",
                    ConfirmCommand = payButtonCmd,
                    IconUri = null,
                    Tag = ExtendFunction.功能开通,
                    Color = new Color()
                },
                new InfoIcon
                {
                    Title = "密码修改",
                    ConfirmCommand = payButtonCmd,
                    IconUri = null,
                    Tag = ExtendFunction.密码修改,
                    Color = new Color()
                },
                new InfoIcon
                {
                    Title = "余额查询",
                    ConfirmCommand = payButtonCmd,
                    IconUri = null,
                    Tag = ExtendFunction.余额查询,
                    Color = new Color()
                }
            };

            DataEx = new ObservableCollection<InfoIcon>(list);
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            PasswordTips = "请输入密码";
        }
        public override bool OnLeaving(NavigationContext navigationContext)
        {
            _runningCheckPassword = false;
            return base.OnLeaving(navigationContext);
        }

        protected override void OnPayButtonClick(Info i)
        {
            var payMethod = (PayMethod) i.Tag;
            OpRechargeModel.RechargeMethod = payMethod;
            ExtraPaymentModel.Complete = false;
            ExtraPaymentModel.CurrentBusiness = Business.充值;
            ExtraPaymentModel.CurrentPayMethod = payMethod;
            ExtraPaymentModel.FinishFunc = OnRechargeCallback;
            //准备门诊充值所需病人信息
            var patientInfo = ChaKaModel.查询建档Out;
            ExtraPaymentModel.PatientInfo = new PatientInfo
            {
                Name = patientInfo.病人姓名,
                PatientId = patientInfo.就诊卡号,
                IdNo = patientInfo.身份证号,
                GuardianNo = null,
                CardNo = CardModel.CardNo,
                Remain = decimal.Parse(patientInfo.市民卡余额)
            };

            ChangeNavigationContent(OpRechargeModel.RechargeMethod.ToString());
            switch (payMethod)
            {
                case PayMethod.未知:
                case PayMethod.预缴金:
                case PayMethod.社保:
                    throw new ArgumentOutOfRangeException();
                case PayMethod.现金:
                    OnCAClick();
                    break;

                case PayMethod.银联:
                case PayMethod.支付宝:
                case PayMethod.微信支付:
                case PayMethod.苹果支付:
                    Navigate(A.CZ.InputAmount);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override Task<Result> OnRechargeCallback()
        {
            return DoCommand(p =>
            {
                _result = new Result<DataHandlerEx.Response>();

                try
                {
                    p.ChangeText("正在进行充值，请稍候...");
                    RechargeModel.Res市民卡账户充值 = null;

                    FillRequest();

                    _result = DataHandlerEx.Query(RechargeModel.Req市民卡账户充值);
                    if (!_result.IsSuccess)
                    {
                        PrintModel.SetPrintInfo(false, new PrintInfo
                        {
                            TypeMsg = "充值失败",
                            TipMsg = $"您于{DateTimeCore.Now.ToString("HH:mm")}分充值{ExtraPaymentModel.TotalMoney.In元()}失败",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            PrintablesList = new List<Queue<IPrintable>>
                            {
                                OpRechargePrintables(false)
                            },
                            TipImage = "提示_凭条",
                            DebugInfo = _result.Message
                        });
                        Navigate(A.CZ.Print);
                        ExtraPaymentModel.Complete = true;
                        return Result.Fail(_result.Message);
                    }

                    RechargeModel.Res市民卡账户充值 = Res市民卡账户充值.Deserilize(_result.Value.dest);

                    ExtraPaymentModel.Complete = true;

                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "充值成功",
                        TipMsg = $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功充值{ExtraPaymentModel.TotalMoney.In元()}",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        PrintablesList = new List<Queue<IPrintable>>
                        {
                            OpRechargePrintables(true)
                        },
                        TipImage = "提示_凭条"
                    });
                    Navigate(A.CZ.Print);
                    return Result.Success();
                }
                catch (Exception ex)
                {
                    Logger.Main.Error(
                        $"[{ExtraPaymentModel.CurrentPayMethod}充值]发起存钱交易时出现异常，原因:{ex.Message}\r\n{ex.StackTrace}\r\n{ex.InnerException}");
                    ShowAlert(false, "充值失败", "发生系统异常 ");
                    return Result.Fail("系统异常");
                }
                finally
                {
                    DBManager.Insert(new RechargeInfo
                    {
                        CardNo = ChaKaModel?.CardNo,
                        PatientId = ChaKaModel?.查询建档Out?.就诊卡号,
                        RechargeMethod = ExtraPaymentModel.CurrentPayMethod,
                        TotalMoney = ExtraPaymentModel.TotalMoney,
                        Success = _result.IsSuccess,
                        ErrorMsg = _result.Message
                    });
                }
            });
        }

        protected override Queue<IPrintable> OpRechargePrintables(bool success)
        {
            if (!success)
                if (ExtraPaymentModel.CurrentPayMethod != PayMethod.现金) //只有现金才需要打凭条
                    return null;
            var queue = PrintManager.NewQueue("市民卡充值");
            var patientInfo = ChaKaModel.查询建档Out;
            var sb = new StringBuilder();
            sb.Append($"状态：充值{(success ? "成功" : "失败")}\n");
            sb.Append($"姓名：{patientInfo.病人姓名}\n");
            sb.Append($"就诊卡号：{patientInfo.就诊卡号}\n");
            sb.Append($"充值方式：{ExtraPaymentModel.CurrentPayMethod}\n");
            sb.Append($"充值前余额：{ChaKaModel.Remain.In元()}\n");
            sb.Append($"充值金额：{RechargeModel.Req市民卡账户充值.amount.In元()}\n");
            if (success)
            {
                sb.Append($"充值后余额：{RechargeModel.Res市民卡账户充值.账户余额.In元()}\n");
                sb.Append($"收据号：{RechargeModel.Res市民卡账户充值.凭证号}\n");
            }
            else
            {
                sb.Append($"异常原因：{_result.Message}\n");
            }
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的个人信息。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText {Text = sb.ToString()});

            return queue;
        }

        protected virtual void FillRequest()
        {
            switch (ExtraPaymentModel.CurrentPayMethod)
            {
                case PayMethod.未知:
                    break;

                case PayMethod.现金:
                    RechargeModel.Req市民卡账户充值 = new Req市民卡账户充值
                    {
                        transCode = 0001,
                        银行卡号 = "".FillPadChar(20),
                        银行卡流水 = "".FillPadChar(20),
                        卡类型 = ChaKaModel.IsCitizenCard ? "00" : "01",
                        卡号 = ChaKaModel.CardNo.FillPadChar(12),
                        amount = ExtraPaymentModel.TotalMoney
                    };
                    break;

                case PayMethod.银联:
                    RechargeModel.Req市民卡账户充值 = new Req市民卡账户充值
                    {
                        transCode = 0002,
                        银行卡号 = ((TransResDto) ExtraPaymentModel.PaymentResult).CardNo.FillPadChar(20),
                        银行卡流水 = ((TransResDto) ExtraPaymentModel.PaymentResult).Trace.FillPadChar(20),
                        卡类型 = ChaKaModel.IsCitizenCard ? "00" : "01",
                        卡号 = ChaKaModel.CardNo.FillPadChar(12),
                        amount = ExtraPaymentModel.TotalMoney
                    };
                    break;

                case PayMethod.预缴金:
                    break;

                case PayMethod.社保:
                    break;

                case PayMethod.支付宝:
                    break;

                case PayMethod.微信支付:
                    break;

                case PayMethod.苹果支付:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual void OnExtButtonClick(Info i)
        {
            var info = i.Tag.As<ExtendFunction>();
            switch (info)
            {
                case ExtendFunction.功能开通:
                    OpenFunction();
                    break;
                case ExtendFunction.密码修改:
                    UpdatePassword();
                    break;
                case ExtendFunction.余额查询:
                    RemainQuery();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual void OpenFunction()
        {
            //todo 账户和智慧医疗均开通
            if (ChaKaModel.HasSmartHealth)
            {
                ShowAlert(true, "温馨提示", "您的智慧医疗结算已开通！");
                return;
            }

            //todo 智慧医疗未开通

            ShowConfirm("智慧医疗开通",
                "提示信息：\"智慧医疗结算\"须知\n" +
                "智慧医疗结算服务使用须知\n" +
                "杭州市卫生局为解决就医难中的门诊就诊三长一短问题（挂号、候诊、付费排队时间长、诊疗时间短），联合杭州市民卡有限公司推出了\"智慧医疗结算\"服务，说明如下：\n" +
                "1、\"智慧医疗结算\"服务以开通市民卡帐户为前提，开通\"智慧医疗结算\"服务后自动开通帐户功能，诊间结算时自费部分使用市民卡帐户内资 金（《市民卡帐户使用须知》内的圈存、查询及密码规则不适用于儿童市民卡）。\n" +
                "2、\"智慧医疗结算\"服务开通后，持卡人在所有特约医疗机构就诊时使用市民卡帐户支付免输密码。该功能必须本人凭卡使用。\n" +
                "3、持卡人可在特约医疗机构现金充值，该充值金额可退。本使用须知最终解释权归我司所有", cp =>
                {
                    if (!cp) return;

                    IsAuth = false;
                    
                    
                    PlaySound(SoundMapping.请输入手机号码);
                    ShowMask(true, new UpdatePhone {DataContext = this});
                });
        }

        protected virtual void UpdatePassword()
        {
            if (ChaKaModel.CardType == "2" || ChaKaModel.CardType == "3")
            {
                var result = 读十进制卡号();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "温馨提示", result.Message);
                    return;
                }
                StartCheckPassword(() =>
                {
                    RechargeModel.OldPassword = RechargeModel.Password;
                    string password1St = null;
                    string password2Nd;
                    StartCheckPassword(() =>
                    {
                        password1St = RechargeModel.Password;
                        StartCheckPassword(() =>
                        {
                            password2Nd = RechargeModel.Password;
                            if (password1St != password2Nd)
                            {
                                ShowAlert(false, "温馨提示", "两次密码不一致，请重试！");
                                return Result.Fail("两次密码不一致");
                            }
                            RechargeModel.NewPassword = RechargeModel.Password;
                            result = 修改密码();
                            if (!result.IsSuccess)
                            {
                                ShowAlert(false, "温馨提示", result.Message);
                                return Result.Fail(result.Message);
                            }
                            ShowAlert(true, "温馨提示", "修改密码成功");
                            return Result.Success();
                        }, "请再次输入新密码");
                        return Result.Success();
                    }, "请输入六位新密码");

                    return Result.Success();
                });
            }
            else
            {
                ShowAlert(false, "温馨提示", "您的卡暂不支持该功能！");
            }
        }

        protected virtual void RemainQuery()
        {
            if (ChaKaModel.CardType == "2" || ChaKaModel.CardType == "3")
            {
                var result = 读十进制卡号();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "温馨提示", result.Message);
                    return;
                }
                StartCheckPassword(() =>
                {
                    result = 查询余额();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", result.Message);
                        return Result.Fail(result.Message);
                    }
                    ShowAlert(true, "温馨提示", $"余额：{ChaKaModel.Remain}元");
                    return Result.Success();
                });
            }
            else
            {
                ShowAlert(true,"温馨提示",$"余额：{ChaKaModel.Remain}元");
            }
        }


        protected virtual void UpdateConfirm()
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

            ShowMask(false);

            if (!ChaKaModel.HasAccount || !ChaKaModel.HasSmartHealth)
                if (ChaKaModel.CardType == "1")
                {
                    var result = 儿童医疗开通();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", result.Message);
                        return;
                    }
                    ShowAlert(true, "温馨提示", "您的智慧医疗功能已成功开通！");
                }
                else
                {
                    var result = 账户医疗开通();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", result.Message);
                        return;
                    }
                    ShowAlert(true, "温馨提示", "您的智慧医疗功能已成功开通！");
                }
            else if (!ChaKaModel.HasSmartHealth)
                if (ChaKaModel.CardType == "1")
                {
                    var result = 儿童医疗开通();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", result.Message);
                        return;
                    }
                    ShowAlert(true, "温馨提示", "您的智慧医疗功能已成功开通！");
                }
                else
                {
                    var result = 读十进制卡号();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", result.Message);
                        return;
                    }

                    ShowMask(true, new Password {DataContext = this});
                    StartCheckPassword(智慧医疗开通);
                }
        }

        protected virtual Result 儿童医疗开通()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在开通儿童医疗，请稍候...");
                var req = new Req儿童医疗开通
                {
                    transCode = 57005,
                    amount = 0,
                    标志位 = "003",
                    手机号 = NewPhone.FillPadChar(11),
                    身份证号 = ChaKaModel.查询建档Out.身份证号.FillPadChar(18),
                    外卡号 = ChaKaModel.CardNo.FillPadChar(9),
                    芯片号 = ChaKaModel.Res读接触非接卡号.卡识别码.FillPadChar(32)
                };
                var result = DataHandlerEx.Query(req);
                if (!result.IsSuccess)
                    return Result.Fail($"儿童医疗开通失败:{result.Message}");
                var res = Res儿童医疗开通.Deserilize(result.Value.dest);
                return res.应答码 == "00"
                    ? Result.Success()
                    : Result.Fail($"儿童医疗开通失败:{ErrorCodeParse.RespondParse(res.应答码)}");
            }).Result;
        }

        protected virtual Result 账户医疗开通()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在开通账户医疗，请稍候...");
                var req = new Req账户医疗开通
                {
                    transCode = 57005,
                    amount = 0,
                    标志位 = "001",
                    手机号 = NewPhone,
                    账户开通 = "0001",
                    网上支付 = "0000",
                    短信提醒 = "0000",
                    智慧医院 = "0001",
                    身份证号 = ChaKaModel.查询建档Out?.身份证号
                };
                var result = DataHandlerEx.Query(req);
                if (!result.IsSuccess)
                    return Result.Fail($"账户医疗开通失败:{result.Message}");
                var res = Res账户医疗开通.Deserilize(result.Value.dest);
                return res.应答码 == "00"
                    ? Result.Success()
                    : Result.Fail($"账户医疗开通失败:{ErrorCodeParse.RespondParse(res.应答码)}");
            }).Result;
        }

        protected virtual Result 智慧医疗开通()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在开通智慧医疗，请稍候...");
                var req = new Req智慧医疗开通
                {
                    transCode = 57005,
                    amount = 0,
                    标志位 = "002",
                    手机号 = NewPhone,
                    账户开通 = "0000",
                    网上支付 = "0000",
                    短信提醒 = "0000",
                    智慧医院 = "0001",
                    身份证号 = ChaKaModel.查询建档Out?.身份证号,
                    //todo 交易密码
                    交易密码 = "交易密码"
                };
                var result = DataHandlerEx.Query(req);
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "温馨提示", $"智慧医疗开通失败:{result.Message}");
                    return Result.Fail($"智慧医疗开通失败:{result.Message}");
                }

                var res = Res智慧医疗开通.Deserilize(result.Value.dest);
                if (res.应答码 == "00")
                {
                    ;
                    ShowAlert(true, "温馨提示", "您的智慧医疗功能已成功开通！");
                    return Result.Success();
                }

                ShowAlert(false, "", $"智慧医疗开通失败:{ErrorCodeParse.RespondParse(res.应答码)}");
                return Result.Success();
            }).Result;
        }

        protected virtual Result 读十进制卡号()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在读卡号，请稍候...");
                var req = new Req读十进制卡号
                {
                    transCode = 9901,
                    amount = 0
                };
                var result = DataHandlerEx.Query(req);
                if (!result.IsSuccess)
                    return Result.Fail($"读卡号失败:{result.Message}");
                var res = Res读十进制卡号.Deserilize(result.Value.dest);
                RechargeModel.DCardNo = res.卡号;
                return Result.Success();
            }).Result;
        }

        protected virtual Result 修改密码()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在修改密码，请稍候...");
                var req = new Req密码修改或重置分次十六进制
                {
                    transCode = 91325,
                    amount = 0,
                    修改类型 = "0002",
                    修改对象 = "0001",
                    身份证号码 = ChaKaModel.查询建档Out?.身份证号?.FillPadChar(20, StringHandler.EncodingFlag.GBK),
                    原密码 = RechargeModel.OldPassword,
                    新密码 = RechargeModel.NewPassword,
                    姓名 = ChaKaModel.查询建档Out?.病人姓名?.FillPadChar(20, StringHandler.EncodingFlag.GBK)
                };
                var result = DataHandlerEx.Query(req);
                if (!result.IsSuccess)
                    return Result.Fail($"修改密码失败:{result.Message}");
                var res = Res密码修改或重置分次十六进制.Deserilize(result.Value.dest);
                return res.应答码 == "00"
                    ? Result.Success()
                    : Result.Fail($"修改密码失败:{ErrorCodeParse.RespondParse(res.应答码)}");
            }).Result;
        }
        protected virtual Result 查询余额()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在查询余额，请稍候...");
                var req = new Req余额查询密码外入版
                {
                    transCode = 91025,
                    amount = 0,
                    密码 = RechargeModel.Password
                };
                var result = DataHandlerEx.Query(req);
                if (!result.IsSuccess)
                    return Result.Fail($"查询余额失败:{result.Message}");
                var res = Res余额查询密码外入版.Deserilize(result.Value.dest);
                ChaKaModel.Remain =decimal.Parse(res?.金额??"0");
                return Result.Success();
            }).Result;
        }

        protected virtual void 密码回显()
        {
            var req = new Req密码回显
            {
                transCode = 9904,
                amount = 0,
                卡号 = RechargeModel.DCardNo.FillPadChar(10)
            };
            var result = DataHandlerEx.Query(req);
            if (!result.IsSuccess)
                return;

            var res = Res密码回显.Deserilize(result.Value.dest);
            RechargeModel.Res密码回显 = res;
            if (res.首位标志 == "0")
            {
                Logger.Device.Info($"用户输入密码：{res.密码值}");
                RechargeModel.Password = res.密码值;
            }
            if (res.首位标志 == "1")
            {
                Logger.Device.Info("用户取消密码输入");
                _userCancelInputPassword = true;
            }
            if (res.首位标志 == "2")
            {
                Logger.Device.Info("用户密码输入超时");
                ShowAlert(false, "温馨提示", "输入密码超时");
                _userCancelInputPassword = true;
            }
            if (res.首位标志 == "3")
            {
                Logger.Device.Info("密码键盘未处于密码输入状态");
                ShowAlert(false, "温馨提示", "密码键盘未处于密码输入状态");
                _userCancelInputPassword = true;
            }
            if (res.首位标志 == "4")
            {
                Logger.Device.Info("用户输入还未完成");
                ShowAlert(false, "温馨提示", "用户输入还未完成");
                _userCancelInputPassword = true;
            }
        }

        protected virtual void CheckPassword()
        {
            _runningCheckPassword = true;
            while (_runningCheckPassword)
            {
                Thread.Sleep(1000);
                密码回显();
                if (_userCancelInputPassword)
                {
                    _runningCheckPassword = false;
                    ShowMask(false);
                }
                else
                {
                    Password = RechargeModel.Password;
                    if (RechargeModel.Res密码回显 != null && RechargeModel.Res密码回显.密码个数 == "0016")
                    {
                        Logger.Device.Info($"用户输入了确定，16进制密码:{RechargeModel.Res密码回显.密码值}");
                        _runningCheckPassword = false;
                        ShowMask(false);
                        CallBack?.Invoke();
                    }
                }
            }
        }

        protected virtual void StartCheckPassword(Func<Result> callBack = null, string tips = null)
        {
            PasswordTips = tips ?? "请输入密码";
            _userCancelInputPassword = false;
            CallBack = callBack;
            new Thread(CheckPassword)
            {
                IsBackground = true,
                Name = "CheckPassword"
            }.Start();
        }
    }
}