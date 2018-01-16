using System.Collections.Generic;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.VirtualHospital.Component.Loan.Models;

namespace YuanTu.VirtualHospital.Component.Loan.ViewModels
{
    class InfoViewModel : ViewModelBase
    {
        public override string Title => "借款人当前状态";

        [Dependency]
        public ILoanModel LoanModel { get; set; }

        private List<InfoItem> _items;

        public List<InfoItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        private string _hint = "借款人信息";

        public string Hint
        {
            get { return _hint; }
            set { _hint = value; }
        }

        public DelegateCommand ConfirmCommand { get; set; }

        public InfoViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        private void Confirm()
        {
            Next();
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            var info = LoanModel.Res查询借款权限.data;
            Items = new List<InfoItem>()
            {
                new InfoItem("借款权限：",info.loanAuthorityDesc),
                new InfoItem("信用额度：",info.creditsAmt.In元()),
                new InfoItem("已借款金额：",info.loanedAmt.In元()),
                new InfoItem("剩余额度：",info.remainingAmt.In元()),
                new InfoItem("逾期天数：",info.overdueDays),
                new InfoItem("本院借款权限：",info.hospLoanAuthorityDesc),
                new InfoItem("本院借款剩余额度：",info.hospRemainingAmt.In元()),
                new InfoItem("签约状态：",info.signStatusDesc),
            };
        }
    }
}
