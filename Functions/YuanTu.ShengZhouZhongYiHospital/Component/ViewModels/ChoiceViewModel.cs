using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Prism.Events;
using Prism.Regions;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.EventModels;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Devices.CardReader;
using YuanTu.Devices.PrinterCheck;
using YuanTu.Devices.PrinterCheck.CePrinterCheck;
using YuanTu.ShengZhouZhongYiHospital.Component.BillPay.Services;
using YuanTu.ShengZhouZhongYiHospital.HisNative.Models;
using YuanTu.Core.Models;
using YuanTu.ShengZhouZhongYiHospital.Component.Auth.Models;

namespace YuanTu.ShengZhouZhongYiHospital.Component.ViewModels
{
    public class ChoiceViewModel : YuanTu.Default.Clinic.Component.ViewModels.ChoiceViewModel
    {

        static ChoiceViewModel()
        {
            Application.Current.Exit += (s, e) =>
            {
                try
                {
                    var vm = GetInstance<IRFCpuCardReader[]>().FirstOrDefault(p => p.DeviceId == "CRT310_IC");
                    vm.UnInitialize();
                    vm.DisConnect();
                }
                catch (Exception)
                {

                }
            };
        }
        [Dependency]
        public IEventAggregator EventAggregator { get; set; }

        public string PatientDesc
        {
            get { return _patientDesc; }
            set
            {
                _patientDesc = value;
                OnPropertyChanged();
            }
        }

        public override void OnSetButton()
        {
            if (CurrentStrategyType() == DeviceType.Clinic)
            {
                if (!string.IsNullOrWhiteSpace(Startup.VideoPath))
                {
                    var uri = new Uri(Startup.VideoPath, UriKind.Absolute);
                    VideoUri = uri;
                }
            }
            DisableHomeButton = true;
            DisablePreviewButton = true;
            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();

            LayoutRule = config.GetValue("LayoutRule");
            var bts = new List<ChoiceButtonInfo>();
            var k = Enum.GetValues(typeof(Business));
            foreach (Business buttonInfo in k)
            {
                var v = config.GetValue($"Functions:{buttonInfo}:Visabled");
                if (v != "1") continue;
                bts.Add(new ChoiceButtonInfo
                {
                    Name = config.GetValue($"Functions:{buttonInfo}:Name") ?? "未定义",
                    ButtonBusiness = buttonInfo,
                    Order = config.GetValueInt($"Functions:{buttonInfo}:Order"),
                    IsEnabled = config.GetValueInt($"Functions:{buttonInfo}:IsEnabled") == 1,
                    ImageSource = resource.GetImageResource(config.GetValue($"Functions:{buttonInfo}:ImageName"))
                });
            }

            var visabled = config.GetValue("Functions:体检缴费:Visabled");
            if (visabled == "1")
            {
                bts.Add(new ChoiceButtonInfo
                {
                    Name = config.GetValue("Functions:体检缴费:Name"),
                    ButtonBusiness = (Business)100,
                    Order = config.GetValueInt("Functions:体检缴费:Order"),
                    IsEnabled = config.GetValueInt("Functions:体检缴费:IsEnabled") == 1,
                    ImageSource = resource.GetImageResource(config.GetValue("Functions:体检缴费:ImageName"))
                });
            }
            Data = bts.OrderBy(p => p.Order).ToList();
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            Debug.WriteLine("[当前页面]:" + this.GetHashCode());
            base.OnEntered(navigationContext);
            ConstInner.ClearCacheDataCallback = () =>
            {
                PatientDesc = null;
            };

            //缴费墓碑解除
            if (PrescriptionLock.HasLock())
            {
                DoCommand(lp =>
                {
                    lp.ChangeText("正在解除处方锁定，请稍候...");
                    PrescriptionLock.RemoveLock();
                });
            }

            if (CurrentStrategyType() == DeviceType.Clinic)
            {
                Play();
            }
            else
            {
                StartCheckCard();
            }

        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            if (CurrentStrategyType() == DeviceType.Clinic)
            {
                Pause();
            }
            _working = false;
            return base.OnLeaving(navigationContext);
        }

