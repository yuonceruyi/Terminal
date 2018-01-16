using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.DischargeSettlement;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Recharge;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Advertisement;
using YuanTu.Core.Advertisement.Data;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Devices.CardReader;
using YuanTu.YiWuArea.Dialog;
using YuanTu.YiWuArea.Insurance.Models;
using YuanTu.YiWuArea.Insurance.Models.Base;
using YuanTu.YiWuFuBao.Common;
using YuanTu.YiWuFuBao.Component.ChuYuan.Models;
using YuanTu.YiWuFuBao.Dtos;
using YuanTu.YiWuFuBao.Models;

namespace YuanTu.YiWuFuBao.Component.ViewModels
{
    public class ChoiceViewModel : YiWuArea.Component.ViewModels.ChoiceViewModel
    {
        private VideoPlayerState _videoPlayerState;
        private Uri _videoUri;
        private string _mainTips;

        public VideoPlayerState VideoPlayerState
        {
            get { return _videoPlayerState; }
            set
            {
                _videoPlayerState = value;
                OnPropertyChanged();
            }
        }

        public Uri VideoUri
        {
            get { return _videoUri; }
            set
            {
                _videoUri = value;
                OnPropertyChanged();
            }
        }

        public string MainTips
        {
            get { return _mainTips; }
            set { _mainTips = value;OnPropertyChanged(); }
        }

        public override void OnSet()
        {
            base.OnSet();
            if (!string.IsNullOrWhiteSpace(Default.Clinic.Startup.VideoPath))
            {
                var uri = new Uri(Default.Clinic.Startup.VideoPath, UriKind.RelativeOrAbsolute);
                VideoUri = uri;
            }
            if (FrameworkConst.DeviceType.Contains("BG"))
            {
                MainTips = string.Empty;
            }
            else
            {
                MainTips = "选择功能后插卡";
            }
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            if (ConstInner.门诊挂号结算暂存 != null)
            {
                DoCommand(lp =>
                {
                    ConstInner.暂存退号();
                }, false);
            }
           
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            VideoPlayerState = VideoPlayerState.Pause;
            return base.OnLeaving(navigationContext);
        }


