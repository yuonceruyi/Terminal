using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using YuanTu.Core.FrameworkBase;
using Prism.Regions;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.Extension;
using YuanTu.ChongQingArea.Component.Auth.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.ChongQingArea.SiHandler;
using System.Threading;
using System.Windows.Threading;
using YuanTu.ChongQingArea.Component.Auth.Views;
using YuanTu.ChongQingArea.Component.Auth.ViewModels;
using YuanTu.Consts.Models;
using YuanTu.Consts.Enums;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Consts.Gateway;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;

namespace YuanTu.ChongQingArea.Component.Auth.ViewModels
{
    public class InPatientInfoViewModel : ViewModelBase
    {
        public InPatientInfoViewModel()
        {
            ConfirmCommand = new DelegateCommand(Do);
        }
        public override string Title => "个人信息";
        public string Hint { get; set; } = "住院患者信息";
        public ICommand ConfirmCommand { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            Name = PatientModel.住院患者信息.name;
            Sex = PatientModel.住院患者信息.sex;
            Birth = PatientModel.住院患者信息.birthday;
            IdNo = PatientModel.住院患者信息.idNo.Mask(14, 3);
            AccBalance = PatientModel.住院患者信息.accBalance.In元();
        }
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }
        [Dependency]
        public IPaymentModel PaymentModel { get; set; }
        public virtual void Do()
        {
            ChangeNavigationContent($"{Name}\r\n余额{AccBalance}");
            if (ChoiceModel.Business == Business.出院结算)
            {
                if (CheckCY())
                {
                    GatherConfirmInfo();
                    Next();
                }
                else
                {
                    ShowAlert(false, "自助出院", "没有查询到您的出院申请，请稍后再试");
                    //返回首页
                    return;
                }
            }
            else
            {
                Next();
            }
        }
        [Dependency]
        public ICardModel CardModel { get; set; }
        private void GatherConfirmInfo()
        {
            PaymentModel.LeftList = new List<PayInfoItem>
            {
                new PayInfoItem("日期：", DateTimeCore.Today.ToString("yyyy-MM-dd")),
                new PayInfoItem("时间：",DateTimeCore.Now.ToString("HH:mm:ss")),
                new PayInfoItem("科室：", PatientModel.住院患者信息.deptName),
                new PayInfoItem("入院日期：", PatientModel.住院患者信息.createDate)
            };

            PaymentModel.RightList = new List<PayInfoItem>
            {
                new PayInfoItem("押金金额：", PatientModel.住院患者信息.accBalance.In元(), true),
                new PayInfoItem("消费金额：", PatientModel.住院患者信息.cost.In元(), true),
                new PayInfoItem("押金余额：", PatientModel.住院患者信息.balance.In元(), true),
            };
        }
        private res出院记录查询 出院记录 = new res出院记录查询();
        private bool CheckCY()
        {

            var patientInfo = PatientModel.住院患者信息;
            //var record = BillRecordModel.所选缴费概要;

            //是否使用社保影响卡类型
            //ByWCL20170724
            string t_cardType = "0";
            string t_cardNo = patientInfo.cardNo;
            if (CardModel.CardType == CardType.社保卡)//社保卡不使用社保，身份证号做标识
            {
                t_cardType = ((int)CardType.身份证).ToString();
                t_cardNo = patientInfo.idNo;
            }
            else//其他使用原始卡类型
            {
                t_cardType = ((int)CardModel.CardType).ToString();
            }
            var req = new req出院记录查询
            {
                patientId = patientInfo.patientId,
                cardType = t_cardType,
                cardNo = t_cardNo,
                operId = FrameworkConst.OperatorId,
            };

            出院记录 = DataHandlerEx.出院记录查询(req);
            return 出院记录.success;
        }
        #region Binding
        private string name;
        private string sex;
        private string birth;
        private string idNo;
        private string accBalance;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }
        public string Sex
        {
            get { return sex; }
            set
            {
                sex = value;
                OnPropertyChanged();
            }
        }
        public string Birth
        {
            get { return birth; }
            set
            {
                birth = value;
                OnPropertyChanged();
            }
        }
        public string IdNo
        {
            get { return idNo; }
            set
            {
                idNo = value;
                OnPropertyChanged();
            }
        }
        public string AccBalance
        {
            get { return accBalance; }
            set
            {
                accBalance = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding
    }
}
