using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Component.Models;

namespace YuanTu.Default.Component.TakeNum.ViewModels
{
    public class RecordViewModel:ViewModelBase
    {
        public override string Title => "选择预约记录";

        [Dependency]
        public IAppoRecordModel RecordModel { get; set; }

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public ITakeNumModel TakeNumModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public ICancelAppoModel CancelAppoModel { get; set; }

        public IReadOnlyCollection<挂号预约记录> 挂号预约记录
        {
            get { return _挂号预约记录; }
            set
            {
                _挂号预约记录 = value;
                OnPropertyChanged();
            }
        }

        public RecordViewModel()
        {
            CancelCommand=new DelegateCommand<挂号预约记录>(DoCancel);
            ConfirmCommand=new DelegateCommand<挂号预约记录>(DoConfirm);
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            挂号预约记录 = RecordModel.Res挂号预约记录查询.data;
        }
      

        protected virtual Result Comfirm()
        {
            return  DoCommand(lp =>
            {
                lp.ChangeText("正在进行取号，请稍候...");

                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                var record = RecordModel.所选记录;

                TakeNumModel.Req预约取号 = new req预约取号
                {
                    patientId = patientInfo.patientId,
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,

                    appoNo = RecordModel.所选记录.regNo,
                    //searchType = ((int)regMode.预约).ToString(),

                    operId = FrameworkConst.OperatorId,
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    accountNo = patientInfo.patientId,
                    cash = PaymentModel.Amount.ToString(),
#pragma warning disable 612
                    medDate = record.medDate,
                    scheduleId = record.scheduleId,
                    medAmPm = record.medAmPm
#pragma warning restore 612

                    //bankCardNo = pos?.CardNo,
                    //bankTime = pos?.TransTime,
                    //bankDate = pos?.TransDate,
                    //posTransNo = pos?.Trace,
                    //bankTransNo = pos?.Ref,
                    //deviceInfo = pos?.TId,
                    //sellerAccountNo = pos?.MId,
                };
                TakeNumModel.Res预约取号 = DataHandlerEx.预约取号(TakeNumModel.Req预约取号);
                if (TakeNumModel.Res预约取号?.success ?? false)
                {
                    PrintModel.SetPrintInfo(true, "取号成功", $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分取号",
                             ConfigurationManager.GetValue("Printer:Receipt"), TakeNumPrintables());


                    Navigate(A.QH.Print);
                    return Result.Success();
                }
                else
                {
                    PrintModel.SetPrintInfo(false, "取号失败",errorMsg: TakeNumModel.Res预约取号?.msg);

                    Navigate(A.QH.Print);
                    return Result.Fail("", TakeNumModel.Res预约取号?.msg);
                }
                
            }).Result;
         
        }

       

        protected virtual Queue<IPrintable> TakeNumPrintables()
        {
            var queue = PrintManager.NewQueue("取号单");
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var takeNum = TakeNumModel.Res预约取号.data;
            var record = RecordModel.所选记录;
            var sb = new StringBuilder();
            sb.Append($"状态：取号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：预约取号\n");
            sb.Append($"科室名称：{record.deptName}\n");
            //sb.Append($"诊疗科室：{paiban.deptName}\n");
            sb.Append($"就诊医生：{record.doctName}\n");
            sb.Append($"挂号费：{record.regFee.In元()}\n");
            sb.Append($"诊疗费：{record.treatFee.In元()}\n");
            sb.Append($"挂号金额：{record.regAmount.In元()}\n");
            sb.Append($"就诊场次：{record.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{takeNum?.address}\n");
            sb.Append($"挂号序号：{takeNum?.appoNo}\n");
            //sb.Append($"个人支付：{Convert.ToDouble(quhao.selfFee).In元()}\n");
            //sb.Append($"医保支付：{Convert.ToDouble(quhao.insurFee).In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
        #region Binding
       
        private IReadOnlyCollection<挂号预约记录> _挂号预约记录;
        public DelegateCommand<挂号预约记录> CancelCommand { get; private set; }
        public DelegateCommand<挂号预约记录> ConfirmCommand { get; private set; }

        protected virtual void DoCancel(挂号预约记录 record)
        {
            RecordModel.所选记录 = record;
            ShowConfirm("取消预约",$"确定取消 {record.deptName} {record.doctName} 的预约吗？", b =>
            {
                if (!b) return;
                DoCommand(lp =>
                {
                    lp.ChangeText("正在进行取消预约，请稍候...");

                    var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                    

                    CancelAppoModel.Req取消预约 = new req取消预约
                    {
                        appoNo = record.appoNo,
                        patientId = patientInfo.patientId,
                        operId = FrameworkConst.OperatorId,
                        regMode = "1",
#pragma warning disable 612
                        medDate = record.medDate,
                        scheduleId = record.scheduleId,
                        medAmPm = record.medAmPm,
                        regNo = record.regNo
#pragma warning restore 612
                    };
                    CancelAppoModel.Res取消预约 = DataHandlerEx.取消预约(CancelAppoModel.Req取消预约);
                    if (CancelAppoModel.Res取消预约?.success ?? false)
                    {
                        ShowAlert(true, "取消预约", "您已取消预约成功");

                    }
                    else
                    {
                        ShowAlert(false, "取消预约", "取消预约失败", debugInfo: CancelAppoModel.Res取消预约?.msg);


                    }

                });

            },60);

            //ShowConfirm("取消预约", $"确定取消 {record.deptName} {record.doctName} 的预约吗？", CancelAppt, 60);



        }


        protected virtual  void DoConfirm(挂号预约记录 record)
        {
            RecordModel.所选记录= record;
            ChangeNavigationContent(record.doctName);
            
            //PaymentModel.Date = record.medDate;
            //PaymentModel.Time = record.medAmPm.SafeToAmPm();
            //PaymentModel.Department = record.deptName;
            //PaymentModel.Doctor = record.doctName;

            PaymentModel.Self = decimal.Parse(record.regAmount);
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Amount = decimal.Parse(record.regAmount);
            PaymentModel.NoPay = false;
            PaymentModel.ConfirmAction = Comfirm;

            PaymentModel.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("日期：",record.medDate),
                new PayInfoItem("时间：",record.medAmPm.SafeToAmPm()),
                new PayInfoItem("科室：",record.deptName),
                new PayInfoItem("医生：",record.doctName),
            };

            PaymentModel.RightList = new List<PayInfoItem>()
            {
                new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                new PayInfoItem("医保报销：",PaymentModel.Insurance.In元()),
                new PayInfoItem("支付金额：",PaymentModel.Amount.In元(),true),
            };

            Next();
        }

      
        #endregion
    }
}
