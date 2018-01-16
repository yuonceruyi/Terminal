using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using YuanTu.Consts.Models.Recharge;
using YuanTu.Core.FrameworkBase;
using Prism.Regions;
using Prism.Commands;
using YuanTu.Consts;
using YuanTu.Default.Component.Auth.Dialog.Views;
using YuanTu.Core.Extension;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Gateway;

namespace YuanTu.QDKFQRM.Component.ZYRecharge.ViewModels
{
    class PayerNameViewModel : ViewModelBase
    {
        public PayerNameViewModel()
        {
            ModifyNameCommand = new DelegateCommand(ModifyNameCmd);
            ConfirmCommand = new DelegateCommand(Confirm);
        }
        public override string Title => "输入交款人姓名";
        private string _hint = "请输入交款人姓名";

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public ICommand ModifyNameCommand { get; set; }
        public ICommand ConfirmCommand { get; set; }
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            ShowMask(false);
            return base.OnLeaving(navigationContext);
        }

        public virtual void Confirm()
        {
            if (Name.IsNullOrWhiteSpace())
            {
                ModifyNameCmd();
                return;
            }
            var patientInfo = PatientModel.住院患者信息;
            IpRechargeModel.Req住院预缴金充值 = new req住院预缴金充值
            {
                patientId = patientInfo.patientHosId,                                
                accountNo = patientInfo.patientHosId,
                payerName = Name,
                operId = FrameworkConst.OperatorId,
            };            
            ChangeNavigationContent($"交款人：{Name}");
            Next();
        }
        public virtual void ModifyNameCmd()
        {
            Name = "";
            ShowMask(true, new FullInputBoard()
            {
                SelectWords = p => { Name = p; },
                KeyAction = p => { StartTimer(); if (p == KeyType.CloseKey) ShowMask(false); }

            }, 0.1, pt => { ShowMask(false); });
        }

        #region Binding
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        #endregion Binding

        #region Ioc
        [Dependency]
        public IIpRechargeModel IpRechargeModel { get; set; }
        [Dependency]
        public IPatientModel PatientModel { get; set; }
        #endregion Ioc
    }
}