        private bool _working;
        private string _patientDesc;

        protected virtual void StartCheckCard()
        {
            if (ConstInner.HaveCacheData())
            {
                return;
            }
            Task.Run(() =>
            {
                if (_working)
                {
                    return; //预判防止重复
                }

                _working = true;
                var vm = GetInstance<IRFCpuCardReader[]>().FirstOrDefault(p => p.DeviceId == "CRT310_IC");
                Thread.Sleep(500);

                if (vm != null)
                {
                    if (vm.Connect().IsSuccess)
                    {
                        vm.Initialize();
                        //var pos = vm.GetCardPosition();
                        //if (pos.IsSuccess && (pos.Value == CardPos.停卡位 || pos.Value == CardPos.IC位))//一进来就有卡
                        //{
                        //    _working = false;
                        //    return;
                        //}

                        while (_working)
                        {
                            var pos = vm.GetCardPosition();
                            if (pos.IsSuccess && (pos.Value == CardPos.停卡位 || pos.Value == CardPos.IC位))
                            {
                                Logger.Main.Info($"[自动读卡]发现卡，开始读卡！");
                                //有卡，读卡
                                _working = false;
                                //EventAggregator.GetEvent<SystemInfoEvent>().Publish(new SystemInfoEvent
                                //{
                                //    DisablePreviewButton = true,
                                //    DisableHomeButton = false
                                //});
                                var result = CheckReceiptPrinter();
                                if (!result.IsSuccess)
                                {
                                    ShowAlert(false, "打印机检测", result.Message, extend: new AlertExModel()
                                    {
                                        HideCallback = at =>
                                        {
                                            if (NavigationEngine.State == A.Home)
                                            {
                                                ConstInner.ClearCacheData();
                                                StartCheckCard();
                                            }

                                        }
                                    });
                                    return;
                                }
                                if (NavigationEngine.State == A.Home)
                                {
                                    Read(vm);
                                }
                                //Task.Run()

                                break;
                            }
                            Thread.Sleep(300);
                        }
                        // vm.DisConnect();
                    }
                }
            });

        }

