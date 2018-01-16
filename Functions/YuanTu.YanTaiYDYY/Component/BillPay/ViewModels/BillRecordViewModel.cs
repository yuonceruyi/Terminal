using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;

namespace YuanTu.YanTaiYDYY.Component.BillPay.ViewModels
{
    public class BillRecordViewModel:YuanTu.Default.Component.BillPay.ViewModels.BillRecordViewModel
    {
        private string _difAmount;

        public string DifAmount
        {
            get { return _difAmount; }
            set
            {
                _difAmount = value;
                OnPropertyChanged();
            }
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            TipMsg = "快速充值";

            Collection = BillRecordModel.Res获取缴费概要信息.data.Select(p => new PageData
            {
                CatalogContent = $"{p.doctName} {p.deptName}\r\n金额 {p.billFee.In元()}",
                List = p.billItem,
                Tag = p
            }).ToArray();
            BillCount = $"{BillRecordModel.Res获取缴费概要信息.data.Count}张处方单";
            TotalAmount = BillRecordModel.Res获取缴费概要信息.data.Sum(p => decimal.Parse(p.billFee)).In元();
            try
            {
                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                var accbalance = Convert.ToDecimal(BillRecordModel.Res获取缴费概要信息.data.Sum(p => decimal.Parse(p.billFee)));
                DifAmount = (Convert.ToDecimal(patientInfo.accBalance ?? "0") - accbalance).In元();
            }
            catch { }
        }

        //实现快速充值
        protected override void Do()
        {
            Switch(A.ChongZhi_Context, A.CZ.RechargeWay);
        }
    }
}
