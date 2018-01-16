using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Core.Extension;
using YuanTu.ChongQingArea.Component.Auth.Models;
using YuanTu.Consts.Enums;
using YuanTu.Consts;
using YuanTu.Core.Log;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.ChongQingArea.Models.Payment;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Gateway;

namespace YuanTu.ChongQingArea.Component.ViewModels
{
    public class SelectSiViewModel : ViewModelBase
    {
        public SelectSiViewModel()
        {
            NoSiCommand = new DelegateCommand(NoSiPay);
            UseSiCommand = new DelegateCommand(UseSiPay);
        }

        public override string Title => "请点击下方卡片选择是否使用社保";
        public string Tips = "";

        public override void OnEntered(NavigationContext navigationContext)
        {
            SiModel.UseSi = false;
            ChangeNavigationContent("");

            LeftList = PaymentModel.LeftList;
            RightList = PaymentModel.RightList;
            MidList = PaymentModel.MidList;

            ViewTitle = Title;
            Hint = $"{ChoiceModel.Business}信息";

            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();

            var prefix = "PayIn:充值";
            ImgTxtBtnIconUri = resource.GetImageResourceUri(config.GetValue($"{prefix}:ImageName"));
            NoSiContent = "自费支付";
            UseSiContent = "社保支付";
            if (ChoiceModel.Business == Business.出院结算 && decimal.Parse(PatientModel.住院患者信息.balance) > 0)
            {
                NoSiCommand = null;
                UseSiCommand = null;
                //tips.
                Tips = "您的账户中尚有余额，请到窗口进行结算 ";
                OnPropertyChanged();
            }
        }