        protected override void Do(ChoiceButtonInfo param)
        {
          
            var choiceModel = GetInstance<IChoiceModel>();

            choiceModel.Business = param.ButtonBusiness;
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var result = CheckReceiptPrinter();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "打印机检测", result.Message);
                return;
            }
            var engine = NavigationEngine;
            var fctx=new FormContext(A.ChaKa_Context,ConstInner.IsOldMachine? A.CK.Choice:A.CK.Card);
            switch (param.ButtonBusiness)
            {
                case Business.建档:
                    var config = GetInstance<IConfigurationManager>();
                    if (config.GetValue("SelectCreateType") == "1")
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Select),
                            CreateJump,
                             new FormContext(A.JianDang_Context, AInner.JD.Confirm), param.Name);
                    else
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context,A.CK.IDCard),
                            CreateJump,
                            new FormContext(A.JianDang_Context, A.JD.Print), param.Name);
                    break;

                case Business.挂号:
                    engine.JumpAfterFlow(fctx,
                        RegisterJump,
                        new FormContext(A.XianChang_Context, A.XC.Wether), param.Name);
                    break;

                case Business.预约:
                    engine.JumpAfterFlow(fctx, AppointJump,
                        new FormContext(A.YuYue_Context, A.YY.Date), param.Name);
                    break;

                case Business.取号:
                  
                    engine.JumpAfterFlow(fctx, TakeNumJump,
                        new FormContext(A.QuHao_Context, A.QH.Record), param.Name);
                    break;

                case Business.缴费:
                   
                    engine.JumpAfterFlow(fctx, BillPayJump,
                        new FormContext(A.JiaoFei_Context, A.JF.BillRecord), param.Name);
                    break;

                case Business.充值:
                   
                    engine.JumpAfterFlow(fctx,
                        RechargeJump,
                        new FormContext(A.ChongZhi_Context, A.CZ.RechargeWay), param.Name);
                    break;

                case Business.查询:
                    Navigate(A.QueryChoice);

                    break;
                case Business.住院押金:
                   
                    OnInRecharge(param);
                    break;
                case Business.出院结算:
                    choiceModel.AuthContext = A.ZhuYuan_Context;
                    NavigationEngine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.InPatientNo), ChuYuanBillpay,
                        new FormContext(AInner.ChuYuanBillpay_Context, AInner.ChuYuan.SiCard), param.Name);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override Task<Result<FormContext>> TakeNumJump()
        {
            
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 预约取号");
                var patientModel = GetInstance<IPatientModel>();
                var recordModel = GetInstance<IAppoRecordModel>();
                var cardModel = GetInstance<ICardModel>();
                var takeNumModel = GetInstance<ITakeNumModel>();
                lp.ChangeText("正在查询预约记录，请稍候...");
                recordModel.Req挂号预约记录查询 = new req挂号预约记录查询
                {
                    patientId = patientModel.当前病人信息?.patientId,
                    patientName = patientModel.当前病人信息?.name,
                    startDate = "",
                    endDate = "",
                    searchType = "1",
                    cardNo = cardModel.CardNo,
                    cardType = ((int) cardModel.CardType).ToString()
                };
                recordModel.Res挂号预约记录查询 = DataHandlerEx.挂号预约记录查询(recordModel.Req挂号预约记录查询);
                if (recordModel.Res挂号预约记录查询?.success ?? false)
                {
                    if (recordModel.Res挂号预约记录查询?.data?.Count > 1)
                        return Result<FormContext>.Success(default(FormContext));
                    if (recordModel.Res挂号预约记录查询?.data?.Count == 1)
                    {
                        recordModel.所选记录 = recordModel.Res挂号预约记录查询.data.FirstOrDefault();
                        if (recordModel.所选记录.status != "0")
                        {
                            ShowAlert(false, "预约记录查询", "没有预约记录");
                            return Result<FormContext>.Fail("");
                        }
                        var record = recordModel.所选记录;

                        takeNumModel.List = new List<PayInfoItem>
                        {
                            new PayInfoItem("就诊日期：", record.medDate.SafeConvertToDate("yyyy-MM-dd", "yyyy年MM月dd日")),
                            new PayInfoItem("就诊科室：", record.deptName),
                            new PayInfoItem("就诊医生：", record.doctName),
                            new PayInfoItem("就诊时段：", record.medAmPm.SafeToAmPm()),
                            new PayInfoItem("就诊序号：", record.appoNo),
                            new PayInfoItem("挂号金额：", record.regAmount.In元(), true)
                        };
                        return
                            Result<FormContext>.Success(new FormContext(A.QuHao_Context, A.QH.TakeNum));
                    }
                    ShowAlert(false, "预约记录查询", "没有获得预约记录信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }
                ShowAlert(false, "预约记录查询", "没有获得预约记录信息", debugInfo: recordModel.Res挂号预约记录查询?.msg);
                return Result<FormContext>.Fail("");
            });
        }

        protected override Task<Result<FormContext>> IpRechargeJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 住院预缴金充值");
                var patientModel = GetInstance<IPatientModel>();
                var canRecharge = new string[]{ "入院","预出院"}
                ;
                if (canRecharge.Contains(patientModel.住院患者信息.status ))
                {
                    return Result<FormContext>.Success(default(FormContext));
                }
                else
                {
                    ShowAlert(false, "住院缴押金", $"当前为【{patientModel.住院患者信息.status}】状态，不能缴押金");
                    return Result<FormContext>.Fail("");
                }
            });
        }

        protected virtual Task<Result<FormContext>> ChuYuanBillpay()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在获取出院费用信息");
                var patient = GetInstance<IPatientModel>() as PatientModel;
                var patientInfo = patient.住院患者信息;
                if (patientInfo.status != "预出院")
                {
                    ShowAlert(false, "不能操作", $"您当前的状态为：【{patientInfo.status}】，不能使用出院结算功能");
                    return Result<FormContext>.Fail($"状态为:{patientInfo.status}，不能结算");
                }
                //if (patientInfo.extend.IsNullOrWhiteSpace())
                //{
                //    ShowAlert(false, "不能操作", $"平台未能查询到您的具体费用信息，请到窗口处理！");
                //    return Result<FormContext>.Fail($"extend返回为空");
                //}
                var subInfo = patient.ZhuyuanryxxOut.ZHUYUANRYMX.ZHUYUANXX;
                var model = GetInstance<IDischargeSettlementModel>() as HospitalDischargeSettlementModel;
                if (!subInfo.ZHUYUANDJJYH.IsNullOrWhiteSpace())//医保病人
                {
                    if (subInfo.BINGRENLB != "107")//107为本地医保
                    {
                        ShowAlert(false, "不能操作", $"当前仅支持义乌本地医保的出院结算！");
                        return Result<FormContext>.Fail($"当前仅支持义乌本地医保的出院结算");
                    }
                    //if (model.ZyPatientSubInfoDto.YOUHUILB=="6")//6为分娩结算，暂不支持
                    //{
                    //    ShowAlert(false, "不能操作", $"暂不支持分娩结算病人自助出院！");
                    //    return Result<FormContext>.Fail("暂不支持分娩结算病人自助出院");
                    //}
                    model.YiBaoCardInfoCallback = lp2 =>
                    {
                        ChuYuanPreBillPay(lp2);
                    };
                    return Result<FormContext>.Success(default(FormContext));
                }
                //自费操作
                return ChuYuanPreBillPay(lp);

            });
        }

        protected virtual Result<FormContext> ChuYuanPreBillPay(LoadingProcesser lp)
        {
            var model = GetInstance<IDischargeSettlementModel>() as HospitalDischargeSettlementModel;
            var payment = GetInstance<IPaymentModel>();
            var patient = GetInstance<IPatientModel>() as PatientModel;
            var card = GetInstance<ICardModel>() as CardModel;
            var patientInfo = patient.住院患者信息;
            var subInfo = patient.ZhuyuanryxxOut.ZHUYUANRYMX.ZHUYUANXX;
            model.Req自助出院预结算 = new req自助出院预结算()
            {
                patientId = patientInfo.patientHosId,
            };
            var isSiPay = !subInfo.ZHUYUANDJJYH.IsNullOrWhiteSpace();
           
            if (isSiPay)//社保
            {
                if (subInfo.YOUHUILB == "6") //6为分娩结算，走73号交易
                {
                    if (subInfo.YIBAOFMBZBZ=="0")
                    {
                        lp.ChangeText("正在操作医保住院分娩结算，请稍后...");
                        var req74 = new Req住院分娩结算()
                        {
                            准生证号码 = subInfo.ZHUNSHENGZHM,
                            出生证号码 = subInfo.CHUSHENGZHM,
                            婴儿出生日期 = subInfo.YINGERCSRQ,
                            分娩种类 = subInfo.FENMIANZL,
                            住院总费用 = subInfo.ZHUYUANMQZFY,
                        };
                        var res74 = SipayHandler.调用住院分娩结算(req74);
                        if (!res74.IsSuccess)
                        {
                            ShowAlert(false, "不能操作", res74.错误信息);
                            return Result<FormContext>.Fail(res74.错误信息);
                        }
                        var icInfo = PatientIcData.Deserialize(card.参保人员信息.写卡后IC卡数据);
                        model.Req自助出院预结算.extend = $@"<YIBAOJYH>74</YIBAOJYH>
<YIBAOJYRCXX>{res74.报文入参}</YIBAOJYRCXX>
<YIBAOJYCCXX>{res74.报文出参}</YIBAOJYCCXX>
<YIBAOYWZQH>{SipayHandler.SiToken}</YIBAOYWZQH>
<JIUZHENKH>{icInfo.Value?.个人社保编号}</JIUZHENKH>
<JIUZHENKLX>3</JIUZHENKLX>
<YIBAOKXX>{card.参保人员信息.写卡后IC卡数据}</YIBAOKXX>";

                    }


                }
                else
                {
                    lp.ChangeText("正在获取医保费用信息，请稍后...");
                    var req = model.Req住院预结算 = new Req住院预结算()
                    {
                        Ic信息 = card.参保人员信息.写卡后IC卡数据,
                        结算类型 = "1",
                        转诊转院申请单编号 = "",
                        住院号 = patientInfo.patientHosId,
                        住院登记交易号 = subInfo.ZHUYUANDJJYH,
                        本次结算明细条数 = subInfo.YIBAOFYTS,
                        是否需要个帐支付 = "0",
                    };
                    var resp = model.Res住院预结算 = SipayHandler.调用住院预结算(req);
                    if (!resp.IsSuccess)
                    {
                        ShowAlert(false, "不能操作", resp.错误信息);
                        return Result<FormContext>.Fail(resp.错误信息);
                    }
                    model.Req自助出院预结算.extend = $"<YIBAOJYH>34</YIBAOJYH><YIBAOJYRCXX>{resp.报文入参}</YIBAOJYRCXX><YIBAOJYCCXX>{resp.报文出参}</YIBAOJYCCXX>";

                }
            }
            model.Res自助出院预结算 = DataHandlerEx.自助出院预结算(model.Req自助出院预结算);
            if (!model.Res自助出院预结算.success)
            {
                ShowAlert(false, "不能操作", model.Res自助出院预结算?.msg ?? "网关返回异常，请稍后再试！");
                return Result<FormContext>.Fail(model.Res自助出院预结算?.msg);
            }

            var preData = model.Res自助出院预结算.data;
            var rechargeamount = decimal.Parse(subInfo.YUJIAOKZE.BackNotNullOrEmpty("0"))*100;
            var fee = preData.extend.ToJsonObject<ChuYuanPayFee>();
            if (isSiPay&& subInfo.YOUHUILB!="6")//6为分娩结算 不需要走其他接口
            {
                var 计算结果信息 = model.Res住院预结算.计算结果信息;
                var 个人现金支付 = (decimal.Parse(计算结果信息.个人现金支付) * 100);
                var 合计报销金额 = (decimal.Parse(计算结果信息.合计报销金额) * 100);
                var 费用总额 = (decimal.Parse(计算结果信息.费用总额) * 100);

                var 民政补助支付 = (decimal.Parse(计算结果信息.民政补助支付) * 100);
                var 大病救助支付 = (decimal.Parse(计算结果信息.大病救助支付) * 100);

               
                payment.Self = (个人现金支付 - 民政补助支付 - 大病救助支付).TrimToFen() - decimal.Parse(fee.discountAmount) - rechargeamount; 
                payment.Insurance = (合计报销金额 + 民政补助支付 + 大病救助支付).TrimToFen();
                payment.Total = 费用总额.TrimToFen();
            }
            else
            {
                payment.Self = (decimal.Parse(fee.amount) - decimal.Parse(fee.discountAmount) - rechargeamount).TrimToFen();
                payment.Insurance = decimal.Parse("0");
                payment.Total = payment.Self;
            }
          
            payment.NoPay = payment.Self <= 0;
            payment.ConfirmAction = ChuYuanPayConfirm;

            payment.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("入院日期：",patientInfo.createDate),
                new PayInfoItem("入院科室：",""),
                new PayInfoItem("床位号：",patientInfo.bedNo),
                new PayInfoItem("主治医生：",""),
            };
            
            payment.RightList = new List<PayInfoItem>()
            {

                new PayInfoItem("押金总额：",(subInfo.YUJIAOKZE?.BackNotNullOrEmpty("0")) + "元"/*patientInfo.extend.In元()*/),
                new PayInfoItem("医保报销：",payment.Insurance.In元()),
                //new PayInfoItem("支付金额：",payment.Total.In元(),true),
            };

            if (payment.Self >= 0)
            {
                payment.RightList.Add(new PayInfoItem("支付金额：", payment.Total.In元(), true));
            }
            else
            {
                payment.RightList.Add(new PayInfoItem("退回金额：", payment.Total.In元(), true));
            }

            return Result<FormContext>.Success(
                new FormContext(AInner.ChuYuanBillpay_Context, AInner.ChuYuan.Confirm));
        }

        protected virtual Result ChuYuanPayConfirm()
        {
            return DoCommand(lp =>
            {

                var patient = GetInstance<IPatientModel>() as PatientModel;
                var payment = GetInstance<IPaymentModel>();
                var chuyuanModel = GetInstance<IDischargeSettlementModel>() as HospitalDischargeSettlementModel;
                var patientInfo = patient.住院患者信息;
               // var card = GetInstance<ICardModel>() as CardModel;
                var subInfo = patient.ZhuyuanryxxOut.ZHUYUANRYMX.ZHUYUANXX;
                var isSiPay = !subInfo.ZHUYUANDJJYH.IsNullOrWhiteSpace();

                if (payment.Self < 0) //需要给病人退钱
                {
                    lp.ChangeText("正在获取押金充值记录，请稍后...");
                    var res = DataHandlerEx.住院预缴金充值记录查询(new req住院预缴金充值记录查询()
                    {
                        patientId = patientInfo.patientHosId,
                    });
                    if (!res.success)
                    {
                        ShowAlert(false, "出院结算", res.msg);
                        return Result.Fail(res.code, res.msg);
                    }
                    var canRefund = new[] {PayMethod.支付宝.GetEnumDescription(), PayMethod.微信支付.GetEnumDescription()};
                    var histories = res.data.Where(p => canRefund.Contains(p.tradeMode)).ToArray();
                    var ret = StartRefund(lp, histories, payment.Self);
                    if (!ret)
                    {
                        return ret;
                    }
                }else if (payment.Self > 0)
                {
                    lp.ChangeText("正在充值，请稍后...");
                    var ret = StartRecharge(lp);
                    if (!ret)
                    {
                        return ret;
                    }
                }

                chuyuanModel.Req自助出院结算 = new req自助出院结算()
                {
                    patientId = patientInfo.patientHosId,
                };
                if (isSiPay && subInfo.YOUHUILB != "6")//6为分娩结算 不需要走其他接口
                {
                    lp.ChangeText("正在联系社保出院，请稍后...");
                    var req38 = new Req住院信息变动()
                    {
                        住院号 = patientInfo.patientHosId,
                        住院登记交易号 = subInfo.ZHUYUANDJJYH,
                        出院日期 = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                        病人床号 = subInfo.CHUANGWEIH,
                        诊断医生姓名 = subInfo.ZHUZHIYISHENG,
                        诊断描述 = subInfo.CHUYUANZDMC,
                        疾病编号 = subInfo.CHUYUANZDDM,
                        疾病名称 = subInfo.CHUYUANZDMC,
                        科室名称 = patientInfo.deptName,
                        科室编号 = "0",
                        出院原因 = "1",
                        变动类型 = "2"
                    };
                    var req38_hg = new Req住院信息变动()
                    {
                        住院号 = patientInfo.patientHosId,
                        住院登记交易号 = subInfo.ZHUYUANDJJYH,
                        出院日期 = "",
                        病人床号 = subInfo.CHUANGWEIH,
                        诊断医生姓名 = subInfo.RUYUANSZYSXM,
                        诊断描述 = subInfo.RUYUANZDMC,
                        疾病编号 = subInfo.RUYUANZDDM,
                        疾病名称 = subInfo.RUYUANZDMC,
                        科室名称 = patientInfo.deptName,
                        科室编号 = "0",
                        出院原因 = "",
                        变动类型 = "1"
                    };
                    var bdRet = SipayHandler.调用住院信息变动(req38);
                    if (!bdRet.IsSuccess)
                    {
                        ShowAlert(false, "社保交易失败", bdRet.错误信息);
                        //return Result.Fail(bdRet.错误信息);
                        return Result.Success();//返回为True，来保证三方支付不会退钱
                    }
                    lp.ChangeText("正在社保出院结算，请稍后...");
                    var req= chuyuanModel.Req出院结算= new Req出院结算()
                    {
                        结算类型 = "1",
                        转诊转院申请单编号 = "",
                        住院号 = patientInfo.patientHosId,
                        住院登记交易号 = subInfo.ZHUYUANDJJYH,
                        本次结算明细条数 = subInfo.YIBAOFYTS,
                        是否需要个帐支付 = "0",
                    };
                    var siRest = SipayHandler.调用出院结算(req);
                    if (siRest.IsSuccess)
                    {
                        lp.ChangeText("出院失败，正在回滚状态，请稍后...");
                        bdRet = SipayHandler.调用住院信息变动(req38_hg);
                        ShowAlert(false, "社保交易失败", siRest.错误信息);
                        //return Result.Fail(siRest.错误信息);
                        return Result.Success();//返回为True，来保证三方支付不会退钱
                    }
                    var retry = 1;
                    var retryCount = 3;
                    var errMsg = string.Empty;
                    lp.ChangeText("正在确认社保出院结果，请稍后...");

                    while (retry <= retryCount)
                    {
                        var cmfRet = new Req交易确认()
                        {
                            交易类型 = "30",
                            医保交易流水号 = siRest.住院结算交易交流水号,
                            是否需要诊间结算 =  "0",
                            HIS事务结果 = "1"
                        };
                        var cmfres = chuyuanModel.Res交易确认 = SipayHandler.调用交易确认(cmfRet);
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
                        lp.ChangeText("出院状态确认失败，正在回滚，请稍后...");
                        bdRet = SipayHandler.调用住院信息变动(req38_hg);
                        ShowAlert(false, "社保出院失败", errMsg);
                        //return Result.Fail(errMsg);
                        return Result.Success();//返回为True，来保证三方支付不会退钱
                    }
                  
                }

                var chuyuanresp = chuyuanModel.Res自助出院结算 = DataHandlerEx.自助出院结算(chuyuanModel.Req自助出院结算);
                if (!chuyuanresp.success)
                {
                    ShowAlert(false,"自助出院",chuyuanresp.msg??"出院发生异常，请稍后再试！", extend: new AlertExModel()
                    {
                        HideCallback = tp =>
                        {
                            Navigate(A.Home);//导航回家
                        }
                    });
                    //return Result.Fail(chuyuanresp.msg);//不能传Code，因为钱已经充到账户里面了
                    return Result.Success();//返回为True，来保证三方支付不会退钱
                }
                var pm = GetInstance<IPrintModel>();
                var config = GetInstance<IConfigurationManager>();
                pm.SetPrintInfo(true, new PrintInfo
                {
                    TypeMsg = "自助出院成功",
                    TipMsg = $"您已于{DateTimeCore.Now:HH:mm}分成功出院",
                    TipImage = "提示_凭条",
                    PrinterName = config.GetValue("Printer:Receipt"),
                    Printables = ChuYuanPrintables()
                });
                Navigate(AInner.ChuYuan.Print);
                return Result.Success();
            }).Result;

        }

        private Result StartRefund(LoadingProcesser lp,住院充值记录[] records, decimal amount)
        {
            var patient = GetInstance<IPatientModel>();
            var payment = GetInstance<IPaymentModel>();
            var patientInfo = patient.住院患者信息;
            var refundAmount = Math.Abs(amount);

            var orders = records.Select(p => new{Item=p ,cash = decimal.Parse(p.cash) }).ToArray();

            var refunds = orders.Where(p => p.cash < 0).ToArray();
            var realDicts = orders.Where(p => p.cash > 0).Select(p =>
            {
                var rm = refunds.Where(q => q.Item.receiptNo == p.Item.receiptNo).Sum(q=>q.cash);
                return new {Item = p.Item, cash = p.cash+rm};
            }).ToArray();

            if (realDicts.Sum(p=>p.cash) < refundAmount)//可以退
            {
                ShowAlert(false, "出院结算", "您没有充足的余额用于退费，请到窗口操作！");
                return Result.Fail("");
            }
            lp.ChangeText("正在退费，请稍后...");
            var refundLeft = refundAmount;
            for (int i = 0; i < realDicts.Length; i++)
            {
                var min = Math.Min(refundLeft, realDicts[i].cash);

                var req = new req住院预缴金充值
                {
                    patientId = patientInfo.patientHosId,
                    accountNo = patientInfo.patientHosId,
                    operId = FrameworkConst.OperatorId,
                    cash = (0 - min).ToString(),
                    tradeMode = realDicts[i].Item.tradeMode,
                    outTradeNo = realDicts[i].Item.receiptNo
                };
                var resp = DataHandlerEx.住院预缴金充值(req);
                if (!resp.success)
                {
                    ShowAlert(false, "出院结算", $"{resp.msg}\n请到窗口操作！");
                    return Result.Fail(resp.msg);
                }
                var refundId = Guid.NewGuid().ToString().Replace("-", "");
                var refundRet = DataHandlerEx.扫码退费(new req扫码退费()
                {
                    outTradeNo = realDicts[i].Item.receiptNo,
                    outRefundNo = refundId,
                    fee = min.ToString(),
                    reason = "自助出院退多余押金"
                });
                if (!refundRet.success)
                {
                    DataHandlerEx.住院押金充值确认(new req住院押金充值确认()
                    {
                        patientId = patientInfo.patientHosId,
                        sFlowId = resp.data.extend,
                        transNo= ""
                    });
                    ShowAlert(false, "出院结算", $"{refundRet.msg}\n请到窗口操作！",extend:new AlertExModel()
                    {
                        HideCallback = tp =>
                        {
                            Navigate(A.Home);//导航回家
                        }
                    });
                    return Result.Fail(refundRet.msg);
                }
                DataHandlerEx.住院押金充值确认(new req住院押金充值确认()
                {
                    patientId = patientInfo.patientHosId,
                    sFlowId = resp.data.extend,
                    transNo = refundRet.data?.outTradeNo
                });
                refundLeft -= min;
                if (refundLeft <= 0)
                {
                    break;
                }
            }


            return Result.Success();
        }

        private Result StartRecharge(LoadingProcesser lp)
        {
           
            var patient = GetInstance<IPatientModel>();
            var extraPayment = GetInstance<IExtraPaymentModel>();
            var patientInfo = patient.住院患者信息;

            if (extraPayment.CurrentPayMethod==PayMethod.智慧医疗)
            {
                ShowAlert(false, "出院结算", "出院结算暂不支持智慧医疗!");
                return Result.Fail("出院结算暂不支持智慧医疗");
            }

            var req = new req住院预缴金充值
            {
                patientId = patientInfo.patientHosId,
                accountNo = patientInfo.patientHosId,
                operId = FrameworkConst.OperatorId,
                cash = extraPayment.TotalMoney.ToString(),
                tradeMode = extraPayment.CurrentPayMethod.GetEnumDescription(),
            };
            FillRechargeRequest(req);
            var resp = DataHandlerEx.住院预缴金充值(req);
            if (!resp.success)
            {
                ShowAlert(false,"出院结算","出院结算充值失败，请稍后再试!", extend: new AlertExModel()
                {
                    HideCallback = tp =>
                    {
                        Navigate(A.Home);//导航回家
                    }
                });
                return Result.Fail(resp.code,resp.msg);
            }
            extraPayment.Complete = true;
            return Result.Success();
        }

        protected virtual void FillRechargeRequest(req住院预缴金充值 req)
        {
            var extraPayment = GetInstance<IExtraPaymentModel>();
            if (extraPayment.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPayment.PaymentResult as TransResDto;
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
            else if (extraPayment.CurrentPayMethod == PayMethod.支付宝 ||
                     extraPayment.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = extraPayment.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    req.payAccountNo = thirdpayinfo.buyerAccount;
                    req.transNo = thirdpayinfo.outPayNo;
                    req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                }
            }
        }

        protected virtual Queue<IPrintable> ChuYuanPrintables()
        {
            var cardModel = GetInstance<ICardModel>();
            var patient = GetInstance<IPatientModel>();
            var patientInfo = patient.住院患者信息;
            var printManager = GetInstance<IPrintManager>();
            var queue = printManager.NewQueue("自助出院");
            var sb = new StringBuilder();
            sb.Append($"状态：出院成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"住院号：{cardModel.CardNo}\n");
            sb.Append($"住院ID：{patientInfo.patientHosId}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"如需打印发票或出院费用清单，请到住院收费处打印。\n");
            sb.Append($"请妥善保管好您的个人信息。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }

        protected override void ExitCard()
        {
            if (ConstInner.IsOldMachine)
            {
                Task.Run(() =>
                {
                    try
                    {
                        var reader = GetInstance<IMagCardReader[]>().FirstOrDefault(p => p.DeviceId == "ACT_A6_Mag&IC");
                        if (reader != null)
                        {
                            if (reader.Connect().IsSuccess)
                            {
                                reader.UnInitialize();
                                reader.DisConnect();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Main.Error($"[主动退卡]主页主动退卡发生异常,{ex.Message} {ex.StackTrace}");
                        
                    }
                });
            }
            else
            {
                base.ExitCard();
            }
           
        }
    }
}