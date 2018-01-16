using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Consts.Gateway;
using YuanTu.Consts;
using System.Linq;
using YuanTu.Consts.Models.Payment;
using System.Collections.Generic;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Core.Services.PrintService;
using System.Text;
using YuanTu.Default.Component.Tools.Models;
using System.Drawing;
using YuanTu.ShenZhenArea.Enums;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.TakeNum.ViewModels
{
    public class QueryViewModel : YuanTu.Default.Component.TakeNum.ViewModels.QueryViewModel
    {

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }


        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }

        [Dependency]
        public IPrintModel PrintModel { get; set; }



        protected static readonly BarCode.Code128 BarCode128 = new BarCode.Code128
        {
            Magnify = 1,
            Height = 80
        };

        public override string Title => "输入订单号";

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            //ShowAlert(true, "温馨提示", "订单号可选择输入\n输入订单号可以更精确的定位到预约记录");
        }


        public override void Do()
        {
            if (RegNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "订单号", "请输入订单号！");
                return;
            }

            AppoRecordModel.RegNo = RegNo.Trim();
            TakeNumberConfirm();
        }

        public virtual void TakeNumberConfirm()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在进行取号，请稍候...");
                var patientInfo = PatientModel.当前病人信息;

                TakeNumModel.Req预约取号 = new req预约取号
                {
                    patientId = patientInfo.patientId,
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,
                    appoNo = AppoRecordModel.RegNo,
                    searchType = ((int)regMode.预约挂号).ToString(),
                    orderNo = AppoRecordModel.RegNo,
                    operId = FrameworkConst.OperatorId,
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    accountNo = patientInfo.patientId,
                    cash = PaymentModel.Total.ToString(),
                };
                FillRechargeRequest(TakeNumModel.Req预约取号);

                TakeNumModel.Res预约取号 = DataHandlerEx.预约取号(TakeNumModel.Req预约取号);
                if (TakeNumModel.Res预约取号?.success ?? false)
                {
                    ExtraPaymentModel.Complete = true;
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "取号成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分取号",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = TakeNumPrintables(),
                        TipImage = "提示_凭条"
                    });
                    ShowAlert(true, "温馨提示", "35岁以上第一次来我院就诊患者请到分诊台测量血压",20);
                    Navigate(A.QH.Print);
                    return Result.Success();
                }
                else
                {
                    //第三方支付失败时去支付流程里面处理，不在业务里面处理
                    if (NavigationEngine.State != A.Third.PosUnion)
                    {
                        //PrintModel.SetPrintInfo(false, "取号失败", errorMsg: TakeNumModel.Res预约取号?.msg);
                        PrintModel.SetPrintInfo(false, new PrintInfo
                        {
                            TypeMsg = "取号失败",
                            DebugInfo = TakeNumModel.Res预约取号?.msg
                        });
                        Navigate(A.QH.Print);
                    }

                    ExtraPaymentModel.Complete = true;

                    return Result.Fail(TakeNumModel.Res预约取号?.code ?? -100, TakeNumModel.Res预约取号?.msg);
                }
            });
        }

        protected virtual Queue<IPrintable> TakeNumPrintables()
        {
            var queue = PrintManager.NewQueue("预约取号单");
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var takeNum = TakeNumModel.Res预约取号.data;
            var sb = new StringBuilder();
            #region 登记号条码
            var image = BarCode128.GetCodeImage(patientInfo.patientId, BarCode.Code128.Encode.Code128A);
            queue.Enqueue(new PrintItemImage
            {
                Align = ImageAlign.Center,
                Image = image,
                Height = image.Height / 1.5f,
                Width = image.Width / 1.5f
            });
            #endregion
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"登记号：{patientInfo.patientId}\n");
            sb.Append($"就诊科室：{takeNum.deptName}\n");
            sb.Append($"就诊医生：{takeNum.doctName}\n");
            //sb.Append($"挂号费：{takeNum.regFee.In元()}\n");
            //sb.Append($"诊疗费：{takeNum.regAmount.In元()}\n");
            //sb.Append($"预约时段：{(takeNum.medDate.Contains("^")? takeNum.medDate.Split('^')[1]:takeNum.medDate)}\n");
            sb.Append($"预约时段：{(takeNum.medDate.Contains("^") ? takeNum.medDate.Split('^')[1] : takeNum.medDate).Replace("预约时段:", "")}\n");
            sb.Append($"就诊地址：{takeNum.address}\n");
            sb.Append($"取号报到：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();

            //sb.Append($"您的排队号是{takeNum.visitNo}。前方还有{takeNum.extend}人等待就医，请耐心等候！\n");
            sb.Append($"您的排队号是{takeNum.extend}。请耐心等候！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 12, System.Drawing.FontStyle.Bold) });
            sb.Clear();
            
            sb.Append($"35岁以上第一次来我院就诊患者请到分诊台测量血压！\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

        protected virtual void FillRechargeRequest(req预约取号 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
                    req.bankTime = posinfo.TransTime;
                    req.bankDate = posinfo.TransDate;
                    req.posTransNo = posinfo.Trace;
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.MId;
                }
            }
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 || extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = extraPaymentModel.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    req.payAccountNo = thirdpayinfo.buyerAccount;
                    req.transNo = thirdpayinfo.outPayNo;
                    req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                }
            }
        }

    }
}