        //[Dependency]社保支付
        //public ICardModel CardModel { get; set; }
        protected virtual void UseSiPay()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在计算社保费用信息，请稍候...");
                SiModel.UseSi = true;
                PaymentModel.PayMethod = PayMethod.社保;
                Logger.Main.Info("进入UseSiPay");
                if (CardModel.CardType == CardType.社保卡)
                {
                    Logger.Main.Info("持社保卡");
                    var rest = SiModel.ConfirmSi?.Invoke();
                    if (rest?.IsSuccess ?? false)
                    {
                        Logger.Main.Info("SiModel.ConfirmSi成功");
                        PaymentModel.NoPay = (PaymentModel.Self == 0);

                        if (PaymentModel.FundPayment > 0)
                        {
                            PaymentModel.RightList = new List<PayInfoItem>()
                            {
                                new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                                new PayInfoItem("医保统筹支付：",PaymentModel.FundPayment.In元()),
                                new PayInfoItem("医保帐户支付：",PaymentModel.AccountPay.In元()),
                            };
                        }
                        else if (PaymentModel.CivilServant > 0)
                        {
                            PaymentModel.RightList = new List<PayInfoItem>()
                            {
                                new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                                new PayInfoItem("医保统筹支付：",PaymentModel.FundPayment.In元()),
                                new PayInfoItem("医保帐户支付：",PaymentModel.AccountPay.In元()),
                                new PayInfoItem("公务员补：",PaymentModel.CivilServant.In元()),
                                new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
                            };
                        }                      
                        else
                        {
                            PaymentModel.RightList = new List<PayInfoItem>()
                            {
                                new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                                new PayInfoItem("医保统筹支付：",PaymentModel.FundPayment.In元()),
                                new PayInfoItem("医保帐户支付：",PaymentModel.AccountPay.In元()),
                                new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
                            };
                        }

                        Next();
                    }
                    else
                    {
                        Logger.Main.Info("[社保]SiModel.ConfirmSi失败");

                        Logger.Main.Info("预结算失败进行解锁");
                        if (ChoiceModel.Business == Business.挂号)
                        {
                            UnlockSource();
                        }
                        ShowAlert(false, "社保预结算", "社保预结算失败:" + rest?.Message);
                        return;
                    }

                }
                else
                {
                    Logger.Main.Info("持就诊卡");
                    if (ChoiceModel.Business == Business.挂号)
                    {
                        StackNavigate(A.XianChang_Context, B.XC.InsertSiCard);
                    }
                    else if (ChoiceModel.Business == Business.缴费)
                    {
                        StackNavigate(A.JiaoFei_Context, B.JF.InsertSiCard);
                    }
                    else if (ChoiceModel.Business == Business.出院结算)
                    {
                        StackNavigate(B.ChuYuanJieSuan_Context, B.CY.InsertSiCard);
                    }
                    else if (ChoiceModel.Business == Business.取号)
                    {
                        StackNavigate(A.QuHao_Context, B.QH.InsertSiCard);
                    }
                    else
                    {
                        Next();
                    }
                }
            });
        }

        /// <summary>
        /// 自费支付
        /// </summary>
        protected virtual void NoSiPay()
        {
            Logger.Main.Info("进入NoSiPay");
            SiModel.UseSi = false;
            PaymentModel.PayMethod = PayMethod.未知;
            PaymentModel.NoPay = false;
            PaymentModel.Self = PaymentModel.Total;
            PaymentModel.Insurance = 0;
            PaymentModel.RightList = new List<PayInfoItem>()
            {
                new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true),
            };
            Next();
        }

        /// <summary>
        /// 挂号解锁
        /// </summary>
        private void UnlockSource()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在解除锁号,请稍后......");
                var scheduleInfo = ScheduleModel.所选排班;
                var lock01 = new req挂号解锁
                {
                    operId = FrameworkConst.OperatorId,
                    medDate = scheduleInfo.medDate,
                    scheduleId = scheduleInfo.scheduleId,
                    lockId = RegisterModel.Res挂号锁号?.data?.lockId,
                };
                //可以无返回
                RegisterModel.Res挂号解锁 = DataHandlerEx.挂号解锁(lock01);

            });
        }

        //public override bool OnLeaving(NavigationContext navigationContext)
        //{
        //    Logger.Main.Info("社保返回主界面");

        //    if (ChoiceModel.Business == Business.挂号)
        //    {
        //        UnlockSource();
        //    }
        //    return base.OnLeaving(navigationContext);
        //}

        #region
        private bool _useSi;
        private string _viewTitle;
        private string _hint;
        private bool _noPay;
        private List<PayInfoItem> _leftList;
        private List<PayInfoItem> _rightList;
        private List<PayInfoItem> _midList;
        private string _noSiContent;
        private string _useSiContent;
        //private string _quickRechargeContent;
        private Uri _imgTxtBtnIconUri;

        public bool UseSi
        {
            get { return _useSi; }
            set
            {
                _useSi = value;
                OnPropertyChanged();
            }
        }

        public string ViewTitle
        {
            get { return _viewTitle; }
            set
            {
                _viewTitle = value;
                OnPropertyChanged();
            }
        }

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public bool NoPay
        {
            get { return _noPay; }
            set
            {
                _noPay = value;
                OnPropertyChanged();
            }
        }

        public List<PayInfoItem> LeftList
        {
            get { return _leftList; }
            set
            {
                _leftList = value;
                OnPropertyChanged();
            }
        }

        public List<PayInfoItem> RightList
        {
            get { return _rightList; }
            set
            {
                _rightList = value;
                OnPropertyChanged();
            }
        }

        public List<PayInfoItem> MidList
        {
            get { return _midList; }
            set
            {
                _midList = value;
                OnPropertyChanged();
            }
        }

        public string NoSiContent
        {
            get { return _noSiContent; }
            set
            {
                _noSiContent = value;
                OnPropertyChanged();
            }
        }
        public string UseSiContent
        {
            get { return _useSiContent; }
            set
            {
                _useSiContent = value;
                OnPropertyChanged();
            }
        }

        public Uri ImgTxtBtnIconUri
        {
            get { return _imgTxtBtnIconUri; }
            set
            {
                _imgTxtBtnIconUri = value;
                OnPropertyChanged();
            }
        }


        #endregion

        #region

        [Dependency]
        public IPaymentModels PaymentModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }

        [Dependency]
        public ISiModel SiModel { get; set; }

        [Dependency]
        public IRegisterModel RegisterModel { get; set; }

        [Dependency]
        public IScheduleModel ScheduleModel { get; set; }
        #endregion

        #region Binding

        public ICommand NoSiCommand { get; set; }
        public ICommand UseSiCommand { get; set; }

        #endregion
    }
}