        private void Read(IRFCpuCardReader rfCpuCardReader)
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在读取卡片信息，请稍后...");
                rfCpuCardReader.MoveCard(CardPos.IC位);
                Thread.Sleep(200); //等卡停稳
                var pm = GetInstance<IPatientModel>() as PatientInfoModel;
                var cm = GetInstance<ICardModel>();
                pm.Req门诊读卡 = new Req门诊读卡() { };
                pm.Req门诊读卡.卡类别 = "2"; //医保
                pm.Res门诊读卡 = HisHandleEx.执行门诊读卡(pm.Req门诊读卡);
                if (pm.Res门诊读卡.IsSuccess)
                {
                    cm.CardType = CardType.社保卡;
                    return 病人信息查询(lp, pm.Res门诊读卡.卡号);
                }
                else if (pm.Res门诊读卡.Message.Contains("您未在本院就诊过"))
                {
                    return Result.Fail(pm.Res门诊读卡.Message);
                }
                pm.Req门诊读卡.卡类别 = "1"; //就诊卡
                pm.Res门诊读卡 = HisHandleEx.执行门诊读卡(pm.Req门诊读卡);
                if (pm.Res门诊读卡.IsSuccess)
                {
                    cm.CardType = CardType.就诊卡;
                    return 病人信息查询(lp, pm.Res门诊读卡.卡号);

                }
                return Result.Fail("读卡失败，请确认您的卡片是否正确插入！");
            }).ContinueWith(ret =>
            {
                if (!ret.Result.IsSuccess && !string.IsNullOrEmpty(ret.Result.Message)) //失败重来
                {
                    ShowAlert(false, "病人信息查询失败", ret.Result.Message, 10, extend: new AlertExModel()
                    {
                        HideCallback = hc =>
                        {
                            var vm = GetInstance<IRFCpuCardReader[]>().FirstOrDefault(p => p.DeviceId == "CRT310_IC");
                            vm.MoveCard(CardPos.不持卡位);
                            ConstInner.ClearCacheData();
                            StartCheckCard();
                        }
                    });
                }
            });

        }


        private Result 病人信息查询(LoadingProcesser lp, string cardNo)
        {
            var pm = GetInstance<IPatientModel>() as PatientInfoModel;
            var cm = GetInstance<ICardModel>();
            pm.Req病人信息查询 = new req病人信息查询
            {
                cardNo = cardNo,
                cardType = ConstInner.CardTypeMapping[cm.CardType].ToString()
            };
            pm.Res病人信息查询 = DataHandlerEx.病人信息查询(pm.Req病人信息查询);
            if (pm.Res病人信息查询.success)
            {
                if (pm.Res病人信息查询.data == null || pm.Res病人信息查询.data.Count == 0)
                {
                    return Result.Fail("未查询到病人的信息(列表为空)");
                }
                var vm = GetInstance<IRFCpuCardReader[]>().FirstOrDefault(p => p.DeviceId == "CRT310_IC");
                var position = vm.GetCardPosition();
                if (!position.IsSuccess || !(position.Value == CardPos.停卡位 || position.Value == CardPos.IC位))
                {
                    return Result.Fail("已经退卡,请插卡重新操作");
                }
                cm.CardNo = cardNo;
                //姓名：高密 性别：女 卡号：F0000000034567
                PatientDesc = $"姓名：{pm.当前病人信息.name} 性别：{pm.当前病人信息.sex.SafeToSex()} 卡号：{cardNo}";
                ConstInner.SaveCacheData(pm.Req门诊读卡, pm.Res门诊读卡, pm.Req病人信息查询, pm.Res病人信息查询, cm.CardType, cm.CardNo);
                JudgeNavigation(lp);

                return Result.Success();
            }
            return Result.Fail(pm.Res病人信息查询.msg);
        }

        private void JudgeNavigation(LoadingProcesser lp)
        {
            var choiceModel = GetInstance<IChoiceModel>();
            var billpayRet = BuildBillpayInfoFromGateway(lp);
            var engine = NavigationEngine;
            if (billpayRet.IsSuccess)
            {
                PrescriptionLock.RemoveLock();
                BeginInvoke(DispatcherPriority.ContextIdle, () =>
                {
                    ShowConfirm("温馨提示", "您有未交费的处方单，请问是否直接付费？", callback: cbk =>
                    {
                        if (cbk)
                        {
                            choiceModel.Business = Business.缴费;
                            //有费用，直接跳转
                            engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Info), BillPayJump,
                                new FormContext(A.JiaoFei_Context, A.JF.BillRecord), "缴费信息");
                        }
                    }, extend: ConfirmExModel.Build("直接付费", "其他功能"));
                });


                return;
            }
            var takeNumRet = BuildTakeNumInfoFromGateway(lp);
            if (takeNumRet.IsSuccess)
            {
                //recordModel.Res挂号预约记录查询
                var recordModel = GetInstance<IAppoRecordModel>();

                if (recordModel.Res挂号预约记录查询.data.Any(p =>
                {
                    var record = p;
                    var realTime = record.medDate + " " + record.medTime;
                    DateTime medTime;
                    if (DateTime.TryParseExact(realTime, "yyyy-MM-dd HH:mm", null, DateTimeStyles.AllowWhiteSpaces,
                        out medTime))
                    {
                        return DateTimeCore.Today == medTime.Date && DateTimeCore.Now < medTime;
                    }
                    else if (DateTime.TryParseExact(record.medDate, "yyyy-MM-dd", null, DateTimeStyles.AllowWhiteSpaces, out medTime))
                    {
                        return DateTimeCore.Today == medTime.Date;
                    }
                    return true;
                }))
                {
                    var context = takeNumRet.Value?.Context ?? A.QuHao_Context;
                    var state = takeNumRet.Value?.Address ?? A.QH.Record;
                    BeginInvoke(DispatcherPriority.ContextIdle, () =>
                    {
                        ShowConfirm("温馨提示", "您有已经预约的记录，请问是否直接取号？", callback: cbk =>
                        {
                            if (cbk)
                            {
                                //有预约记录，直接跳转
                                choiceModel.Business = Business.取号;
                                engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Info), null,
                                    new FormContext(context, state), "取号信息");
                            }
                        }, extend: ConfirmExModel.Build("直接取号", "其他功能"));
                    });
                }





                return;
            }
        }

        protected override Result CheckReceiptPrinter()
        {
            var choiceModel = GetInstance<IChoiceModel>();
            switch (choiceModel.Business)
            {
                case (Business)100:
                case Business.建档:
                case Business.挂号:
                case Business.预约:
                case Business.取号:
                case Business.缴费:
                    return GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
            }
            return Result.Success();
        }

        protected override void Do(ChoiceButtonInfo param)
        {
            var choiceModel = GetInstance<IChoiceModel>();
            choiceModel.Business = param.ButtonBusiness;
            var pm = GetInstance<IPatientModel>() as PatientInfoModel;
            var cm = GetInstance<ICardModel>();
            var result = CheckReceiptPrinter();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "打印机检测", result.Message);
                return;
            }
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var engine = NavigationEngine;

            var realAddress = ConstInner.FillCacheData(pm, cm) && CurrentStrategyType() != DeviceType.Clinic ? A.CK.Info : A.CK.HICard;
            Logger.Main.Info($"最终跳转:{realAddress}");
            if (param.ButtonBusiness == (Business)100)
            {
                engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, realAddress), BillPayJump,
                    new FormContext(A.JiaoFei_Context, A.JF.BillRecord), param.Name);
                return;
            }
            switch (param.ButtonBusiness)
            {
                case Business.建档:
                    if (pm.Res门诊读卡 != null)
                    {
                        ShowAlert(false, "操作失败", "您的档案已经存在，无需重复建档！");
                        return;
                    }
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.IDCard),
                        CreateJump,
                        new FormContext(A.JianDang_Context, AInner.JD.Confirm), param.Name);
                    break;

                case Business.挂号:

                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, realAddress),
                        RegisterJump,
                        new FormContext(A.XianChang_Context, A.XC.Wether), param.Name);
                    break;

                case Business.预约:

                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, realAddress), AppointJump,
                        new FormContext(A.YuYue_Context, A.YY.Date), param.Name);
                    break;

                case Business.取号:

                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, realAddress), TakeNumJump,
                        new FormContext(A.QuHao_Context, A.QH.Record), param.Name);
                    break;

                case Business.缴费:

                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, realAddress), BillPayJump,
                        new FormContext(A.JiaoFei_Context, A.JF.BillRecord), param.Name);
                    break;



                case Business.充值:

                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        RechargeJump,
                        new FormContext(A.ChongZhi_Context, A.CZ.RechargeWay), param.Name);
                    break;

                case Business.查询:
                    Navigate(A.QueryChoice);

                    break;
                case Business.住院押金:

                    OnInRecharge(param);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override Task<Result<FormContext>> BillPayJump()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在查询待缴费信息，请稍候...");
                var result = BuildBillpayInfoFromGateway(lp);
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "温馨提示", result.Message, extend: new AlertExModel()
                    {
                        HideCallback = hp =>
                        {
                            Navigate(A.Home);
                        }
                    });
                    return Result<FormContext>.Fail(result.Message);
                }
                return Result<FormContext>.Success(default(FormContext));
            });
        }

        private Result BuildBillpayInfoFromGateway(LoadingProcesser lp)
        {
            var billRecordModel = GetInstance<IBillRecordModel>();
            var patientModel = GetInstance<IPatientModel>();
            var cardModel = GetInstance<ICardModel>();
            billRecordModel.Req获取缴费概要信息 = new req获取缴费概要信息
            {
                patientId = patientModel.当前病人信息.patientId,
                cardType = ((int)cardModel.CardType).ToString(),
                cardNo = cardModel.CardNo,
                billType = ""
            };
            billRecordModel.Res获取缴费概要信息 = DataHandlerEx.获取缴费概要信息(billRecordModel.Req获取缴费概要信息);
            if (billRecordModel.Res获取缴费概要信息?.success ?? false)
            {
                if (billRecordModel.Res获取缴费概要信息?.data?.Count > 0)
                {

                    lp.ChangeText("正在锁定当前处方，请稍候...");
                    var 处方单号 =
                        string.Join(",", billRecordModel.Res获取缴费概要信息.data.Select(p => $"{p.billType}/{p.billNo}")) + ",";
                    var ret = PrescriptionLock.AddLock(patientModel.当前病人信息, 处方单号);
                    if (!ret.IsSuccess)
                    {

                        return Result.Fail(ret.Message);
                    }
                    return Result.Success();
                }
                //ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息(列表为空)");
                return Result.Fail("没有获得待缴费信息(列表为空)");
            }
            return Result.Fail(billRecordModel.Res获取缴费概要信息?.msg);
        }

        private Result<FormContext> BuildTakeNumInfoFromGateway(LoadingProcesser lp)
        {
            var patientModel = GetInstance<IPatientModel>();
            var recordModel = GetInstance<IAppoRecordModel>();
            var cardModel = GetInstance<ICardModel>();
            var takeNumModel = GetInstance<ITakeNumModel>();
            //2017-05-26 挂号预约记录查询 多尼说patientId改成传身份证号
            recordModel.Req挂号预约记录查询 = new req挂号预约记录查询
            {
                patientId = patientModel.当前病人信息?.idNo,
                patientName = patientModel.当前病人信息?.name,
                startDate = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                endDate = DateTimeCore.Now.AddDays(7).ToString("yyyy-MM-dd"),
                searchType = "1",
                cardNo = cardModel.CardNo,
                cardType = ((int)cardModel.CardType).ToString()
            };
            recordModel.Res挂号预约记录查询 = DataHandlerEx.挂号预约记录查询(recordModel.Req挂号预约记录查询);
            if (recordModel.Res挂号预约记录查询?.success ?? false)
            {
                if (recordModel.Res挂号预约记录查询?.data?.Count > 1)
                {
                    return Result<FormContext>.Success(null);
                }
                if (recordModel.Res挂号预约记录查询?.data?.Count == 1)
                {
                    recordModel.所选记录 = recordModel.Res挂号预约记录查询.data.FirstOrDefault();
                    var record = recordModel.所选记录;

                    takeNumModel.List = new List<PayInfoItem>
                    {
                        new PayInfoItem("就诊日期：", record.medDate.SafeConvertToDate("yyyy-MM-dd", "yyyy年MM月dd日")),
                        new PayInfoItem("就诊科室：", record.deptName),
                        new PayInfoItem("就诊医生：", record.doctName),
                        new PayInfoItem("就诊时段：", record.medAmPm.SafeToAmPm()),
                        new PayInfoItem("就诊序号：", record.appoNo),
                    };
                    return Result<FormContext>.Success(new FormContext(A.QuHao_Context, A.QH.TakeNum));
                }
                //ShowAlert(false, "预约记录查询", "没有获得预约记录信息(列表为空)");
                return Result<FormContext>.Fail("没有获得预约记录信息(列表为空)");
            }
            //ShowAlert(false, "预约记录查询", "没有获得预约记录信息", debugInfo: recordModel.Res挂号预约记录查询?.msg);
            return Result<FormContext>.Fail("没有获得预约记录信息", new Exception(recordModel.Res挂号预约记录查询?.msg));
        }

        protected override Task<Result<FormContext>> TakeNumJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 预约取号");
                lp.ChangeText("正在查询预约记录，请稍候...");
                //2017-05-26 挂号预约记录查询 多尼说patientId改成传身份证号
                var ret = BuildTakeNumInfoFromGateway(lp);
                if (!ret.IsSuccess)
                {
                    ShowAlert(false, "预约记录查询", ret.Message, debugInfo: ret.Exception?.Message);
                }
                return ret;
            });
        }

    }
}
