using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.YiWuArea.Commons;
using YuanTu.YiWuArea.Insurance.Dtos;
using YuanTu.YiWuArea.Insurance.Models;
using YuanTu.YiWuArea.Insurance.Services;
using YuanTu.YiWuFuBao.Component.TakeNum.Models;
using YuanTu.YiWuFuBao.Models;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Register;
using YuanTu.Core.Gateway;
using YuanTu.Core.Navigating;
using YuanTu.Core.Systems;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.YiWuArea;
using YuanTu.YiWuArea.Models;
using YuanTu.YiWuFuBao.Common;
using CardModel = YuanTu.YiWuFuBao.Models.CardModel;
using RegisterModel = YuanTu.YiWuFuBao.Component.Register.Models.RegisterModel;

namespace YuanTu.YiWuFuBao.Component.TakeNum.ViewModels
{
    public class TakeNumViewModel: YuanTu.Default.Component.TakeNum.ViewModels.TakeNumViewModel
    {
        [Dependency]
        public ISipayService SipayService { get; set; }
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }


        public override void OnEntered(NavigationContext navigationContext)
        {
            if (ConstInner.门诊挂号结算暂存 != null)
            {
                DoCommand(lp =>
                {
                    ConstInner.暂存退号();
                }, false);
            }
            base.OnEntered(navigationContext);
        }

        protected override void ConfirmAction()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在获取费用信息，请稍后...");
                var record = RecordModel.所选记录;
                var patientInfo = PatientModel.当前病人信息;
                ChangeNavigationContent(record.doctName);
                var tm = TakeNumModel as TakeNumModel;
                tm.Req预约取号 = new req预约取号
                {
                    patientId = patientInfo.patientId,
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,

                    appoNo = RecordModel.所选记录.appoNo,
                    //searchType = ((int)regMode.预约).ToString(),
                    orderNo = RecordModel.所选记录.orderNo,
                    operId = FrameworkConst.OperatorId,
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    accountNo = patientInfo.patientId,
                    cash = PaymentModel.Total.ToString(),
#pragma warning disable 612
                    medDate = record.medDate,
                    scheduleId = record.scheduleId,
                    medAmPm = record.medAmPm
#pragma warning restore 612
                };
               var qh= tm.Res预约取号= DataHandlerEx.预约取号(TakeNumModel.Req预约取号);
              
                if (!tm.Res预约取号.success)
                {
                    ShowAlert(false, "取号失败", $"取号失败！\r\n{tm.Res预约取号.msg}");
                    return;
                }

