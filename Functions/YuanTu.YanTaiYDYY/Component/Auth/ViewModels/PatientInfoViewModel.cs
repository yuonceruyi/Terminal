using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Core.Extension;
using YuanTu.Devices.CardReader;
using YuanTu.Consts.Gateway;
using YuanTu.Consts;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tools;
using YuanTu.Core.Log;
using System.Windows.Input;
using Prism.Commands;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.YanTaiYDYY.Component.Auth.Dialog.Views;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Gateway;
using YuanTu.Default.Component.Auth.Dialog.Views;
using YuanTu.Default.Component.Tools.Models;

namespace YuanTu.YanTaiYDYY.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : YuanTu.Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        private bool _showFirstPwd;
        private bool _showSecondPwd;

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        public PatientInfoViewModel(IMagCardDispenser[] magCardDispenser) : base(null)
        {
            _magCardDispenser = magCardDispenser.FirstOrDefault(p => p.DeviceId == "ZBR_Mag");
            ConfirmCommand = new DelegateCommand(Confirm);
            UpdateCommand = new DelegateCommand(() =>
            {
                IsAuth = !Phone.IsNullOrWhiteSpace();
                ShowUpdatePhone = true;
            });

            UpdateCancelCommand = new DelegateCommand(() => { ShowUpdatePhone = false; });
            UpdateConfirmCommand = new DelegateCommand(UpdateConfirm);

            FirstPwdCommand = new DelegateCommand(() =>
            {
                ShowFirstPwd = true;
            });
            FirstPwdCancelCommand = new DelegateCommand(() => { ShowFirstPwd = false; });
            FirstPwdConfirmCommand = new DelegateCommand(UpdateFirstPwdConfirm);

            SecondPwdCommand = new DelegateCommand(() =>
            {
                ShowSecondPwd = true;
            });
            SecondPwdCancelCommand = new DelegateCommand(() => { ShowSecondPwd = false; });
            SecondPwdConfirmCommand = new DelegateCommand(UpdateSecondPwdConfirm);
        }

        public ICommand ModifyNameCommand { get; set; }
        public ICommand FirstPwdCommand { get; set; }
        public ICommand FirstPwdCancelCommand { get; set; }
        public ICommand FirstPwdConfirmCommand { get; set; }

        public ICommand SecondPwdCommand { get; set; }
        public ICommand SecondPwdCancelCommand { get; set; }
        public ICommand SecondPwdConfirmCommand { get; set; }

        /// <summary>
        /// 显示输入密码页面
        /// </summary>
        public bool ShowFirstPwd
        {
            get { return _showFirstPwd; }
            set
            {
                _showFirstPwd = value;
                if (value)
                {
                    NewPwd1 = null;

                    PlaySound(SoundMapping.请设置卡密码);
                    ShowMask(true, new FirstPwd { DataContext = this });
                }
                else
                {
                    ShowMask(false);
                }
            }
        }

        /// <summary>
        /// 二次密码输入确认页面
        /// </summary>
        public bool ShowSecondPwd
        {
            get { return _showSecondPwd; }
            set
            {
                _showSecondPwd = value;
                if (value)
                {
                    NewPwd2 = null;

                    PlaySound(SoundMapping.请再次输入卡密码);
                    ShowMask(true, new SecondPwd { DataContext = this });
                }
                else
                {
                    ShowMask(false);
                }
            }
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ModifyNameCommand = new DelegateCommand(ModifyNameCmd);
            ChangeNavigationContent("");

            if (ChoiceModel.Business == Business.建档 && CardModel.CardType == CardType.身份证)
            {
                ChangeNavigationContent($"{IdCardModel.Name}\r\n{IdCardModel.IdCardNo.Mask(14, 3)}");
                Name = IdCardModel.Name;
                Sex = IdCardModel.Sex.ToString();
                Birth = IdCardModel.Birthday.ToString("yyyy-MM-dd");
                Phone = null;
                Pwd1 = null;
                Pwd2 = null;
                IdNo = IdCardModel.IdCardNo.Mask(14, 3);
                IsAuth = false;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;
                CanUpdatePwd = true;
                NameBorderThick = 0;
            }
            else if (ChoiceModel.Business == Business.建档 && CardModel.CardType == CardType.NoCard)
            {
                ChangeNavigationContent($"{IdCardModel.Name}\r\n{IdCardModel.IdCardNo.Mask(14, 3)}");
                Name = IdCardModel.Name;
                Sex = IdCardModel.Sex.ToString();
                Birth = IdCardModel.Birthday.ToString("yyyy-MM-dd");
                Phone = null;
                Pwd1 = null;
                Pwd2 = null;
                IdNo = IdCardModel.IdCardNo.Mask(14, 3);
                IsAuth = false;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;
                CanUpdatePwd = true;
                NameBorderThick = 1;
                ModifyNameCommand.Execute(null);
            }
            else
            {
                IsAuth = true;
                ShowUpdatePhone = false;
                CanUpdatePhone = false;

                CanUpdatePwd = false;
                Pwd1 = null;
                Pwd2 = null;

                NameBorderThick = 0;
                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                Name = patientInfo.name;
                Sex = patientInfo.sex;
                Birth = patientInfo.birthday.SafeToSplit(' ', 1)[0];
                Phone = patientInfo.phone.Mask(3, 4);
                IdNo = patientInfo.idNo.Mask(14, 3);
                GuardIdNo = patientInfo.guardianNo.Mask(14, 3);

                if (CardModel.ExternalCardInfo == "他院卡")
                {
                    CanUpdatePhone = string.IsNullOrWhiteSpace(patientInfo.phone);
                    Phone = string.IsNullOrWhiteSpace(patientInfo.phone) ? null : patientInfo.phone;
                    CanUpdatePwd = true;
                }
            }
        }

        protected virtual void CreatePatient(LoadingProcesser lp)
        {
            if (CardModel.ExternalCardInfo == "他院卡")
            {
                lp.ChangeText("正在激活区域他院就诊卡，请稍候...");
                CreateModel.Req病人建档发卡 = new req病人建档发卡
                {
                    operId = FrameworkConst.OperatorId,
                    cardNo = CardModel.CardNo,
                    cardType = "2",
                    name = IdCardModel.Name,
                    sex = IdCardModel.Sex.ToString(),
                    birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                    idNo = IdCardModel.IdCardNo,
                    idType = "1", //测试必传
                    nation = IdCardModel.Nation,
                    address = IdCardModel.Address,
                    phone = Phone,
#pragma warning disable 612
                    guardianName = null,
                    school = null,
#pragma warning restore 612
                    pwd = NewPwd2, //卡密码
                    cash = null,
                    accountNo = null,
                    patientType = null,
                    setupType = ((int)CreateModel.CreateType).ToString(),
                    extend = "2"//外院卡
                };
                CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);
                if (CreateModel.Res病人建档发卡.success)
                {
                    PatientModel.Req病人信息查询 = new req病人信息查询
                    {
                        cardNo = CardModel.CardNo,
                        cardType = ((int)CardModel.CardType).ToString(),
                        extend = "1"
                    };
                    PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
                    Next();
                }
                else
                {
                    ShowAlert(false, "区域他院就诊卡激活", "激活区域他院就诊卡失败：" + CreateModel.Res病人建档发卡.msg);
                }

            }
            else
            {
                lp.ChangeText("正在准备就诊卡，请稍候...");
                if (!GetNewCardNo()) return;

                lp.ChangeText("正在检查就诊卡，请稍候...");
                if (!CheckCardNo()) return;

                CreateModel.Req病人建档发卡 = new req病人建档发卡
                {
                    operId = FrameworkConst.OperatorId,
                    cardNo = CardModel.CardNo,
                    cardType = "2",
                    name = IdCardModel.Name,
                    sex = IdCardModel.Sex.ToString(),
                    birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                    idNo = IdCardModel.IdCardNo,
                    idType = "1", //测试必传
                    nation = IdCardModel.Nation,
                    address = IdCardModel.Address,
                    phone = Phone,
#pragma warning disable 612
                    guardianName = null,
                    school = null,
#pragma warning restore 612
                    pwd = NewPwd2, //卡密码
                    cash = null,
                    accountNo = null,
                    patientType = null,
                    setupType = ((int)CreateModel.CreateType).ToString(),
                    extend = "1"//本院卡
                };
                Next();
            }
        }

        public override void Confirm()
        {
            if (Name.IsNullOrWhiteSpace())
            {
                ModifyNameCommand.Execute(null);
                return;
            }
            if (ChoiceModel.Business == Business.建档 || CardModel.ExternalCardInfo == "他院卡")
            {
                if (Phone.IsNullOrWhiteSpace())
                {
                    ShowUpdatePhone = true;
                    return;
                }
                if (Pwd1.IsNullOrWhiteSpace())
                {
                    ShowFirstPwd = true;
                    return;
                }
                if (Pwd2.IsNullOrWhiteSpace())
                {
                    ShowSecondPwd = true;
                    return;
                }
            }
            DoCommand(lp =>
            {
                if (ChoiceModel.Business == Business.建档 || CardModel.ExternalCardInfo == "他院卡")
                {
                    switch (CreateModel.CreateType)
                    {
                        case CreateType.成人:
                            CreatePatient(lp);
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
            });
        }

        public virtual void ModifyNameCmd()
        {
            if (CardModel.CardType == CardType.NoCard)
            {
                Name = "";
                ShowMask(true, new FullInputBoard()
                {
                    SelectWords = p => { Name = p; },
                    KeyAction = p => { StartTimer(); if (p == KeyType.CloseKey) ShowMask(false); }

                }, 0.1, pt => { ShowMask(false); });
            }
        }

        protected override bool GetNewCardNo()
        {
            try
            {
                if (FrameworkConst.DoubleClick && FrameworkConst.VirtualThridPay)
                {
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
                    ShowAlert(false, "建档发卡", $"发卡机读卡号失败：{result.Message}");
                    return false;
                }
                if (result.Value[TrackRoad.Trace2].Substring(0, 4) != "3706")
                {
                    ShowAlert(false, "建档发卡", $"发卡机获取卡号异常，卡号不是区域卡：{result.Value[TrackRoad.Trace2]}");
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
        protected virtual bool CheckCardNo()
        {
            //            DoCommand(ctx =>
            //            {
            PatientModel.Req病人信息查询 = new req病人信息查询
            {
                Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),
                extend = "1"
            };
            PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

            if (DataHandler.UnKnowErrorCode.Contains(PatientModel.Res病人信息查询.code))
            {
                ShowAlert(false, "网络校验", "当前网络可能存在异常，请稍候再试或联系工作人员");
                return false;
            }

            if (PatientModel.Res病人信息查询.success)
            {
                if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count <= 0) return true;
                ShowAlert(false, "卡号校验", "该卡号已被使用，请联系工作人员处理");
                return false;
            }
            return true;

            //            }, false);//嵌套，强制执行
            //            return true;
        }

        public virtual void UpdateFirstPwdConfirm()
        {
            if (string.IsNullOrWhiteSpace(NewPwd1))
            {
                ShowAlert(false, "温馨提示", "请输入密码");
                return;
            }
            if (!NewPwd1.Is6Number())
            {
                ShowAlert(false, "温馨提示", "请输入正确的6位数字密码");
                return;
            }
            if (ChoiceModel.Business == Business.建档 || CardModel.ExternalCardInfo == "他院卡")
            {
                Pwd1 = NewPwd1;
                ShowFirstPwd = false;
                if (Pwd2.IsNullOrWhiteSpace())
                {
                    ShowSecondPwd = true;
                    return;
                }
                return;
            }
        }

        public virtual void UpdateSecondPwdConfirm()
        {
            if (string.IsNullOrWhiteSpace(NewPwd2))
            {
                ShowAlert(false, "温馨提示", "请再次输入确认密码");
                return;
            }
            if (!NewPwd2.Is6Number())
            {
                ShowAlert(false, "温馨提示", "请输入正确的6位数字密码");
                return;
            }

            if (NewPwd1 != NewPwd2)
            {
                ShowAlert(false, "温馨提示", "两次密码输入不一致，请确认后重新输入");
                return;
            }
            if (ChoiceModel.Business == Business.建档 || CardModel.ExternalCardInfo == "他院卡")
            {
                Pwd2 = NewPwd2;
                ShowSecondPwd = false;
                return;
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
            if (ChoiceModel.Business == Business.建档 || CardModel.ExternalCardInfo == "他院卡")
            {
                Phone = NewPhone;
                CreateModel.Phone = NewPhone;
                ShowUpdatePhone = false;
                return;
            }
            ShowUpdatePhone = false;
        }
        #region Binding
        private int _nameBorderThick;
        private string _pwd1;
        private string _pwd2;
        private bool _canUpdatePwd;
        private string _buttonPwd1Content;
        private string _buttonPwd2Content;
        private string _newPwd1;
        private string _newPwd2;

        private string _pwd1Tips;
        private string _pwd2Tips;

        public int NameBorderThick
        {
            get { return _nameBorderThick; }
            set
            {
                _nameBorderThick = value;
                OnPropertyChanged();
            }
        }

        public string Pwd1
        {
            get { return _pwd1; }
            set
            {
                _pwd1 = value;
                ButtonPwd1Content = _pwd1 == null ? "添加" : "修改";
                Pwd1Tips = "请输入6位数字密码";
                OnPropertyChanged();
            }
        }
        public string Pwd2
        {
            get { return _pwd2; }
            set
            {
                _pwd2 = value;
                ButtonPwd2Content = _pwd1 == null ? "添加" : "修改";
                Pwd2Tips = "请再次输入6位数字密码";
                OnPropertyChanged();
            }
        }

        public bool CanUpdatePwd
        {
            get { return _canUpdatePwd; }
            set
            {
                _canUpdatePwd = value;
                OnPropertyChanged();
            }
        }
        public string ButtonPwd1Content
        {
            get { return _buttonPwd1Content; }
            set
            {
                _buttonPwd1Content = value;
                OnPropertyChanged();
            }
        }
        public string ButtonPwd2Content
        {
            get { return _buttonPwd2Content; }
            set
            {
                _buttonPwd2Content = value;
                OnPropertyChanged();
            }
        }

        public string NewPwd1
        {
            get { return _newPwd1; }
            set
            {
                _newPwd1 = value;
                OnPropertyChanged();
            }
        }

        public string NewPwd2
        {
            get { return _newPwd2; }
            set
            {
                _newPwd2 = value;
                OnPropertyChanged();
            }
        }

        public string Pwd1Tips
        {
            get { return _pwd1Tips; }
            set
            {
                _pwd1Tips = value;
                OnPropertyChanged();
            }
        }
        public string Pwd2Tips
        {
            get { return _pwd2Tips; }
            set
            {
                _pwd2Tips = value;
                OnPropertyChanged();
            }
        }
        #endregion Binding
    }
}
