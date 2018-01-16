using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Gateway;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Core.Services.PrintService;
using YuanTu.Core.Systems;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.YiWuArea;
using YuanTu.YiWuArea.Commons;
using YuanTu.YiWuArea.Insurance.Dtos;
using YuanTu.YiWuArea.Insurance.Models;
using YuanTu.YiWuArea.Insurance.Models.Base;
using YuanTu.YiWuArea.Insurance.Services;
using YuanTu.YiWuArea.Models;
using YuanTu.YiWuFuBao.Common;
using YuanTu.YiWuFuBao.Component.BillPay.Models;
using YuanTu.YiWuFuBao.Models;
using CardModel = YuanTu.YiWuFuBao.Models.CardModel;

namespace YuanTu.YiWuFuBao.Component.BillPay.ViewModels
{
    public class BillRecordViewModel: YuanTu.Default.Component.BillPay.ViewModels.BillRecordViewModel
    {
        [Dependency]
        public ISipayService SipayService { get; set; }
      

        protected override void Do()
        {

            BillRecordModel.所选缴费概要 = SelectData.Tag.As<缴费概要信息>();
            ChangeNavigationContent(SelectData.CatalogContent);

           if (CardModel.CanUseInsurance())
            {

                DoCommand(lp =>
                {
                    lp.ChangeText("正在与社保服务交互...");
                    try
                    {
                        PerPayWithSi( lp);
                    }
                    catch (Exception ex)
                    {
                        
                       ShowAlert(false,"",ex.Message);
                    }
                    
                });
                return;
            }
            //自费病人交易


            var recordInfo = BillRecordModel.所选缴费概要;


            PaymentModel.Self = decimal.Parse(recordInfo.billFee).TrimToFen();
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = decimal.Parse(recordInfo.billFee).TrimToFen();
            PaymentModel.NoPay = PaymentModel.Self==0;
            PaymentModel.ConfirmAction = Confirm;

            PaymentModel.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("日期：",recordInfo.billDate?.SafeToSplit(' ')?[0] ?? recordInfo.billDate),
                new PayInfoItem("时间：",recordInfo.billDate?.SafeToSplit(' ')?[1] ?? null),
                new PayInfoItem("科室：",recordInfo.deptName),
                new PayInfoItem("医生：",recordInfo.doctName),
            };

            PaymentModel.RightList = new List<PayInfoItem>()
            {
                new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                new PayInfoItem("医保报销：",PaymentModel.Insurance.In元()),
                new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
            };
            Next();
        }