                ConstInner.门诊挂号结算暂存 = new RegisterInfoTombDto()
                {
                    CardNo = CardModel.CardNo,
                    CardType = CardModel.CardType,
                    病人信息 = patientInfo,
                    MedDate = record.medDate,
                    MedAmPm = record.medAmPm,
                    ScheduleId = record.scheduleId,
                    //挂号信息 = tm.Res预约取号
                    appoNo = tm.Res预约取号.data.appoNo,
                    //orderNo = tm.Res预约取号.data.,
                    visitNo = tm.Res预约取号.data.visitNo,
                    //regFlowId = tm.Res预约取号.data.regFlowId,
                    extend = tm.Res预约取号.data.extend,
                };
                tm.Req获取缴费概要信息 = new req获取缴费概要信息
                {
                    patientId = patientInfo.patientId,
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,
                    billType = "",
                    extend = tm.Res预约取号.data.extend
                };
                tm.Res获取缴费概要信息 = DataHandlerEx.获取缴费概要信息(tm.Req获取缴费概要信息);
                if (!tm.Res获取缴费概要信息.success)
                {
                    ShowAlert(false, "取号失败", $"取号费用信息获取失败！\r\n{tm.Res获取缴费概要信息.msg}");
                    ConstInner.暂存退号(); //退号
                    return;
                }
                if (CardModel.CanUseInsurance())
                {
                    var ret = PerPayWithSi(lp);
                    if (!ret.IsSuccess)
                    {
                        lp.ChangeText("社保计费失败，取消取号...");
                        ConstInner.暂存退号(); //退号
                    }
                }
                else
                {
                    try
                    {
                        var totalFee = tm.Res获取缴费概要信息.data.First().billFee;
                        PaymentModel.Self = decimal.Parse(totalFee).TrimToFen();
                        PaymentModel.Insurance = 0m;
                        PaymentModel.Total = decimal.Parse(totalFee).TrimToFen(); 
                        PaymentModel.NoPay = PaymentModel.Self == 0;
                        PaymentModel.ConfirmAction = Confirm;

                        PaymentModel.LeftList = new List<PayInfoItem>()
                        {
                            new PayInfoItem("日期：", record.medDate),
                            new PayInfoItem("时间：", record.medAmPm.SafeToAmPm()),
                            new PayInfoItem("科室：", record.deptName),
                            new PayInfoItem("医生：", record.doctName),
                        };

                        PaymentModel.RightList = new List<PayInfoItem>()
                        {
                            new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                            new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                            new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true),
                        };

                        Next();
                    }
                    catch (Exception ex)
                    {
                        Logger.Main.Error("[取号]取号时异常，" + ex);
                        ShowAlert(false, "挂号失败", "由于挂号数据存在问题，挂号查询失败，请到窗口处理！");
                        ConstInner.暂存退号(); //退号
                    }
                }

            });

        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                var tm = TakeNumModel as TakeNumModel;
                var payfee = tm.Res获取缴费概要信息.data.First();
                var extendInfo = payfee.extend + $"<remarks>取号</remarks>";

                if (CardModel.CanUseInsurance() )
                {
                    lp.ChangeText("正在进行社保挂号，请稍候...");
                    var siRest = PayWithSi(lp);
                    if (!siRest.IsSuccess)
                    {
                        return siRest;
                    }
                    var cm = CardModel as CardModel;
                    var extraPaymentModel = GetInstance<IExtraPaymentModel>();


                    var payInfo =
                        $"<JIUZHENID>{tm.Res缴费预结算.data.balBillNo}</JIUZHENID><SelfPayMode>{extraPaymentModel.CurrentPayMethod.GetEnumDescription()}</SelfPayMode><SelfPay>{PaymentModel.Self}</SelfPay><InsurancePay>{PaymentModel.Insurance}</InsurancePay><AmountPay>{PaymentModel.Total}</AmountPay><InsurancePayFlowId>{tm.Res社保门诊结算?.结算流水号}</InsurancePayFlowId><InsurancePayTime>{tm.Res社保门诊结算.结算时间}</InsurancePayTime><InsurancePayAccount>{CardModel.CardNo}</InsurancePayAccount>";
                    var ratiostr = string.Join("||", tm.所有自负比例列表?.Select(p => p.ToString()) ?? new string[0]);
                    extendInfo = payfee.extend + $"<PayRatio>{ratiostr}</PayRatio>" + $"<remarks>取号</remarks>" +
                                 ($"<YIBAOJSCCXX>{tm.Res社保门诊结算.报文出参}</YIBAOJSCCXX><YIBAOYWZQH>{tm.Req社保门诊结算.对账流水号}</YIBAOYWZQH><YIBAOKXX>{cm.参保人员信息.报文出参}</YIBAOKXX>") +
                                 payInfo;

                }
                var patientInfo = PatientModel.当前病人信息;
                lp.ChangeText("正在进行取号，请稍候...");
                tm.Req缴费结算 = new req缴费结算
                {
                    patientId = patientInfo.patientId,
                    cardType = ((int) CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,
                    operId = FrameworkConst.OperatorId,
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    cash = PaymentModel.Self.ToString("0"),
                    accountNo = patientInfo.patientId,
                    billNo = payfee.billNo,
                    allSelf = PaymentModel.Insurance == 0 ? "1" : "0",
                    extend = extendInfo,
                    // balBillNo = localModel.Res缴费预结算?.data?.balBillNo
                };

                FillResigterRequest(tm.Req缴费结算);
                if (CardModel.CanUseInsurance())
                {
                    tm.Req缴费结算.tradeMode = PayMethod.社保.GetEnumDescription();
                }
                tm.Res缴费结算 = DataHandlerEx.缴费结算(tm.Req缴费结算);
               
                if (tm.Res缴费结算?.success ?? false)
                {
                    ExtraPaymentModel.Complete = true;
                    //if (CardModel.CanUseInsurance())
                    //{
                    //    lp.ChangeText("取号成功，正在发起社保确认...");
                    //    PayConfirm(tm?.Res社保门诊结算, true, "取号成功");
                    //}
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "取号成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分取号",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = TakeNumPrintables(),
                        TipImage = "提示_凭条"
                    });
                    ConstInner.门诊挂号结算暂存 = null; //结算成功，干掉墓碑
                    Navigate(A.QH.Print);
                    return Result.Success();
                }
                var code = tm.Res缴费结算?.code??0;
                if (DataHandler.UnKnowErrorCode.Contains(code))//出现单边账了
                {
                    ConstInner.门诊挂号结算暂存 = null; //结算未知，为防止损失，干掉墓碑
                    if (CardModel.CanUseInsurance())//打印社保单边账凭条
                    {
                       // PayConfirm(tm?.Res社保门诊结算, true, $"网关返回未知结果{code}，保险起见先行扣费");
                        var errorMsg = $"社保扣费成功，网关返回未知结果{code}，打印凭条结束交易！\n请执凭条到人工咨询此交易结果！";
                        var queue = PrintManager.NewQueue($"社保单边账");
                        var sb = new StringBuilder();
                        sb.Append($"状态：{errorMsg}\n");
                        sb.Append($"姓名：{patientInfo.name}\n");
                        sb.Append($"门诊号：{CardModel.CardNo}\n");
                        sb.Append($"交易类型：社保\n");
                        sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                        sb.Append($"社保交易号：{tm.Res社保门诊结算?.结算流水号}\n");
                        sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
                        sb.Append($"祝您早日康复！\n");
                        queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                        PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), queue);
                        ShowAlert(false, "业务处理异常", errorMsg);
                        Navigate(A.Home);
                    }
                }
                else
                {
                    if (CardModel.CanUseInsurance())
                    {
                        lp.ChangeText("取号失败，正在撤销社保挂号...");
                        PayRefund( tm.Res缴费结算?.msg ?? "取号失败，HIS返回未知");
                    }
                    //第三方支付失败时去支付流程里面处理，不在业务里面处理
                    var navigating = NavigationEngine;
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = "取号失败",
                        DebugInfo = tm.Res缴费结算?.msg
                    });
                    Navigate(A.QH.Print);
                }

                ExtraPaymentModel.Complete = true;
                return Result.Fail(tm.Res缴费结算?.code ?? -100, tm.Res缴费结算?.msg);
            }).Result;
        }

        protected override bool CanConfirm()
        {
            return true; //base.CanConfirm();
        }

        protected virtual void FillResigterRequest(req缴费结算 req)
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
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 ||
                     extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
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
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.智慧医疗)
            {
                var cmf = (TakeNumModel as TakeNumModel).Res交易确认;
                if (cmf != null)
                {
                    req.payAccountNo = CardModel.CardNo;
                    req.transNo = cmf.诊间结算流水号;
                }

            }
        }
        protected virtual Result PerPayWithSi(LoadingProcesser lp)
        {
            var tm = TakeNumModel as TakeNumModel;
#warning 注意判断使用真实社保卡号

            #region[社保操作]

            var ybInfo = tm.Res获取缴费概要信息.data.First().extendBalanceInfo.ToJsonObject<YbOpPayHisInfo>();
            var preInfo = ybInfo.preOpPayJSON.First();
            var record = RecordModel.所选记录;
            var usecache = true;
            while (true) //当从缓存中获取自负比例计算失败后，再次运行从社保平台获取
            {
                lp.ChangeText("正在获取自负比例，请稍后...");
                var it = GetBatchChargeRatio(preInfo.chargeItems, usecache);
                if (!it.IsSuccess)
                {
                    ShowAlert(false, "社保交易失败", it.Message);
                    return Result.Fail(it.Message);
                }
                foreach (var chargeitem in preInfo.chargeItems)
                {
                    var itm = it.Value.FirstOrDefault(p => (p.医嘱明细序号 == chargeitem.docOrderNo));
                    if (itm != null)
                    {
                        chargeitem.selfRatio = itm.自负比列;
                        chargeitem.importSelfRatio = itm.进口类自负比例;
                    }
                }
                lp.ChangeText("正在获取社保费用信息，请稍后...");
                var perId = "1";//预结算就诊Id固定为这个
                var req = new Req门诊预结算()
                {
                    Ic信息 = CardModel.CardNo, // "C09FB5A05"/*"C09B0D3B9"*/,// "C0A06FAA5",
                    收费类型 = "3",
                    门诊号 = perId,// preInfo.outpatientNo,
                    疾病编号 = preInfo.diseaseCode,
                    病种审批号 = preInfo.diseaseApprovalCode,
                    疾病名称 = preInfo.diseaseName,
                    疾病描述 = preInfo.diseaseDesc,
                    本次结算单据张数 = preInfo.billCount,
                    是否需要个帐支付 = preInfo.isPersonPay,
                    对账流水号 = SipayHandler.SiToken,
                    单据列表 = preInfo.billItems.Select(p =>
                        new 预算单据列表()
                        {
                            单据号 = perId,// p.billNo,
                            门诊号 = p.outpatientNo,
                            处方号码 = p.prescriptionNo,
                            就诊日期 = p.visitDate,
                            收费类型 = p.chargeType,
                            科室代码 = "",
                            科室名称 = p.deptName,
                            开方医师身份证号 = p.doctorIdNo,
                            疾病编号 = p.diseaseCode,
                            疾病名称 = preInfo.diseaseName,
                            疾病描述 = p.diseaseDesc,
                            非医保项目总额 = p.noInsuranceFee,
                            收费明细条数 = p.feeCount,
                        }).ToItemList(),
                    收费项目列表 = preInfo.chargeItems.Select(p => new 预算收费项目列表
                    {
                        单据号码 = perId,// p.billNo,
                        药品诊疗类型 = p.treatType,
                        项目医院编号 = p.itemHospCode,
                        项目医院端名称 = p.itemHospName,
                        医院端规格 = p.itemSpecs,
                        医院端剂型 = p.itemLiquid,
                        单复方标志 = p.singleMark,
                        单价 = p.price,
                        数量 = MoneyTools.SafeGetQty(p),
                        单位 = p.itemUnit.BackNotNullOrEmpty("无"),
                        项目总金额 = p.amount,
                        自付比例 = p.selfRatio,
                        进口类自负比例 = p.importSelfRatio,
                        项目包装数量 = p.itemPackageCount,
                        项目最小包装单位 = p.itemMinUnit,
                        每天次数 = p.countOfDay,
                        每次用量 = p.unitOfDay,
                        用量天数 = p.useDay,
                        疾病编码 = p.diseaseCode,
                        项目贴数 = p.itemCount
                    }).ToItemList()
                };


                tm.Req社保门诊预结算 = req;
                var siPreRest = SipayHandler.调用门诊预结算(req);
                tm.Res社保门诊预结算 = siPreRest;

                if ((!siPreRest.IsSuccess) && (siPreRest?.错误信息?.Contains("自负比例") ?? false)) //不成功 而且包含自付比例
                {
                    usecache = false;
                    continue;
                }
                break;
            }

            if (!tm.Res社保门诊预结算.IsSuccess)
            {
                ShowAlert(false, "社保交易失败", tm.Res社保门诊预结算.错误信息);
                return Result.Fail(tm.Res社保门诊预结算.错误信息);
            }

            #endregion

            var 个人现金支付 = (decimal.Parse(tm.Res社保门诊预结算.计算结果信息.个人现金支付) * 100);
            var 合计报销金额 = (decimal.Parse(tm.Res社保门诊预结算.计算结果信息.合计报销金额) * 100);
            var 费用总额 = (decimal.Parse(tm.Res社保门诊预结算.计算结果信息.费用总额) * 100);

            var 民政补助支付 = (decimal.Parse(tm.Res社保门诊预结算.计算结果信息.民政补助支付) * 100);
            var 大病救助支付 = (decimal.Parse(tm.Res社保门诊预结算.计算结果信息.大病救助支付) * 100);

            PaymentModel.Self = (个人现金支付 - 民政补助支付 - 大病救助支付).TrimToFen(); ;
            PaymentModel.Insurance = (合计报销金额 + 民政补助支付 + 大病救助支付).TrimToFen();
            PaymentModel.Total = 费用总额.TrimToFen();
            PaymentModel.NoPay = PaymentModel.Self == 0;
            PaymentModel.ConfirmAction = Confirm;
            if (PaymentModel.NoPay)
            {
                PaymentModel.PayMethod = PayMethod.社保;
            }

            PaymentModel.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("日期：", record.medDate),
                new PayInfoItem("时间：", record.medAmPm.SafeToAmPm()),
                new PayInfoItem("科室：", record.deptName),
                new PayInfoItem("医生：", record.doctName),
            };

            PaymentModel.RightList = new List<PayInfoItem>()
            {
                new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true),
            };

            Next();
            return Result.Success();
        }

        protected virtual Result PayWithSi(LoadingProcesser lp)
        {
            var cm = CardModel as CardModel;
            var tm = TakeNumModel as TakeNumModel;
            #region[社保操作]
            var patientInfo = PatientModel.当前病人信息;
            var record = RecordModel.所选记录;
            var billinfo = tm.Res获取缴费概要信息.data.First();
            var ybInfo = billinfo.extendBalanceInfo.ToJsonObject<YbOpPayHisInfo>();
            var preInfo = ybInfo.preOpPayJSON.First();
            tm.Req缴费预结算 = new req缴费预结算
            {
                patientId = patientInfo.patientId,
                cardType = ((int)CardModel.CardType).ToString(),
                cardNo = CardModel.CardNo,
                operId = FrameworkConst.OperatorId,
                tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                billNo = billinfo.billNo,
                extend = billinfo.extend,
                // balBillNo = localModel.Res缴费预结算?.data?.balBillNo
            };
            lp.ChangeText("正在获取支付序号，请稍后...");
            var res=tm.Res缴费预结算 = DataHandlerEx.缴费预结算(tm.Req缴费预结算);
            if (!res.success)
            {
                ShowAlert(false, "社保交易失败", res.msg);
                return Result.Fail(res.msg);
            }
            var billId = res.data.balBillNo;

            lp.ChangeText("正在确认自负比例，请稍后...");
            var it = GetBatchChargeRatio(preInfo.chargeItems, true);
            if (!it.IsSuccess)
            {
                ShowAlert(false, "社保挂号失败", it.Message);
                return Result.Fail(it.Message);
            }
            tm.所有自负比例列表 = it.Value;
            foreach (var chargeitem in preInfo.chargeItems)
            {
                var itm = it.Value.FirstOrDefault(p => (p.医嘱明细序号 == chargeitem.docOrderNo));
                if (itm != null)
                {
                    chargeitem.selfRatio = itm.自负比列;
                    chargeitem.importSelfRatio = itm.进口类自负比例;
                }
            }

            var req = new Req门诊结算()
            {
                Ic信息 = CardModel.CardNo,//"C09FB5A05",
                收费类型 = "3",
                门诊号 = billId, //preInfo.outpatientNo.BackNotNullOrEmpty(PatientModel.当前病人信息?.patientId),
                疾病编号 = preInfo.diseaseCode,
                病种审批号 = preInfo.diseaseApprovalCode,
                疾病名称 = preInfo.diseaseName,
                疾病描述 = preInfo.diseaseDesc,
                本次结算单据张数 = preInfo.billCount,
                是否需要个帐支付 = preInfo.isPersonPay,
                对账流水号 = SipayHandler.SiToken,
                单据列表 = preInfo.billItems.Select(p =>
                new 结算单据列表()
                {
                    单据号 = billId,// p.billNo,
                    门诊号 = p.outpatientNo,
                    处方号码 = p.prescriptionNo,
                    就诊日期 = DateTimeCore.Now.ToString("yyyy.MM.dd")/*p.visitDate*/,
                    收费类型 = p.chargeType,
                    科室代码 = "",
                    科室名称 = p.deptName.BackNotNullOrEmpty(record?.deptName),
                    医生姓名 = p.doctorIdNo,
                    疾病编号 = p.diseaseCode,
                    疾病名称 = preInfo.diseaseName,
                    疾病描述 = p.diseaseDesc,
                    非医保项目总额 = p.noInsuranceFee,
                    收费明细条数 = p.feeCount,
                }).ToItemList(),
                收费项目列表 = preInfo.chargeItems.Select(p => new 结算收费项目列表
                {
                    单据号码 = billId,// p.billNo,
                    药品诊疗类型 = p.treatType,
                    项目医院编号 = p.itemHospCode,
                    项目医院端名称 = p.itemHospName,
                    医院端规格 = p.itemSpecs,
                    医院端剂型 = p.itemLiquid,
                    单复方标志 = p.singleMark,
                    单价 = p.price,
                    数量 = MoneyTools.SafeGetQty(p),
                    单位 = p.itemUnit.BackNotNullOrEmpty("无"),
                    项目总金额 = p.amount,
                    自付比例 = p.selfRatio,
                    进口类自负比例 = p.importSelfRatio,
                    项目包装数量 = p.itemPackageCount,
                    项目最小包装单位 = p.itemMinUnit,
                    每天次数 = p.countOfDay,
                    每次用量 = p.unitOfDay,
                    用量天数 = p.useDay,
                    疾病编码 = p.diseaseCode,
                    项目贴数 = p.itemCount
                }).ToItemList()
            };
            tm.Req社保门诊结算 = req;
            var siPreRest = SipayHandler.调用门诊结算(req);
            tm.Res社保门诊结算 = siPreRest;
            if (!siPreRest.IsSuccess)
            {
                ShowAlert(false, "社保挂号失败", siPreRest.错误信息);
                return Result.Fail(siPreRest.错误信息);
            }
            lp.ChangeText("正在确认社保交易结果，请稍后...");
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            var retry = 1;
            var retryCount = 3;
            var errMsg = string.Empty;
            while (retry <= retryCount)
            {
                var cmfRet = new Req交易确认()
                {
                    交易类型 = "30",
                    医保交易流水号 = siPreRest.结算流水号,
                    是否需要诊间结算 = (extraPaymentModel.CurrentPayMethod == PayMethod.智慧医疗) ? "1" : "0",
                    HIS事务结果 = "1"
                };
                var cmfres = tm.Res交易确认 = SipayHandler.调用交易确认(cmfRet);
                if (!cmfres.IsSuccess) //确认失败
                {
                    Logger.Main.Error($"[社保交易确认]社保交易确认失败，当前试验次数:{retry}次");
                    lp.ChangeText($"确认失败，正在进行第{retry}次重试...");
                    errMsg = cmfres.错误信息;
                    retry++;
                }
                else
                {
                    break;
                }
            }
            if (retry > retryCount)
            {
                ShowAlert(false, "社保扣费失败", errMsg);
                return Result.Fail(errMsg);
            }


            var ret = SipayService.UploadTradeInfo(new InsuranceTradeRequest()
            {
                TradeName = "门诊取号",
                SiOperatorId = YiWuAreaConsts.SiOperatorNo,
                CardNo = CardModel.CardNo,
                SiToken = req.对账流水号,
                OrderNo = siPreRest.结算流水号,
                TradeCode = "30",
                IcInfo = cm?.参保人员信息.写卡后IC卡数据,
                TradeInput = siPreRest.报文入参,
                TradeRet = 1,
                TradeResult = siPreRest.报文出参,
                IpAddress = NetworkManager.IP,
            });
            tm.TradeId = ret.Value;
            if (!ret.IsSuccess)
            {
                lp.ChangeText("社保交易上传失败，正在撤销...");
                PayRefund(ret.Message);
                return Result.Fail("社保交易上传失败");
            }

            #endregion

            return Result.Success();

        }
        protected virtual Result PayConfirm(Res门诊结算 res, bool issuccrss,string msg)
        {
            try
            {
                if (res == null)
                {
                    return Result.Success();
                }
                return SipayService.ConfirmTradeInfo("30", res.结算流水号, issuccrss,msg);
            }
            catch (Exception ex)
            {
                Logger.Main.Error(ex.Message + " " + ex.StackTrace);
                return Result.Fail(ex.Message);
            }


        }
        private Result<自负比例列表[]> GetBatchChargeRatio(Chargeitem[] arr, bool usecache)
        {

            var allitem = arr.Select(p => new 自负比例项目列表()
            {
                医嘱明细序号 = p.docOrderNo,
                药品诊疗类型 = p.treatType,

                医保编码 = p.itemYBCode,
                医嘱时间 = DateTimeCore.Now.ToString("yyyy.MM.dd"),
                //医嘱时间 = DateTime.ParseExact(p.docOrderTime, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None).ToString("yyyy.MM.dd"),
                限制类标志 = p.restrictedMark,
            }).ToArray();
            var ret = InsurancePayRatioCache.GetFromCache(allitem, lst =>
            {
                var req = new Req批量获取自负比例()
                {
                    医保待遇 = "10",
                    项目列表 = lst.ToItemList()
                };
                var back = SipayHandler.调用批量获取自负比例(req);
                if (back.IsSuccess)
                {
                    return Result<自负比例列表[]>.Success(back.自负比例列表.ToArray());
                }
                return Result<自负比例列表[]>.Fail(back?.错误信息);
            }, usecache);
            return ret;
        }

        protected virtual Result PayRefund(string msg)
        {
            try
            {
                var tm = (TakeNumModel as TakeNumModel);
                var res = tm.Res社保门诊结算;
                var tradeId = tm.TradeId;
                if (res == null)
                {
                    return Result.Success();
                }
                var detail = SipayHandler.调用无卡退费(new Req无卡退费()
                {
                    要作废的结算交易号 = res.结算流水号,
                    经办人 = YiWuAreaConsts.SiOperatorNo,
                });
                if (!detail.IsSuccess)
                {
                    return Result.Fail(detail.错误信息);
                }
                var ret = SipayService.TradeRefund(tradeId, msg);
                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.Main.Error(ex.Message + " " + ex.StackTrace);
                return Result.Fail(ex.Message);
            }

        }
        protected override Queue<IPrintable> TakeNumPrintables()
        {
            var queue = PrintManager.NewQueue("取号单");
            var patientInfo = PatientModel.当前病人信息;
            var takeNum = TakeNumModel.Res预约取号.data;
            var record = RecordModel.所选记录;
            var regType = ConstInner.CurrentRegTypes.ContainsKey(record.extend??"")
                ? ConstInner.CurrentRegTypes[record.extend??""].ToString()
                : "";
            var sb = new StringBuilder();
            sb.Append($"状态：取号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{CardModel.CardNo}\n");
            sb.Append($"交易类型：预约取号\n");
            sb.Append($"科室类别：{regType}\n");
            sb.Append($"科室名称：{record.deptName}\n");
            //sb.Append($"诊疗科室：{paiban.deptName}\n");
            sb.Append($"就诊医生：{record.doctName}\n");
            //sb.Append($"挂号费：{record.regFee.In元()}\n");
            sb.Append($"诊疗费：{record.treatFee.In元()}\n");
            //sb.Append($"挂号金额：{record.regAmount.In元()}\n");
            if (CardModel.CanUseInsurance())
            {
                var localBillPay = (TakeNumModel as TakeNumModel);
                var feeData = localBillPay.Res社保门诊结算?.计算结果信息;
                var accountFee = decimal.Parse(feeData?.当年帐户支付 ?? "0") + decimal.Parse(feeData?.往年帐户支付 ?? "0");
                var baoxiao = decimal.Parse(feeData?.合计报销金额 ?? "0") - accountFee;
                sb.Append($"医保支付：{accountFee.ToString("0.00")}元\n");
                sb.Append($"医保报销：{baoxiao.ToString("0.00")}元\n");
            }
            sb.Append($"个人自费：{PaymentModel.Self.In元()}\n");
            if (PaymentModel.Self > 0)
            {
                sb.Append($"支付方式：{PaymentModel.PayMethod}\n");
            }
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            sb.Append($"就诊场次：{record.medAmPm.SafeToAmPm()}\n");
            sb.Append($"挂号序号：{takeNum?.appoNo}\n");
            sb.Append($"就诊地址：{takeNum?.address}\n");
            sb.Append($"请到分诊台签到\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", (CurrentStrategyType() == DeviceType.Clinic ? 10 : 14), FontStyle.Bold) });
            sb.Clear();
            
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            return queue;
        }
    }
}
