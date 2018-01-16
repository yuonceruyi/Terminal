using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.JiaShanHospital.NativeServices;
using YuanTu.JiaShanHospital.NativeServices.Dto;
using FontStyle = System.Drawing.FontStyle;

namespace YuanTu.JiaShanHospital.Component.TakeNum.ViewModels
{
    public class TakeNumViewModel : YuanTu.Default.Component.TakeNum.ViewModels.TakeNumViewModel
    {
        private Result<PerGetTicketCheckout> _res = new Result<PerGetTicketCheckout>();
        private static string _address = string.Empty;
        private Visibility _cancelVisibility;

        public Visibility CancelVisibility
        {
            get => _cancelVisibility;
            set
            {
                _cancelVisibility = value;
                OnPropertyChanged();
            }
        }
        private int _confirmButtonPosition;
        public int ConfirmButtonPosition
        {
            get => _confirmButtonPosition;
            set
            {
                _confirmButtonPosition = value;
                OnPropertyChanged();
            }
        }
        private int _gridColumnSpan;
        public int GridColumnSpan
        {
            get => _gridColumnSpan;
            set
            {
                _gridColumnSpan = value;
                OnPropertyChanged();
            }
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            if (DateTime.Parse(RecordModel.所选记录.medDate).Date == DateTimeCore.Now.Date)
            {
                CancelVisibility = Visibility.Collapsed;
                GridColumnSpan = 2;
                ConfirmButtonPosition = 0;
            }
            else
            {
                GridColumnSpan = 0;
                ConfirmButtonPosition = 1;
            }
        }

        protected override void ConfirmAction()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("取号预结算,处理时间在1-2分钟");
                var req = new PerGetTicketCheckoutRequest
                {
                    BussinessType = 5,
                    AppointmentId = RecordModel.所选记录.regNo,
                    AppointmentTime = DateTime.Parse(RecordModel.所选记录.medDate),
                    CardNo = CardModel.CardNo,
                    SelfPayTag = CardModel.CardType == CardType.就诊卡 ? 1 : 0,
                    DayTimeFlag = (DayTimeFlag)(int.Parse(RecordModel.所选记录.medAmPm)),
                    PayFlag = PayMedhodFlag.银联,
                    RegisterOrder = RecordModel.所选记录.appoNo,
                    AlipayTradeNo = "",
                    AlipayAmount = ""
                    //RegisterType = 
                };
                Logger.Net.Info($"嘉善:取号预结算入参{JsonConvert.SerializeObject(req)}");
                _res = LianZhongHisService.GetHospitalPerGetTicketCheckoutInfo(req);
                Logger.Net.Info($"嘉善:取号预结算出参{JsonConvert.SerializeObject(_res)}");
                if (!_res.IsSuccess)
                {
                    ShowAlert(false, "温馨提示", $"取号预结算失败:{_res.Message}");
                    return;
                }
                var record = RecordModel.所选记录;
                ChangeNavigationContent(record.doctName);
                PaymentModel.Self = decimal.Parse(_res.Value?.ActualPay ?? "0") * 100;
                PaymentModel.Insurance = decimal.Parse(_res.Value?.HealthCarePay ?? "0") * 100;
                PaymentModel.Total = decimal.Parse(_res.Value?.TotalPay ?? "0") * 100;
                PaymentModel.NoPay = PaymentModel.Self == 0 || FrameworkConst.DoubleClick;
                PaymentModel.ConfirmAction = Confirm;

                PaymentModel.LeftList = new List<PayInfoItem>
                {
                    new PayInfoItem("时段：",record.medAmPm.SafeToAmPm()),
                    new PayInfoItem("科室：",record.deptName),
                    new PayInfoItem("医生：",record.doctName),
                };