        protected virtual void PerPayWithSi(LoadingProcesser lp)
        {
            var card = CardModel as CardModel;

            #region[社保操作]
            var ybInfo = BillRecordModel.所选缴费概要.extendBalanceInfo.ToJsonObject<YbOpPayHisInfo>();
            var preInfo = ybInfo.preOpPayJSON.First();
            var localBillPay = (BillPayModel as BillPayModel);
            var usecache = true;
            while (true)//当从缓存中获取自负比例计算失败后，再次运行从社保平台获取
            {
                var it = GetBatchChargeRatio(preInfo.chargeItems, usecache);
                if (!it.IsSuccess)
                {
                    ShowAlert(false, "社保交易失败", it.Message);
                    return;
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
                var perId = "1";//预结算就诊Id固定为这个
                var payType = "3";//门诊结算
                var icInfo = PatientIcData.Deserialize(card.参保人员信息.写卡后IC卡数据).Value;
                var approveNo = preInfo.diseaseApprovalCode;
                if (icInfo.特殊病标志 != "0"&& ybInfo.YILIAOLB=="02")//是特病病人，并且医疗类别为02
                {
                    lp.ChangeText("正在下载特殊病种登记信息，请稍后...");
                    var ret = SipayHandler.调用病种登记信息下载(new Req病种登记信息下载() {Ic信息 = CardModel.CardNo, 病种类别 = "1"});
                    if (!ret.IsSuccess)
                    {
                        ShowAlert(false, "特殊病种信息下载失败", ret.错误信息);
                        return;
                    }
                    if (!ret.登记信息列表.Any())
                    {
                        ShowAlert(false, "特殊病种信息下载失败", "您没有病种登记信息");
                        return;
                    }
                    var now = int.Parse(DateTimeCore.Now.ToString("yyyyMMdd"));
                    var info = ret.登记信息列表.FirstOrDefault(p =>now >= int.Parse(p.有效开始日期??"0")&& now <= int.Parse(p.有效结束日期??"0"));
                    if (info==null)
                    {
                        ShowAlert(false, "特殊病种信息下载失败", "您没有有效的病种登记信息");
                        return;
                    }
                    approveNo = info.病种证书号;
                    payType =info.病种类别=="1"? "5":"6";
                }
                var req = new Req门诊预结算()
                {
                    Ic信息 = CardModel.CardNo,
                    收费类型 = payType,
                    门诊号 = perId,
                    疾病编号 = preInfo.diseaseCode,
                    病种审批号 = approveNo,
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
                        数量 = MoneyTools.SafeGetQty(p) ,
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
                
                localBillPay.Req社保门诊预结算 = req;
                var siPreRest = SipayHandler.调用门诊预结算(req);
                localBillPay.Res社保门诊预结算 = siPreRest;
               
                if ((!siPreRest.IsSuccess)&&(siPreRest?.错误信息?.Contains("自负比例")??false))//不成功 而且包含自付比例
                {
                    usecache = false;
                    continue;
                }
                break;
            }
           
            if (!localBillPay.Res社保门诊预结算.IsSuccess)
            {
                ShowAlert(false, "社保交易失败", localBillPay.Res社保门诊预结算.错误信息);
                return;
            }
            #endregion

            var 个人现金支付 = (decimal.Parse(localBillPay.Res社保门诊预结算.计算结果信息.个人现金支付) * 100);
            var 合计报销金额 = (decimal.Parse(localBillPay.Res社保门诊预结算.计算结果信息.合计报销金额) * 100);
            var 费用总额 = (decimal.Parse(localBillPay.Res社保门诊预结算.计算结果信息.费用总额) * 100);

            var 民政补助支付 = (decimal.Parse(localBillPay.Res社保门诊预结算.计算结果信息.民政补助支付) * 100);
            var 大病救助支付 = (decimal.Parse(localBillPay.Res社保门诊预结算.计算结果信息.大病救助支付) * 100);

            PaymentModel.Self = (个人现金支付- 民政补助支付- 大病救助支付).TrimToFen(); ;
            PaymentModel.Insurance = (合计报销金额+ 民政补助支付+ 大病救助支付).TrimToFen();
            PaymentModel.Total = 费用总额.TrimToFen();
            PaymentModel.NoPay = PaymentModel.Self==0;
            PaymentModel.ConfirmAction = Confirm;
            if (PaymentModel.NoPay)
            {
                PaymentModel.PayMethod = PayMethod.社保;
            }
            var recordInfo = BillRecordModel.所选缴费概要;
            PaymentModel.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("日期：",recordInfo.billDate?.SafeToSplit(' ')?[0] ?? recordInfo.billDate),
                new PayInfoItem("时间：",recordInfo.billDate?.SafeToSplit(' ')?[1] ?? null),
                new PayInfoItem("科室：",recordInfo.deptName),
                new PayInfoItem("医生：",recordInfo.doctName),
            };

            PaymentModel.RightList = new List<PayInfoItem>()
            {
                new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                new PayInfoItem("医保报销：",PaymentModel.Insurance.In元()),
                new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
            };
            Next();
          
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                try
                {

                    var localBillPay = (BillPayModel as BillPayModel);
                    var patientInfo = PatientModel.当前病人信息;
                    var record = BillRecordModel.所选缴费概要;
                    var extendInfo = record.extend + $"<remarks>缴费</remarks>";
                    if (CardModel.CanUseInsurance())
                    {
                        lp.ChangeText("正在进行社保扣费，请稍候...");
                        var siRest = PayWithSi(lp);
                        if (!siRest.IsSuccess)
                        {
                            return siRest;
                        }
                        var cm = CardModel as CardModel;
                        var extraPaymentModel = GetInstance<IExtraPaymentModel>();
                        
                        var payInfo =
                            $"<JIUZHENID>{localBillPay.Res缴费预结算.data.balBillNo}</JIUZHENID><SelfPayMode>{extraPaymentModel.CurrentPayMethod.GetEnumDescription()}</SelfPayMode><SelfPay>{PaymentModel.Self}</SelfPay><InsurancePay>{PaymentModel.Insurance}</InsurancePay><AmountPay>{PaymentModel.Total}</AmountPay><InsurancePayFlowId>{localBillPay.Res社保门诊结算?.结算流水号}</InsurancePayFlowId><InsurancePayTime>{localBillPay.Res社保门诊结算.结算时间}</InsurancePayTime><InsurancePayAccount>{CardModel.CardNo}</InsurancePayAccount>";
                        var ratiostr = string.Join("||", localBillPay.所有自负比例列表?.Select(p => p.ToString()) ?? new string[0]);
                        extendInfo = record.extend+ $"<PayRatio>{ratiostr}</PayRatio>" + $"<remarks>缴费</remarks>" +
                                     ($"<YIBAOJSCCXX>{localBillPay.Res社保门诊结算.报文出参}</YIBAOJSCCXX><YIBAOYWZQH>{localBillPay.Req社保门诊结算.对账流水号}</YIBAOYWZQH><YIBAOKXX>{cm.参保人员信息.报文出参}</YIBAOKXX>") +
                                     payInfo;
                    }

                    lp.ChangeText("正在进行缴费，请稍候...");
                    BillPayModel.Req缴费结算 = new req缴费结算
                    {
                        patientId = patientInfo.patientId,
                        cardType = ((int)CardModel.CardType).ToString(),
                        cardNo = CardModel.CardNo,
                        operId = FrameworkConst.OperatorId,
                        tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                        cash = PaymentModel.Self.ToString("0"),
                        accountNo = patientInfo.patientId,
                        billNo = record.billNo,
                        allSelf = PaymentModel.Insurance == 0 ? "1" : "0",
                        extend = extendInfo,
                        // balBillNo = localModel.Res缴费预结算?.data?.balBillNo
                    };

                    FillRechargeRequest(BillPayModel.Req缴费结算);//填充支付信息
                    if (CardModel.CanUseInsurance())
                    {
                        BillPayModel.Req缴费结算.tradeMode = PayMethod.社保.GetEnumDescription();
                    }
                    BillPayModel.Res缴费结算 = DataHandlerEx.缴费结算(BillPayModel.Req缴费结算);
                    if (BillPayModel.Res缴费结算?.success ?? false)
                    {
                        ExtraPaymentModel.Complete = true;
                        if (CardModel.CanUseInsurance())
                        {
                            var istebing = localBillPay.Req社保门诊预结算.收费类型 == "5" || localBillPay.Req社保门诊预结算.收费类型 == "6";
                            if (istebing)
                            {
                                var ybInfo = BillRecordModel.所选缴费概要.extendBalanceInfo.ToJsonObject<YbOpPayHisInfo>();
                                if (ybInfo.SHOUFEIWWCBZ=="1")//还有其他费用
                                {
                                    var queue = BillPayPrintables();
                                    var print = GetInstance<IPrintManager>();
                                    print.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), queue);

                                    {
                                        //var billRecordModel = GetInstance<IBillRecordModel>();
                                        //var patientModel = GetInstance<IPatientModel>();
                                        //var cardModel = GetInstance<ICardModel>();
                                        lp.ChangeText("正在查询待缴费信息，请稍候...");
                                        BillRecordModel.Req获取缴费概要信息 = new req获取缴费概要信息
                                        {
                                            patientId =
                                                PatientModel.Res病人信息查询?.data[PatientModel.PatientInfoIndex].patientId,
                                            cardType = ((int) CardModel.CardType).ToString(),
                                            cardNo = CardModel.CardNo,
                                            billType = ""
                                        };
                                        BillRecordModel.Res获取缴费概要信息 = DataHandlerEx.获取缴费概要信息(BillRecordModel.Req获取缴费概要信息);
                                        if (BillRecordModel.Res获取缴费概要信息?.success ?? false)
                                        {
                                            if (BillRecordModel.Res获取缴费概要信息?.data?.Count > 0)
                                            {
                                                NavigationEngine.Navigate(A.JF.BillRecord);
                                            }
                                            else
                                            {
                                                NavigationEngine.Navigate(NavigationEngine.HomeAddress);
                                            }
                                        }
                                    }
                                    return Result.Success();
                                }
                            }
                        }
                        PrintModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = "缴费成功",
                            TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分缴费",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = BillPayPrintables(),
                            TipImage = "提示_凭条"
                        });
                        Navigate(A.JF.Print);
                        return Result.Success();
                    }
                    var code = BillPayModel.Res缴费结算?.code ?? 0;
                    if (DataHandler.UnKnowErrorCode.Contains(code))//出现单边账了
                    {
                        ConstInner.门诊挂号结算暂存 = null; //结算未知，为防止损失，干掉墓碑
                        if (CardModel.CanUseInsurance())//打印社保单边账凭条
                        {
                            var errorMsg = $"社保扣费成功，网关返回未知结果{code}，打印凭条结束交易！\n请执凭条到人工咨询此交易结果！";
                            var queue = PrintManager.NewQueue($"社保单边账");
                            var sb = new StringBuilder();
                            sb.Append($"状态：{errorMsg}\n");
                            sb.Append($"姓名：{patientInfo.name}\n");
                            sb.Append($"门诊号：{CardModel.CardNo}\n");
                            sb.Append($"交易类型：社保\n");
                            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                            sb.Append($"社保交易号：{localBillPay.Res社保门诊结算?.结算流水号}\n");
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
                            lp.ChangeText("缴费失败，正在撤销社保缴费...");
                            PayRefund(BillPayModel.Res缴费结算?.msg ?? "HIS缴费失败，返回未知错误");
                        }
                        //第三方支付失败时去支付流程里面处理，不在业务里面处理
                        if (NavigationEngine.State != A.Third.PosUnion)
                        {
                            PrintModel.SetPrintInfo(false, new PrintInfo
                            {
                                TypeMsg = "缴费失败",
                                DebugInfo = BillPayModel.Res缴费结算?.msg
                            });
                            Navigate(A.JF.Print);
                        }
                    }
                    ExtraPaymentModel.Complete = true;
                    return Result.Fail(BillPayModel.Res缴费结算?.code ?? -100, BillPayModel.Res缴费结算?.msg);
                }
                catch (Exception ex)
                {
                    ShowAlert(false, "", ex.Message);
                    return Result.Fail(ex.Message);
                }
            }).Result;


        }

        protected override void FillRechargeRequest(req缴费结算 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.智慧医疗)
            {
                var cmf = (BillPayModel as BillPayModel).Res交易确认;
                if (cmf != null)
                {
                    req.payAccountNo = CardModel.CardNo;
                    req.transNo = cmf.诊间结算流水号;
                }

            }
            else
            {
                base.FillRechargeRequest(req);

            }
        }

        protected virtual Result PayWithSi(LoadingProcesser lp)
        {

            var bm = BillPayModel as BillPayModel;
            var cm = CardModel as CardModel;

            #region[社保操作]

            var patientInfo = PatientModel.当前病人信息;
            var record = BillRecordModel.所选缴费概要;
            var ybInfo = BillRecordModel.所选缴费概要.extendBalanceInfo.ToJsonObject<YbOpPayHisInfo>();
            var preInfo = ybInfo.preOpPayJSON.First();

            bm.Req缴费预结算 = new req缴费预结算
            {
                patientId = patientInfo.patientId,
                cardType = ((int) CardModel.CardType).ToString(),
                cardNo = CardModel.CardNo,
                operId = FrameworkConst.OperatorId,
                tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                billNo = record.billNo,
                extend = record.extend,
                // balBillNo = localModel.Res缴费预结算?.data?.balBillNo
            };
            lp.ChangeText("正在获取支付序号，请稍后...");
            var res = bm.Res缴费预结算 = DataHandlerEx.缴费预结算(bm.Req缴费预结算);
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
                ShowAlert(false, "社保交易失败", it.Message);
                return Result.Fail(it.Message);
            }
            bm.所有自负比例列表 = it.Value;
            lp.ChangeText("正在进行社保结算，请稍后...");
            foreach (var chargeitem in preInfo.chargeItems)
            {
                var itm = it.Value.FirstOrDefault(p => (p.医嘱明细序号 == chargeitem.docOrderNo));
                if (itm != null)
                {
                    chargeitem.selfRatio = itm.自负比列;
                    chargeitem.importSelfRatio = itm.进口类自负比例;
                }
            }
            var icInfo = PatientIcData.Deserialize(cm.参保人员信息.写卡后IC卡数据).Value;
            var payType = "3";
            var approveNo = preInfo.diseaseApprovalCode;
            if (icInfo.特殊病标志 != "0" && ybInfo.YILIAOLB == "02")//是特病病人，并且医疗类别为02
            {
                lp.ChangeText("正在下载特殊病种登记信息，请稍后...");
                var djret = SipayHandler.调用病种登记信息下载(new Req病种登记信息下载() { Ic信息 = CardModel.CardNo, 病种类别 = "1" });
                if (!djret.IsSuccess)
                {
                    ShowAlert(false, "特殊病种信息下载失败", djret.错误信息);
                    return Result.Fail(djret.错误信息);
                }
                if (!djret.登记信息列表.Any())
                {
                    ShowAlert(false, "特殊病种信息下载失败", "您没有病种登记信息");
                    return Result.Fail("您没有病种登记信息");
                }
                var now = int.Parse(DateTimeCore.Now.ToString("yyyyMMdd"));
                var info = djret.登记信息列表.FirstOrDefault(p => now >= int.Parse(p.有效开始日期 ?? "0") && now <= int.Parse(p.有效结束日期 ?? "0"));
                if (info == null)
                {
                    ShowAlert(false, "特殊病种信息下载失败", "您没有有效的病种登记信息");
                    return Result.Fail("您没有有效的病种登记信息");
                }
                approveNo = info.病种证书号;
                payType = info.病种类别 == "1" ? "5" : "6";
            }
            var req = new Req门诊结算()
            {
                Ic信息 = CardModel.CardNo, //"C09FB5A05",
                收费类型 = payType,
                门诊号 = billId, // preInfo.outpatientNo,
                疾病编号 = preInfo.diseaseCode,
                病种审批号 = approveNo,
                疾病名称 = preInfo.diseaseName,
                疾病描述 = preInfo.diseaseDesc,
                本次结算单据张数 = preInfo.billCount,
                是否需要个帐支付 = preInfo.isPersonPay,
                对账流水号 = SipayHandler.SiToken,
                单据列表 = preInfo.billItems.Select(p =>
                    new 结算单据列表()
                    {
                        单据号 = billId,
                        门诊号 = p.outpatientNo,
                        处方号码 = p.prescriptionNo,
                        就诊日期 = p.visitDate,
                        收费类型 = p.chargeType,
                        科室代码 = "",
                        科室名称 = p.deptName,
                        医生姓名 = p.doctorIdNo,
                        疾病编号 = p.diseaseCode,
                        疾病名称 = preInfo.diseaseName,
                        疾病描述 = p.diseaseDesc,
                        非医保项目总额 = p.noInsuranceFee,
                        收费明细条数 = p.feeCount,
                    }).ToItemList(),
                收费项目列表 = preInfo.chargeItems.Select(p => new 结算收费项目列表
                {
                    单据号码 = billId,
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
            bm.Req社保门诊结算 = req;
            var siPreRest = SipayHandler.调用门诊结算(req);
            bm.Res社保门诊结算 = siPreRest;
            if (!siPreRest.IsSuccess)
            {
                ShowAlert(false, "社保交易失败", siPreRest.错误信息);
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
                var cmfres = bm.Res交易确认 = SipayHandler.调用交易确认(cmfRet);
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

            lp.ChangeText("上传社保交易数据...");
            var ret = SipayService.UploadTradeInfo(new InsuranceTradeRequest()
            {
                TradeName = "门诊结算",
                SiOperatorId = YiWuAreaConsts.SiOperatorNo,
                CardNo = CardModel.CardNo,
                SiToken = req.对账流水号,
                OrderNo = siPreRest.结算流水号,
                TradeCode = "30",
                IcInfo = cm?.参保人员信息.写卡后IC卡数据,
                TradeInput = siPreRest.报文入参,
                TradeRet = 1,
                TradeResult = siPreRest.报文出参,
                IpAddress = NetworkManager.IP
            });
            bm.TradeId = ret.Value;
            if (!ret.IsSuccess)
            {
                lp.ChangeText("社保交易上传失败，正在撤销...");
                PayRefund(ret.Message);
                return Result.Fail("社保交易上传失败");
            }
            #endregion
            return Result.Success();

        }

        protected virtual Result PayRefund(string msg)
        {
            try
            {
                var localBillPay = (BillPayModel as BillPayModel);
                var res = localBillPay.Res社保门诊结算;
                var tradeId = localBillPay.TradeId;
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


        private Result<自负比例列表[]> GetBatchChargeRatio(Chargeitem[] arr,bool usecache)
        {

            var allitem = arr.Select(p => new 自负比例项目列表()
            {
                医嘱明细序号 = p.docOrderNo,
                药品诊疗类型 = p.treatType,

                医保编码 = p.itemYBCode,
                医嘱时间 = DateTime.ParseExact(p.docOrderTime,"yyyy-MM-dd HH:mm:ss",null,DateTimeStyles.None).ToString("yyyy.MM.dd"),
                限制类标志 = p.restrictedMark,
            }).ToArray();
            // var pt = lst.Where(p => !(p.医保编码.StartsWith("c") && p.医保编码.EndsWith("j"))).ToItemList();
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


        protected override Queue<IPrintable> BillPayPrintables()
        {
            var queue = PrintManager.NewQueue("门诊费用缴费");
            var billPay = BillPayModel.Res缴费结算.data;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var record = BillRecordModel.所选缴费概要;
            var sb = new StringBuilder();
            sb.Append($"状态：缴费成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{CardModel.CardNo}\n");
            sb.Append($"交易类型：自助缴费\n");
            sb.Append($"金额总计：{PaymentModel.Total.In元()}\n");
            if (CardModel.CanUseInsurance())
            {
                var localBillPay = (BillPayModel as BillPayModel);
                var feeData = localBillPay.Res社保门诊结算?.计算结果信息;
                var accountFee = decimal.Parse(feeData?.当年帐户支付??"0") + decimal.Parse(feeData?.往年帐户支付 ?? "0");
                var baoxiao = decimal.Parse(feeData?.合计报销金额 ?? "0") - accountFee;
                sb.Append($"医保支付：{accountFee.ToString("0.00")}元\n");
                sb.Append($"医保报销：{baoxiao.ToString("0.00")}元\n");
            }
            if (PaymentModel.Self > 0)
            {
                sb.Append($"个人自费：{PaymentModel.Self.In元()}\n");
                sb.Append($"支付方式：{PaymentModel.PayMethod}\n");
            }
            //sb.Append($"收据号：{billPay.receiptNo}\n");
            sb.Append($"流水号：{billPay.transNo}\n");
            sb.Append($"发药窗口：{billPay.takeMedWin}\n");
            if (!string.IsNullOrEmpty(billPay.testCode))
            {
                sb.Append($"检验条码：{billPay.testCode}\n");
                queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                sb.Clear();
                var image = BarCode128.GetCodeImage(billPay.testCode, BarCode.Code128.Encode.Code128A);
                queue.Enqueue(new PrintItemImage
                {
                    Align = ImageAlign.Left,
                    Image = image,
                    Height = image.Height / 1.5f,
                    Width = image.Width / 1.5f
                });
            }
            sb.Append($"收费项目明细：\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            queue.Enqueue(new PrintItemTriText("名称", "数量", "金额"));
            if (record?.billItem != null)
            {
                foreach (var detail in record.billItem)
                {
                    var realfee = 0m;
                    if (decimal.TryParse(detail.billFee,out realfee)&&realfee==0)
                    {
                        continue;
                    }
                    queue.Enqueue(new PrintItemTriText(detail.itemName, detail.itemQty, detail.billFee.InRMB()));
                }
            }
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的缴费凭证，遗失不补，如需发票，请到人工收费窗口打印\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }

       
    }
}
