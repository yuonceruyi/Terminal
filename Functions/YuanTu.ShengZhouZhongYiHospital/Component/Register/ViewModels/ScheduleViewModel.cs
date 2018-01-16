using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;
using YuanTu.Consts.Models.Payment;
using System.Net.Sockets;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Core.Log;
using Prism.Regions;
using Prism.Commands;
using System.Collections.ObjectModel;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Services.PrintService;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Consts.Dtos.UnionPos;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Reporter;
using YuanTu.ShengZhouZhongYiHospital.Extension;
using YuanTu.ShengZhouZhongYiHospital.HisNative.Models;

namespace YuanTu.ShengZhouZhongYiHospital.Component.Register.ViewModels
{
    public class ScheduleViewModel : YuanTu.Default.Component.Register.ViewModels.ScheduleViewModel
    {
        [Dependency]
        public IConfigurationManager Manager { get; set; }
        [Dependency]
        public IBusinessConfigManager BusinessConfigManager { get; set; }
        [Dependency]
        public IRegDateModel RegDateModel { get; set; }
        [Dependency]
        public IDoctorModel DoctorModel { get; set; }
        [Dependency]
        public IQueryChoiceModel queryChoiceModel { get; set; }
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = ScheduleModel.Res排班信息查询.data.Select(p => new InfoMore
            {
                Title = p.doctName.BackNotNullOrEmpty(DepartmentModel.所选科室?.deptName),
                SubTitle = $"{时间处理(p.medAmPm)}",
                Type = "挂号费",
                Amount = decimal.Parse(p.regAmount),
                ConfirmCommand = confirmCommand,
                Tag = p,
                IsEnabled = p.restnum != "0" || p.extend != "0",
                Extends = "剩余号源: " + p.restnum
            });
            Data = new ObservableCollection<InfoMore>(list);



            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号医生 : SoundMapping.选择预约医生);
        }

        protected override void QuerySource(Info i)
        {
            DoCommand(lp =>
            {
                string regType;
                if (RegTypesModel.SelectRegType.RegType == RegType.专科普通)
                {
                    regType = "03";
                }
                else if (RegTypesModel.SelectRegType.RegType == RegType.专科专家)
                {
                    regType = "06";
                }
                else if (RegTypesModel.SelectRegType.RegType == RegType.夜间特需)
                {
                    regType = "05";
                }
                else
                {
                    regType = "0" + ((int)RegTypesModel.SelectRegType.RegType).ToString();
                }
                SourceModel.Req号源明细查询 = new req号源明细查询
                {
                    operId = FrameworkConst.OperatorId,
                    regMode = ChoiceModel.Business == Business.挂号 ? "01" : "02",
                    regType = regType,
                    medAmPm = ScheduleModel.所选排班.medAmPm,
                    medDate = ChoiceModel.Business == Business.挂号
                        ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                        : RegDateModel.RegDate,
                    deptCode = DepartmentModel.所选科室.deptCode,
                    scheduleId = ScheduleModel.所选排班.scheduleId,
                    doctCode = ScheduleModel.所选排班.doctCode
                };
                SourceModel.Res号源明细查询 = DataHandlerEx.号源明细查询(SourceModel.Req号源明细查询);
                if (SourceModel.Res号源明细查询?.success ?? false)
                {
                    if (SourceModel.Res号源明细查询.data.Any())
                    {
                        var data = SourceModel.Res号源明细查询.data;
                        var limitTime = DateTime.Parse("12:00");
                        if (ChoiceModel.Business == Business.挂号)
                        {
                            if (RegTypesModel.SelectRegType.RegType != RegType.急诊门诊)
                            {
                                var tag = string.Empty;
                                #region 挂号 取第一个号源

                                if (DateTime.Compare(DateTimeCore.Now, limitTime) < 0)
                                {
                                    for (int c = data.Count - 1; c >= 0; c--)
                                    {
                                        var time = DateTime.Parse(data[c].appoNo.Split('-')[1]);
                                        if (DateTime.Compare(limitTime, time) > 0)
                                        {
                                            if (string.IsNullOrEmpty(tag))
                                            {
                                                tag = data[c].appoNo;
                                            }
                                            else
                                            {
                                                var tagTimeArr = tag.Split('-');
                                                if (DateTime.Compare(time, DateTime.Parse(tagTimeArr[1])) < 0)
                                                {
                                                    tag = data[c].appoNo;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    for (int c = data.Count - 1; c >= 0; c--)
                                    {
                                        var time = DateTime.Parse(data[c].appoNo.Split('-')[1]);
                                        if (DateTime.Compare(limitTime, time) < 0)
                                        {
                                            if (string.IsNullOrEmpty(tag))
                                            {
                                                tag = data[c].appoNo;
                                            }
                                            else
                                            {
                                                var tagTimeArr = tag.Split('-');
                                                if (DateTime.Compare(time, DateTime.Parse(tagTimeArr[1])) < 0)
                                                {
                                                    tag = data[c].appoNo;
                                                }
                                            }
                                        }
                                    }
                                }

                                #endregion
                                SourceModel.所选号源 = data.FirstOrDefault((p) => p.appoNo == tag);
                            }
                            CalcAndFillPayInfo(lp);
                        }
                        else
                        {
                            if (RegTypesModel.SelectRegType.RegType != RegType.急诊门诊 &&
                                RegTypesModel.SelectRegType.RegType != RegType.夜间特需)
                            {
                                var temp = new List<号源明细>();
                                for (int c = data.Count - 1; c >= 0; c--)
                                {
                                    var time = DateTime.Parse(data[c].appoNo.Split('-')[1]);
                                    if ((DateTime.Compare(limitTime, time) > 0 && RegDateModel.AmPm == AmPmSession.上午) ||
                                        (DateTime.Compare(limitTime, time) < 0 && RegDateModel.AmPm == AmPmSession.下午) && data[c].restNum != "0")
                                    {
                                        temp.Add(data[c]);
                                    }
                                }
                                if (!temp.Any())
                                {
                                    ShowAlert(false, "号源明细查询", "没有获得号源信息(列表为空)");
                                    return; ;
                                }
                                SourceModel.Res号源明细查询.data = temp;
                            }
                            ChangeNavigationContent(i.Title);
                            Navigate(A.YY.Time);
                        }
                    }
                    else
                    {
                        ShowAlert(false, "号源明细查询", "没有获得号源信息(列表为空)");
                    }
                }
                else
                {
                    ShowAlert(false, "号源明细查询", "没有获得号源信息", debugInfo: SourceModel.Res号源明细查询?.msg);
                }
            });
        }

        protected override void Confirm(Info i)
        {
            ScheduleModel.所选排班 = i.Tag.As<排班信息>();
            ChangeNavigationContent(i.Title);
            QuerySource(i);
        }

        protected void CalcAndFillPayInfo(LoadingProcesser lp)
        {
            if (ChoiceModel.Business == Business.挂号)
            {
                var rmModel = RegisterModel as Models.RegisterModel;
                var ret = 挂号结算And退号(lp);
                if (ret.IsSuccess)
                {
                    var payfee = rmModel.Res挂号取号预结算;
                    PaymentModel.Total = decimal.Parse(payfee.总金额) * 100;
                    PaymentModel.Self = decimal.Parse(payfee.个人现金支付金额) * 100;
                    PaymentModel.Insurance = decimal.Parse(payfee.医保支付金额) * 100;
                    PaymentModel.NoPay = ChoiceModel.Business == Business.预约 || PaymentModel.Self == 0;
                    //默认预约或者自费金额为0时不支付            
                    PaymentModel.ConfirmAction = Confirm;

                    PaymentModel.LeftList = new List<PayInfoItem>()
                        {
                            new PayInfoItem("日期：", RegDateModel.RegDate),
                            new PayInfoItem("时间：",
                                RegTypesModel.SelectRegType.RegType == RegType.急诊门诊 ? "全天" : DateTime.Compare(DateTime.Parse("12:00"),DateTimeCore.Now)>0?"上午":"下午"),
                            new PayInfoItem("科室：", DepartmentModel?.所选科室?.deptName),
                        };

                    PaymentModel.RightList = new List<PayInfoItem>()
                        {
                            new PayInfoItem("医保支付：", PaymentModel.Insurance.In元()),
                            new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                            new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true),
                        };
                    Logger.Net.Info($"[嵊州本地服务，挂号]Navigate");
                    Navigate(A.XC.Confirm);
                }
            }
            else
            {
                PaymentModel.Self = decimal.Parse(DepartmentModel.所选科室.deptIntro) * 100;
                PaymentModel.Insurance = decimal.Parse("0");
                PaymentModel.Total = decimal.Parse(DepartmentModel.所选科室.deptIntro) * 100;
                PaymentModel.NoPay = true; //默认预约或者自费金额为0时不支付            
                PaymentModel.ConfirmAction = Confirm;

                PaymentModel.LeftList = new List<PayInfoItem>()
                {
                    new PayInfoItem("日期：", ChoiceModel.Business == Business.挂号
                        ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                        : RegDateModel.RegDate),
                    new PayInfoItem("时间：", SourceModel?.所选号源?.appoNo),
                    new PayInfoItem("科室：", DepartmentModel?.所选科室?.deptName),
                };

                PaymentModel.RightList = new List<PayInfoItem>()
                {
                    new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                    new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true),
                };
                Next();
            }

        }

        private Result 挂号预结算(LoadingProcesser lp)
        {
            var rmModel = RegisterModel as Models.RegisterModel;
            lp.ChangeText("正在进行挂号预结算，请稍候...");

            var scheduleId = ScheduleModel?.所选排班.scheduleId;
            var treatfee = DepartmentModel.所选科室.fullPy.InRMB();
            var regAmount = DepartmentModel.所选科室.deptIntro.InRMB();
            rmModel.Req挂号取号预结算 = new Req挂号取号预结算()
            {
                患者唯一标识 = PatientModel.当前病人信息.patientId,
                姓名 = PatientModel.当前病人信息.name,
                排班表主键 = scheduleId,
                科室编号 = DepartmentModel.所选科室.deptCode,
                医生工号 = "",
                挂号类型 = "1",
                挂号时间 = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                医保类型 = "",
                预约标记 = "0",
                预约记录主键 = "0",
                挂号费 = "0.00",
                诊疗费 = treatfee,
                工本费 = "0",
                就诊序号 = "0",
                挂号序号 = "0",
                现金结算单流水号 = "",
                现金结算金额 = "0",
                总金额 = regAmount,
                程序名 = "0",
                操作科室 = "0",
                终端编号 = FrameworkConst.OperatorId,
                值班类别 = DepartmentModel?.所选科室?.parentDeptCode,
                支付类别 = "1"
            };
            try
            {
                rmModel.Res挂号取号预结算 = HisHandleEx.执行挂号取号预结算(rmModel.Req挂号取号预结算);
                if (rmModel.Res挂号取号预结算.IsSuccess)
                {
                    return Result.Success();
                }
                ShowAlert(false, "计算失败", rmModel.Res挂号取号预结算.Message);
                return Result.Fail(rmModel.Res挂号取号预结算.Message);
            }
            catch
            {
                ShowAlert(false, "温馨提示", $"挂号预结算失败:HIS服务出现异常，请联系工作人员重启HIS服务");
                return Result.Fail("HIS服务出现异常");
            }

        }

        private Result 挂号结算And退号(LoadingProcesser lp)
        {
            var rmModel = RegisterModel as Models.RegisterModel;
            var ys = 挂号预结算(lp);
            if (!ys.IsSuccess)
            {
                return ys;
            }
            if (rmModel.Res挂号取号预结算.医保类型 != "78")//农保存在该问题
            {
                return ys;
            }


            var scheduleId = ScheduleModel?.所选排班.scheduleId;
            var treatfee = DepartmentModel.所选科室.fullPy.InRMB();
            var regAmount = DepartmentModel.所选科室.deptIntro.InRMB();
            var req = new Req挂号取号结算()
            {
                患者唯一标识 = PatientModel.当前病人信息.patientId,
                姓名 = PatientModel.当前病人信息.name,
                排班表主键 = scheduleId,
                科室编号 = DepartmentModel.所选科室.deptCode,
                医生工号 = "", //ScheduleModel.所选排班.doctCode,
                挂号类型 = "1",
                挂号时间 = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                医保类型 = "", //PatientModel.当前病人信息.accountNo,
                预约标记 = "0",
                预约记录主键 = "0",
                挂号费 = "0.00",
                诊疗费 = treatfee,
                工本费 = "0",
                就诊序号 = "0",
                挂号序号 = "0",
                现金结算单流水号 = "SB0001 ", //强制搞死你
                现金结算金额 = "0",
                总金额 = regAmount,
                程序名 = "0",
                操作科室 = "0",
                终端编号 = FrameworkConst.OperatorId,
                值班类别 = DepartmentModel?.所选科室?.parentDeptCode,
                支付类别 = "1" //强制银联
            };


            var res = HisHandleEx.执行挂号取号结算(req);
            if (!res.IsSuccess)
            {
                ShowAlert(false, "计算费用失败", res.Message);
                return Result.Fail(res.Message);
            }
            //回滚

            var reqRefund = new Req挂号取号回滚()
            {
                患者唯一标识 = PatientModel.当前病人信息.patientId,
                姓名 = PatientModel.当前病人信息.name,
                原挂号记录序号 = res.挂号序号,
                预约标识 = "0",
                预约记录序号 = null
            };
            int i = 3;
            Res挂号取号回滚 resRefund = null;
            while (i-- > 0)
            {
                Logger.Net.Info($"嵊州挂号预结算第{i}次回滚");
                resRefund = HisHandleEx.执行挂号取号回滚(reqRefund);
                if (resRefund.IsSuccess)
                {
                    rmModel.Res挂号取号预结算.总金额 = res.总金额;
                    rmModel.Res挂号取号预结算.个人现金支付金额 = res.个人现金支付金额;
                    rmModel.Res挂号取号预结算.医保支付金额 = res.医保支付金额;
                    return Result.Success();
                }

            }
            ShowAlert(false, "费用计算时代扣失败", resRefund.Message);
            PrintRefundFailed(resRefund.Message);
            return Result.Fail(resRefund.Message);
        }


        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                var rmModel = RegisterModel as Models.RegisterModel;
                var scheduleId = ScheduleModel?.所选排班.scheduleId;
                var treatfee = DepartmentModel.所选科室.fullPy.InRMB();
                var regAmount = DepartmentModel.所选科室.deptIntro.InRMB();
                lp.ChangeText("正在进行现场挂号，请稍候...");
                var extraPaymentModel = GetInstance<IExtraPaymentModel>();
                var tranNo = string.Empty;
                int ZHIFULX = 1;
                if (PaymentModel.PayMethod == PayMethod.银联)
                {
                    tranNo = (extraPaymentModel.PaymentResult as TransResDto).Ref;
                    ZHIFULX = 1;
                }
                else if (PaymentModel.PayMethod == PayMethod.支付宝)
                {
                    tranNo = (extraPaymentModel.PaymentResult as 订单状态).outTradeNo.Substring(4);
                    ZHIFULX = 2;
                }
                else if (PaymentModel.PayMethod == PayMethod.微信支付)
                {
                    tranNo = (extraPaymentModel.PaymentResult as 订单状态).outTradeNo.Substring(4);
                    ZHIFULX = 3;
                }
                else
                {
                    tranNo = BusinessConfigManager.GetFlowId("挂号结算");
                }
                rmModel.Req挂号取号结算 = new Req挂号取号结算()
                {
                    患者唯一标识 = PatientModel.当前病人信息.patientId,
                    姓名 = PatientModel.当前病人信息.name,
                    排班表主键 = scheduleId,
                    科室编号 = DepartmentModel.所选科室.deptCode,
                    医生工号 = "",//ScheduleModel.所选排班.doctCode,
                    挂号类型 = "1",
                    挂号时间 = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                    医保类型 = "",//PatientModel.当前病人信息.accountNo,
                    预约标记 = "0",
                    预约记录主键 = "0",
                    挂号费 = "0.00",
                    诊疗费 = treatfee,
                    工本费 = "0",
                    就诊序号 = "0",
                    挂号序号 = "0",
                    现金结算单流水号 = tranNo,
                    现金结算金额 = "0",
                    总金额 = regAmount,
                    程序名 = "0",
                    操作科室 = "0",
                    终端编号 = FrameworkConst.OperatorId,
                    值班类别 = DepartmentModel?.所选科室?.parentDeptCode,
                    支付类别 = ZHIFULX.ToString()
                };
                var res = rmModel.Res挂号取号结算 = HisHandleEx.执行挂号取号结算(rmModel.Req挂号取号结算);
                if (res.IsSuccess)
                {
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "挂号成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分" + "挂号",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = RegisterPrintables(res),
                        TipImage = "提示_凭条"
                    });
                    ExtraPaymentModel.Complete = true;
                    Logger.Net.Info("[嵊州本地服务，挂号结算] 跳转到 打印");
                    try
                    {
                        lp.ChangeText("正在交易数据上传,请稍后...");
                        上传数据到HIS();
                        if (PaymentModel.Self != 0)
                        {
                            自费交易记录同步到his系统();
                        }
                        if (PaymentModel.Insurance != 0)
                        {
                            医保交易记录同步到his系统(tranNo);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Net.Error($"数据统一上传失败:{e.Message}");
                    }
                    Navigate(A.XC.Print);
                    return Result.Success();
                }
                PrintModel.SetPrintInfo(false, new PrintInfo
                {
                    TypeMsg = "挂号失败",
                    DebugInfo = res?.Message
                });
                ExtraPaymentModel.Complete = true;
                ReportService.HIS请求失败($"嵊州挂号结算失败,HIS服务返回:{res.原始报文}", null);
                Navigate(A.XC.Print);
                Logger.Net.Info($"[嵊州本地服务，挂号结算] 结算失败返回");
                return Result.Fail(res?.RetCode ?? -100, res?.Message);


            }).Result;
        }
        protected Queue<IPrintable> RegisterPrintables(Res挂号取号结算 res)
        {
            try
            {

                var queue = PrintManager.NewQueue("(挂号凭条)");
                var department = DepartmentModel?.所选科室;
                var schedule = ScheduleModel?.所选排班;
                var patientInfo = PatientModel?.当前病人信息;
                var extraPaymentModel = GetInstance<IExtraPaymentModel>();
                var tranNo = string.Empty;
                if (PaymentModel.PayMethod == PayMethod.银联)
                {
                    tranNo = (extraPaymentModel.PaymentResult as TransResDto).Ref;
                }
                else if (PaymentModel.PayMethod == PayMethod.支付宝 || PaymentModel.PayMethod == PayMethod.微信支付)
                {
                    tranNo = (extraPaymentModel.PaymentResult as 订单状态).outTradeNo;
                }
                var time = DateTimeCore.Now.JudgeAmPm();
                var sb = new StringBuilder();
                sb.Append($"--------------------------------\n");
                sb.Append("【当日有效，隔日作废】\n");
                sb.Append($"终端流水：{tranNo}\n");
                sb.Append($"就诊时间：{DateTimeCore.Now.ToString("yyyy-MM-dd")} {time}\n");
                sb.Append($"就诊序号：{res?.全员挂号序号}\n");
                sb.Append($"门诊序号：{res?.挂号序号}\n");
                sb.Append($"姓    名：{patientInfo?.name}\n");
                sb.Append($"就诊卡号：{patientInfo?.patientId}\n");
                sb.Append($"类    别：{RegTypesModel.SelectRegType.RegType.GetRegType()?.Name}\n");
                sb.Append($"--------------------------------\n");
                sb.Append($"科    室：{department?.deptName}  {res?.就诊地点}\n");
                if (RegTypesModel.SelectRegType.RegType == RegType.专家门诊)
                {
                    sb.Append($"医    生：{schedule?.doctName}\n");
                }
                else
                {
                    sb.Append($"医    生：无\n");
                }
                sb.Append($"就 诊 号：{res?.就诊序号}\n");
                var cardtype = CardModel?.CardType == CardType.社保卡 ? CardType.社保卡.ToString() : "自费卡";
                sb.Append($"卡 类 型：{cardtype}\n");
                sb.Append($"挂 号 费：{PaymentModel.Total.In元()}\n");
                sb.Append($"个人支付：{PaymentModel.Self.In元()}\n");
                sb.Append($"医保支付：{PaymentModel.Insurance.In元()}\n");
                sb.Append($"其他支付：0.00元\n");
                sb.Append($"--------------------------------\n");
                sb.Append($"打印时间：{DateTimeCore.Now:HH:mm:ss}\n");
                sb.Append($"机器号：{FrameworkConst.OperatorId}\n");
                sb.Append($"凭本条就诊！\n");
                sb.Append($"请在当日就诊，隔日作废！\n");

                var timep = (SourceModel?.所选号源?.appoNo.Split('-')[1]).BackNotNullOrEmpty("00:00");

                var tmptime = DateTime.ParseExact(timep, "HH:mm", null, DateTimeStyles.None);
                var realTime = DateTimeCore.Today.Add(tmptime - tmptime.Date);

                if (realTime > DateTimeCore.Now)
                {
                    sb.Append($"请于{DateTimeCore.Now.ToString("yyyy-MM-dd")}{SourceModel?.所选号源?.appoNo.Split('-')[1]} 前到 {department?.deptName}  {res?.就诊地点} 候诊\n");
                }
                else
                {
                    sb.Append($"请于{DateTimeCore.Now.AddMinutes(10).ToString("yyyy-MM-dd HH:mm")} 前到 {department?.deptName}  {res?.就诊地点} 候诊\n");
                }
                if (RegTypesModel.SelectRegType.RegType == RegType.急诊门诊)
                {
                    sb.Append($"如需要发票请携带就诊卡和此凭条到咨询台打印\n");
                }
                else
                {
                    sb.Append($"如需要发票请携带就诊卡和此凭条到导引台打印\n");
                }
                queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 9, FontStyle.Regular) });
                sb.Clear();
                if (!string.IsNullOrEmpty(patientInfo.patientId))
                {
                    var image = BarCode128.GetCodeImage(patientInfo.patientId, BarCode.Code128.Encode.Code128A);
                    queue.Enqueue(new PrintItemImage
                    {
                        Align = ImageAlign.Center,
                        Image = image,
                        Height = image.Height / 2f,
                        Width = image.Width / 2f
                    });
                }
                sb.Append(patientInfo?.patientId);
                queue.Enqueue(new PrintItemText { Text = sb.ToString(), StringFormat = PrintConfig.Center, Font = new Font("微软雅黑", 9, System.Drawing.FontStyle.Regular) });
                sb.Clear();
                sb.AppendLine(".");
                sb.AppendLine(".");
                queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 9, FontStyle.Regular) });
                return queue;
            }
            catch (Exception ex)
            {
                Logger.Net.Info($"[嵊州本地服务，挂号结算] 构造打印凭条异常 {ex.Message}");
                return null;
            }

        }

        private void PrintRefundFailed(string reason)
        {
            var queue = PrintManager.NewQueue("挂号单边账");
            var register = RegisterModel.Res预约挂号.data;
            var department = DepartmentModel.所选科室;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var sb = new StringBuilder();
            sb.Append($"就诊日期：{RegDateModel.RegDate}\n");
            sb.Append($"就诊时间：{SourceModel.所选号源?.appoNo}\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"就诊卡号：{patientInfo.patientId}\n");
            sb.Append($"就诊序号：{register.appoNo}\n");
            sb.Append($"科室名称：{department.deptName}\n");
            sb.Append($"科室编号：{department.deptCode}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请执凭条与医护工作人员联系\n");
            sb.Append($"{reason}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText() { Text = sb.ToString() });

            var printName = ConfigurationManager.GetValue("Printer:Receipt");

            PrintManager.QuickPrint(printName, queue);

        }
        protected static readonly BarCode.Code128 BarCode128 = new BarCode.Code128
        {
            Magnify = 1,
            Height = 80
        };

        private void 上传数据到HIS()
        {
            try
            {
                var medAPM = DateTimeCore.Now.JudgeAmPm();
                var req = new req预约挂号记录同步到his系统
                {
                    regMode = "2",
                    cardType = CardModel.CardType.ToString(),
                    idNo = PatientModel.当前病人信息.idNo,
                    patientName = PatientModel.当前病人信息.name,
                    phone = PatientModel.当前病人信息.phone,
                    medAmPm = medAPM.ToString(),
                    medDate = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                    medTime = DateTimeCore.Now.ToString("HH:mm:ss"),
                    regType = RegTypesModel.SelectRegType.RegType.ToString(),
                    deptCode = DepartmentModel.所选科室.deptCode,
                    deptName = DepartmentModel.所选科室.deptName,
                    doctCode = DoctorModel?.所选医生?.doctCode ?? "无",
                    cash = PaymentModel.Total.ToString("0")
                };
                FillRechargeRequest(req);
                var res = DataHandlerEx.预约挂号记录同步到his系统(req);
                if (res.success)
                {
                    Logger.Net.Info($"预约挂号记录同步到his系统成功");
                }
                else
                {
                    Logger.Net.Info($"预约挂号记录同步到his系统失败:{res.msg}");
                }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"预约挂号记录同步到his系统失败异常:{ex.Message}");
                return;
            }
        }
        private void 自费交易记录同步到his系统()
        {
            try
            {

                var req = new req交易记录同步到his系统
                {
                    platformId = FrameworkConst.OperatorId,
                    hisPatientId = CardModel?.CardNo,
                    cardNo = CardModel?.CardNo,
                    idNo = PatientModel?.当前病人信息?.idNo,
                    patientName = PatientModel?.当前病人信息?.name,
                    tradeType = "2",
                    cash = PaymentModel?.Self.ToString("0"),
                    tradeMode = PaymentModel.PayMethod == PayMethod.未知 ? "医保支付" : GetEnumDescription(PaymentModel.PayMethod),
                    operId = FrameworkConst.OperatorId,
                    bankTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    inHos = "1",
                    remarks = "挂号",
                };
                FillRechargeRequest(req);
                var res = DataHandlerEx.交易记录同步到his系统(req);

            }
            catch (Exception ex)
            {
                Logger.Main.Error($"交易记录同步到his系统失败异常:{ex.Message}");
                return;
            }
        }

        private void 医保交易记录同步到his系统(string tranNo)
        {
            try
            {
                var req = new req交易记录同步到his系统
                {
                    platformId = FrameworkConst.OperatorId,
                    hisPatientId = CardModel?.CardNo,
                    cardNo = CardModel?.CardNo,
                    idNo = PatientModel?.当前病人信息?.idNo,
                    patientName = PatientModel?.当前病人信息?.name,
                    tradeType = "2",
                    cash = PaymentModel?.Insurance.ToString("0"),
                    operId = FrameworkConst.OperatorId,
                    bankTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tradeMode = "医保支付",
                    inHos = "1",
                    remarks = "挂号",
                    payAccountNo = "医保账户",
                    tradeTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    transNo = tranNo
                };
                var res = DataHandlerEx.交易记录同步到his系统(req);
                if (res.success)
                {
                    Logger.Net.Info($"交易记录同步到his系统成功");
                }
                else
                {
                    Logger.Net.Info($"交易记录同步到his系统失败:{res.msg}");
                }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"交易记录同步到his系统失败异常:{ex.Message}");
                return;
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

        protected virtual void FillRechargeRequest(req预约挂号记录同步到his系统 req)
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
                }
            }
        }

        protected virtual void FillRechargeRequest(req交易记录同步到his系统 req)
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
                }
            }
        }
        private string 时间处理(string medAmPm)
        {
            if (medAmPm == null)
            {
                return "";
            }
            switch (medAmPm)
            {
                case "1": return "上午";
                case "2": return "下午";
                case "3": return "全天";
                default: return "";
            }
        }
    }
}