                PaymentModel.RightList = new List<PayInfoItem>
                {
                    new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                    new PayInfoItem("医保报销：",PaymentModel.Insurance.In元()),
                    new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
                };
                Next();
            });
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                try
                {
                    lp.ChangeText("正在进行取号,处理时间在1-2分钟请稍候...");
                    var req = new GetTicketCheckoutRequest
                    {
                        BussinessType = 6,
                        AppointmentId = RecordModel.所选记录.regNo,
                        AppointmentTime = DateTime.Parse(RecordModel.所选记录.medDate),
                        CardNo = CardModel.CardNo,
                        SelfPayTag = CardModel.CardType == CardType.就诊卡 ? 1 : 0,
                        DayTimeFlag = (DayTimeFlag)(int.Parse(RecordModel.所选记录.medAmPm)),
                        RegisterOrder = RecordModel.所选记录.appoNo,
                    };
                    var extraPaymentModel = GetInstance<IExtraPaymentModel>();
                    req.AlipayAmount = (PaymentModel.Self / 100).ToString();
                    switch (PaymentModel.PayMethod)
                    {
                        case PayMethod.未知:
                            break;
                        case PayMethod.现金:
                            break;
                        case PayMethod.银联:
                            Logger.Main.Info("取号银联支付");
                            req.PayFlag = PayMedhodFlag.银联;
                            req.Account = (extraPaymentModel.PaymentResult as TransResDto).CardNo;
                            req.AlipayTradeNo = (extraPaymentModel.PaymentResult as TransResDto).Ref;
                            Logger.Main.Info("取号银联Model构造成功");
                            break;
                        case PayMethod.预缴金:
                            //req.PayFlag = PayMedhodFlag.院内账户;
                            break;
                        case PayMethod.社保:
                            break;
                        case PayMethod.支付宝:
                            req.PayFlag = PayMedhodFlag.支付宝;
                            req.AlipayTradeNo = (extraPaymentModel.PaymentResult as 订单状态).outTradeNo;
                            break;
                        case PayMethod.微信支付:
                            req.PayFlag = PayMedhodFlag.微信;
                            req.AlipayTradeNo = (extraPaymentModel.PaymentResult as 订单状态).outTradeNo;
                            break;
                        case PayMethod.苹果支付:
                            break;
                        case PayMethod.智慧医疗:
                            break;
                        default:
                            break;
                    }
                    Logger.Net.Info($"嘉善:取号结算入参{JsonConvert.SerializeObject(req)}");
                    var res = LianZhongHisService.ExcuteHospitalGetTicketCheckout(req);
                    Logger.Net.Info($"嘉善:取号结算出参{JsonConvert.SerializeObject(_res)}");
                    if (res.IsSuccess)
                    {
                        _address = res.Value.VisitingLocation;
                        ExtraPaymentModel.Complete = true;
                        PrintModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = "取号成功",
                            TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分取号",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = TakeNumPrintables(res.Value.RegisterId),
                            TipImage = "提示_凭条"
                        });
                        lp.ChangeText("正在取号交易记录上传，请稍候...");
                        UploadTradeInfo(res.Value.RegisterId);
                        Navigate(A.QH.Print);
                        return Result.Success();
                    }
                    else
                    {
                        if (NavigationEngine.State != A.Third.PosUnion)
                        {
                            PrintModel.SetPrintInfo(false, new PrintInfo
                            {
                                TypeMsg = "取号失败",
                                DebugInfo = res.Message
                            });
                        }
                        if (res.ResultCode == -100)
                        {
                            UploadTradeInfo("123456");
                        }
                        ShowAlert(false, "友情提示", $"取号失败:{res.Message}");
                        ExtraPaymentModel.Complete = true;
                        return res.ResultCode == -100 ? Result.Fail(-100, res.Message) : Result.Fail(-1, res.Message);
                    }
                }
                catch (Exception e)
                {
                    return Result.Fail(-1, $"支付异常{e}");
                }

            }).Result;
        }

        private void UploadTradeInfo(string settleId)
        {
            try
            {
                取号记录同步到his系统();
                if (PaymentModel.Self != 0)
                {
                    自费交易记录同步到his系统(settleId);
                }
                if (PaymentModel.Insurance != 0)
                {
                    PaymentModel.PayMethod = PayMethod.社保;
                    医保交易记录同步到his系统(settleId);
                }
            }
            catch (Exception e)
            {
                Logger.Net.Error($"嘉善:取号交易记录上传失败:{e.Message}");
            }
        }

        protected Queue<IPrintable> TakeNumPrintables(string id)
        {
            var queue = PrintManager.NewQueue("取号单");
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var record = RecordModel.所选记录;
            var payMethod = PaymentModel.Self == 0 ? "医保支付" : PaymentModel.PayMethod.ToString();
            var sb = new StringBuilder();
            sb.Append($"状态：取号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"就诊卡号：{CardModel.CardNo}\n");
            sb.Append($"交易类型：预约取号\n");
            sb.Append($"科室名称：{record.deptName}\n");
            sb.Append($"就诊医生：{record.doctName}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            sb.Append($"就诊时间：{record.medDate}\n");
            sb.Append($"就诊场次：{record.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{_address}\n");
            sb.Append($"挂号序号：{record?.appoNo}\n");
            sb.Append($"取号总费：{PaymentModel.Total.In元()}\n");
            sb.Append($"自费金额：{PaymentModel.Self.In元()}\n");
            sb.Append($"医保金额：{PaymentModel.Insurance.In元()}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", (CurrentStrategyType() == DeviceType.Clinic ? 10 : 14), FontStyle.Bold) });
            sb.Clear();
            sb.Append($"支付方式：{payMethod}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"交易ID：{id}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"该凭条不作报销凭证！\n");
            sb.Append($"如需退号请到窗口进行处理！\n");
            sb.Append($"如需发票，请凭此票到人工窗口换取发票！\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
        private void 自费交易记录同步到his系统(string settleId)
        {
            try
            {
                Logger.Net.Info("开始取号交易记录同步到his系统");
                var req = new req交易记录同步到his系统
                {
                    platformId = FrameworkConst.OperatorId,
                    hisPatientId = CardModel?.CardNo,
                    cardNo = CardModel?.CardNo,
                    idNo = PatientModel?.当前病人信息?.idNo,
                    patientName = PatientModel?.当前病人信息?.name,
                    tradeType = "2",
                    cash = PaymentModel?.Self.ToString(),
                    operId = FrameworkConst.OperatorId,
                    bankTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tradeMode = GetEnumDescription(PaymentModel.PayMethod),
                    inHos = "1",
                    remarks = "取号自费",
                    settleId = settleId
                };
                FillRechargeRequest(req);
                Logger.Net.Info("取号交易记录构造成功");
                var res = DataHandlerEx.交易记录同步到his系统(req);
                if (res.success)
                {
                    Logger.Net.Info($"取号交易记录同步到his系统成功");
                }
                else
                {
                    Logger.Net.Info($"取号交易记录同步到his系统失败:{res.msg}");
                }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"取号交易记录同步到his系统失败异常:{ex.Message}");
            }
        }
        private void 医保交易记录同步到his系统(string settleId)
        {
            try
            {
                Logger.Net.Info("开始取号交易记录同步到his系统");
                var req = new req交易记录同步到his系统
                {
                    platformId = FrameworkConst.OperatorId,
                    hisPatientId = CardModel?.CardNo,
                    cardNo = CardModel?.CardNo,
                    idNo = PatientModel?.当前病人信息?.idNo,
                    patientName = PatientModel?.当前病人信息?.name,
                    tradeType = "2",
                    cash = PaymentModel?.Insurance.ToString(),
                    operId = FrameworkConst.OperatorId,
                    bankTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tradeMode = GetEnumDescription(PaymentModel.PayMethod),
                    inHos = "1",
                    remarks = "取号医保",
                    settleId = settleId
                };
                Logger.Net.Info("取号交易记录构造成功");
                var res = DataHandlerEx.交易记录同步到his系统(req);
                if (res.success)
                {
                    Logger.Net.Info($"取号交易记录同步到his系统成功");
                }
                else
                {
                    Logger.Net.Info($"取号交易记录同步到his系统失败:{res.msg}");
                }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"取号交易记录同步到his系统失败异常:{ex.Message}");
            }
        }
        public static string GetEnumDescription(PayMethod value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }
        protected void FillRechargeRequest(req交易记录同步到his系统 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
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
                    //req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                    req.extend = thirdpayinfo.outTradeNo;
                }
            }
        }
        private void 取号记录同步到his系统()
        {
            try
            {
                Logger.Net.Info($"开始取号记录同步到his系统");
                var medAmpm = DateTimeCore.Now.Hour >= 12 ? "下午" : "上午";
                var req = new req预约挂号记录同步到his系统
                {
                    regMode = "2",
                    cardType = CardModel.CardType.ToString(),
                    idNo = PatientModel.当前病人信息.idNo,
                    patientName = PatientModel.当前病人信息.name,
                    phone = PatientModel.当前病人信息.phone,
                    medAmPm = medAmpm,
                    medDate = DateTimeCore.Now.Date.ToString("yyyy-MM-dd"),
                    medTime = DateTimeCore.Now.Date.ToString("HH:mm:ss"),
                    deptCode = RecordModel.所选记录.deptCode,
                    deptName = RecordModel.所选记录.deptName,
                    doctCode = "无",
                    cash = PaymentModel.Self.ToString(),
                };
                if (PaymentModel.Self != 0)
                {
                    FillRechargeRequest(req);
                }
                var res = DataHandlerEx.预约挂号记录同步到his系统(req);
                if (res.success)
                {
                    Logger.Net.Info($"取号记录同步到his系统成功");
                }
                else
                {
                    Logger.Net.Info($"取号记录同步到his系统失败:{res.msg}");
                }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"取号记录同步到his系统失败异常:{ex.Message}");
            }
        }
        protected void FillRechargeRequest(req预约挂号记录同步到his系统 req)
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
                    //req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                    req.extend = thirdpayinfo.outTradeNo;
                }
            }
        }
    }
}
