using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.FrameworkBase;
using YuanTu.Devices.CashBox;
using Prism.Regions;
using Microsoft.Practices.Unity;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Consts.Models.Recharge;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.DB;

namespace YuanTu.Default.Component.ZYRecharge.ViewModels
{
    public class HatmViewModel : ViewModelBase
    {
        public HatmViewModel()
        {
            ConfirmCashIn = new DelegateCommand(CashIn);
            ConfirmCancle = new DelegateCommand(Cancle);
            ConfirmYes = new DelegateCommand(ContinueYes);
            ConfirmNo = new DelegateCommand(ContinueNo);       
        }
        #region 变量、属性
        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }
        [Dependency]
        public IIpRechargeModel IpRechargeModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        public override string Title => "现金缴住院押金";
        public string Hint { get; set; } = "请按提示缴住院押金";
        public DelegateCommand ConfirmCashIn { get; private set; }
        public DelegateCommand ConfirmCancle { get; private set; }
        public DelegateCommand ConfirmYes { get; private set; }
        public DelegateCommand ConfirmNo { get; private set; }
        private ATMState _currentState;
        private ATMState CurrentState
        {
            get
            {
                return _currentState;
            }
            set
            {              
                _currentState = value;
                CashInVisible = value == ATMState.已放入;
                CancleVisible = value == ATMState.等待入钞;
                ContinueYesVisible = value == ATMState.识别完成;
                ContinueNoVisible = value == ATMState.识别完成;
                var sm = GetInstance<IShellViewModel>();
                switch (value)
                {
                    case ATMState.初始化:
                        Wait(true);                      
                        break;
                    case ATMState.等待入钞:
                        if (Count == 0)
                            HintCash = "请放入100元面值人民币";
                        else
                            HintCash = "请放入100元面值人民币\n若已经放入了足够的现金\n请点击取消直接进入下一步";
                        Wait(false);
                        break;
                    case ATMState.已放入:
                        HintCash = "请在现金放入完成后按确定继续操作";
                        break;
                    case ATMState.正在识别:
                        Wait(true);
                        break;
                    case ATMState.识别完成:
                        HintCash = "是否需要继续放入现金?";
                        Wait(false);
                        break;
                    case ATMState.正在入钞:
                        Wait(true);
                        break;
                    case ATMState.完成入钞:
                        Wait(true);
                        break;
                    case ATMState.入钞失败:
                        Wait(false);
                        break;
                    case ATMState.无法识别:
                        HintCash = "有无法识别的物品\n请取走";
                        Wait(false);
                        break;
                    case ATMState.正在退出:
                        Wait(true);
                        break;
                    case ATMState.退出:
                        Wait(true);
                        break;
                }
            }
        }
        private ATMAction CurrentAction { get; set; }
        enum ATMAction
        {
            入钞,
            取走,
        }
        #endregion

        #region 枚举
        enum ATMState
        {
            初始化,
            等待入钞,
            已放入,
            正在识别,
            识别完成,
            正在入钞,
            完成入钞,
            入钞失败,
            无法识别,
            正在退出,
            退出,
        }
        private int _count;

        private int Count
        {
            get { return _count; }
            set
            {         
                _count = value;
                Amount = value + "元";
            }
        }
        #endregion

        #region Binding
        private bool _cashInVisible;
        private bool _continueYesVisible;
        private bool _continueNoVisible;
        private bool _cancleVisible;        
        private string _hintCash;     
        private string _amount;
        private string _name;
        private string _accBalance;
        
        public bool CashInVisible
        {
            get { return _cashInVisible; }
            set
            {
                _cashInVisible = value;
                OnPropertyChanged();
            }
        }      
        public bool ContinueYesVisible
        {
            get { return _continueYesVisible; }
            set
            {
                _continueYesVisible = value;
                OnPropertyChanged();
            }
        }        
        public bool ContinueNoVisible
        {
            get { return _continueNoVisible; }
            set
            {
                _continueNoVisible = value;
                OnPropertyChanged();
            }
        }
        
        public bool CancleVisible
        {
            get { return _cancleVisible; }
            set
            {
                _cancleVisible = value;
                OnPropertyChanged();
            }
        }        
        public string HintCash
        {
            get { return _hintCash; }
            set
            {
                _hintCash = value;
                OnPropertyChanged();
            }
        }        
        public string Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                OnPropertyChanged();
            }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        public string AccBalance
        {
            get { return _accBalance; }
            set
            {
                _accBalance = value;
                OnPropertyChanged();
            }
        }       
        #endregion Binding          

        public override void OnEntered(NavigationContext navigationContext)
        {
            Count = 0;
            Name = PatientModel.住院患者信息.name;
            AccBalance = decimal.Parse(PatientModel.住院患者信息.accBalance).InRMB();            
            CurrentState = ATMState.初始化;
            CurrentAction = ATMAction.入钞;
            HATM.Call(HATM.Transaction.CashInStart, AtmCallback);
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            if (CurrentState == ATMState.退出)
                return true;

            if (Count == 0 || ExtraPaymentModel.Complete)
            {
                Exit();
                return base.OnLeaving(navigationContext);
            }
            else
            {
                ShowAlert(false, "正在充值","您已投入现金\n请点击确定充值");
                return false;
            }
        }
        protected virtual void CashIn()
        {
            CurrentState = ATMState.正在识别;
            HATM.Call(HATM.Transaction.CashIn, AtmCallback);
        }
        protected virtual void Cancle()
        {
            if (Count == 0)
                Exit();
            else
            {
                CurrentState = ATMState.完成入钞;
                HATM.Call(HATM.Transaction.CashInEnd, AtmCallback);
            }
        }
        protected virtual void ContinueYes()
        {
            CurrentState = ATMState.等待入钞;
            HATM.Call(HATM.Transaction.OpenShutter, AtmCallback);
        }
        protected virtual void ContinueNo()
        {
            CurrentState = ATMState.完成入钞;
            HATM.Call(HATM.Transaction.CashInEnd, AtmCallback);
        }
        protected virtual void Confirm()
        {           
            if (Count == 0)
            {
                Logger.Main.Info("投币金额为0 返回");
                Exit();
                return;
            }
            DoCommand(lp =>
            {
                lp.ChangeText("正在进行充值，请稍候...");
                Logger.Main.Info("投币金额是:" + Count + ",开始充值");
                DBManager.Insert(new CashInputInfo
                {
                    TotalSeconds = Count * 100
                });
                return true;

            }).ContinueWith(ctx =>
            {
                if (ctx.Result)
                {
                    ExtraPaymentModel.TotalMoney = Count * 100;
                    ExtraPaymentModel.FinishFunc?.Invoke();
                }
            });

        }
        void AtmCallback(HATM.CallbackCode callbackCode, int ret, string msg)
        {
            switch (callbackCode)
            {
                case HATM.CallbackCode.STARTCASHSERVICE_OK:
                    HATM.Call(HATM.Transaction.CashInStart, AtmCallback);
                    break;
                case HATM.CallbackCode.STARTCASHSERVICE_FAIL:                    
                    ShowAlert(false, "初始化", "现金模块初始化失败");
                    Exit();
                    break;
                case HATM.CallbackCode.CASHINSTART_OK:
                    HATM.Call(HATM.Transaction.OpenShutter, AtmCallback);
                    break;
                case HATM.CallbackCode.CASHINSTART_FAIL:
                    ShowAlert(false, "初始化", "现金模块初始化失败");                 
                    Exit();
                    break;
                case HATM.CallbackCode.OPENSHUTTER_OK:
                    if (CurrentAction == ATMAction.入钞)
                        CurrentState = ATMState.等待入钞;
                    break;
                case HATM.CallbackCode.OPENSHUTTER_FAIL:
                    if (CurrentState == ATMState.初始化)
                    {
                        ShowAlert(false, "初始化", "现金模块初始化失败");
                        Exit();
                    }
                    else
                    {
                        ShowAlert(false, "初始化", "打开入钞盖失败");                       
                    }
                    break;
                case HATM.CallbackCode.CLOSESHUTTER_OK:
                    CurrentAction = ATMAction.入钞;
                    break;
                case HATM.CallbackCode.CLOSESHUTTER_FAIL:
                    break;
                case HATM.CallbackCode.CASHIN_OK:
                    Count = int.Parse(msg);                    
                    if (CurrentState == ATMState.无法识别)
                        break;
                    CurrentState = ATMState.识别完成;
                    break;
                case HATM.CallbackCode.CASHIN_FAIL:
                    if (CurrentState == ATMState.初始化)
                    {
                        ShowAlert(false, "初始化", "现金模块初始化失败");                       
                        Exit();
                    }
                    else
                    {
                        ShowAlert(false, "入钞", "入钞失败");                        
                        HATM.Call(HATM.Transaction.OpenShutter, AtmCallback);
                    }
                    break;
                case HATM.CallbackCode.CASHINEND_OK:
                    if (CurrentState == ATMState.正在退出)
                    {
                        CurrentState = ATMState.退出;
                        Navigate(A.Home);
                        Wait(false);
                        return;
                    }
                    if (CurrentState == ATMState.完成入钞)
                    {
                        Wait(false);
                        Confirm();                     
                    }
                    break;
                case HATM.CallbackCode.CASHINEND_FAIL:
                    ShowAlert(false, "入钞", "完成入钞失败");                    
                    if (CurrentState == ATMState.正在退出)
                    {
                        CurrentState = ATMState.退出;
                        Navigate(A.Home);
                        Wait(false);
                        return;
                    }
                    break;
                case HATM.CallbackCode.CASHINROLLBACK_OK:
                    HATM.Call(HATM.Transaction.OpenShutter, AtmCallback);
                    break;
                case HATM.CallbackCode.CASHINROLLBACK_FAIL:
                    break;
                case HATM.CallbackCode.STOPCASHSERVICE_OK:
                    break;
                case HATM.CallbackCode.STOPCASHSERVICE_FAIL:
                    break;
                case HATM.CallbackCode.CASH_REFUSE:
                    CurrentAction = ATMAction.取走;
                    CurrentState = ATMState.无法识别;
                    HATM.Call(HATM.Transaction.OpenShutter, AtmCallback);
                    break;

                case HATM.CallbackCode.CASH_ITEMSINSERTED:
                    CurrentState = ATMState.已放入;
                    break;

                case HATM.CallbackCode.CASH_ITEMSTAKEN:
                    if (CurrentAction == ATMAction.入钞)
                        CurrentState = ATMState.等待入钞;
                    else
                        CurrentState = ATMState.识别完成;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(callbackCode), callbackCode, null);
            }
        }
        void Exit()
        {
            CurrentState = ATMState.正在退出;
            HATM.Call(HATM.Transaction.CashInEnd, AtmCallback);
        }
    }
}
