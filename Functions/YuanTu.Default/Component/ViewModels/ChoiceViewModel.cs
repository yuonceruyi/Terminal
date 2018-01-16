using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.EventModels;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Models.Configs;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Navigating;
using YuanTu.Devices.PrinterCheck;

namespace YuanTu.Default.Component.ViewModels
{
    public class ChoiceViewModel : ViewModelBase
    {
        protected bool IsNewMode;

        public ChoiceViewModel()
        {
            _content = DateTimeCore.Now.ToString("HHmmss");
            Command = new DelegateCommand<ChoiceButtonInfo>(Do, info => info.IsEnabled);
        }

        public override string Title => "主页";

        public DelegateCommand<ChoiceButtonInfo> Command { get; set; }
        
        public override void OnEntered(NavigationContext navigationContext)
        {
            OnSetButton();
            NavigationEngine.DestinationStack.Clear();
            TimeOut = 0;
        }

        protected virtual void Do(ChoiceButtonInfo param)
        {
            if (IsNewMode && param.SubModules.Count > 0)
            {
                PushButtonsLayOut(param.SubModules);
                var eventAggregator = GetInstance<IEventAggregator>();
                eventAggregator.GetEvent<ModulesChangeEvent>()
                    .Publish(new ModulesChangeEvent { ModulesChangeAction = PopButtonsLayOut, ButtonStack = ButtonStack, ResetAction = OnSetButton });
                return;
            }
            var choiceModel = GetInstance<IChoiceModel>();
            choiceModel.Business = param.ButtonBusiness;
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var engine = NavigationEngine;

            var result = CheckReceiptPrinter();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "打印机检测", result.Message);
                return;
            }

            var config = GetInstance<IConfigurationManager>();
            SwitchBusiness(param, choiceModel, engine, config);
        }

        protected virtual void SwitchBusiness(ChoiceButtonInfo param, IChoiceModel choiceModel, NavigationEngine engine, IConfigurationManager config)
        {
            switch (param.ButtonBusiness)
            {
                case Business.建档:

                    if (config.GetValue("SelectCreateType") == "1")
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Select),
                            CreateJump,
                            new FormContext(A.JianDang_Context, A.JD.Print), param.Name);
                    else
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                            CreateJump,
                            new FormContext(A.JianDang_Context, A.JD.Print), param.Name);
                    break;

                case Business.挂号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        RegisterJump,
                        new FormContext(A.XianChang_Context, A.XC.Wether), param.Name);
                    break;

                case Business.预约:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), 
                        AppointJump,
                        new FormContext(A.YuYue_Context, A.YY.Date), param.Name);
                    break;

                case Business.取号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), 
                        TakeNumJump,
                        new FormContext(A.QuHao_Context, A.QH.Record), param.Name);
                    break;

                case Business.缴费:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), 
                        BillPayJump,
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

                case Business.补打:
                    Navigate(A.PrintAgainChoice);
                    break;

                case Business.实名认证:
                    engine.JumpAfterFlow(null,
                        RealAuthJump,
                        new FormContext(A.RealAuth_Context, A.SMRZ.Card), param.Name);
                    break;

                case Business.生物信息录入:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        BiometricJump,
                        new FormContext(A.Biometric_Context, A.Bio.Choice), param.Name);
                    break;

                case Business.签到:
                    ShowAlert(true, "签到", "签到请直接刷卡或插入医保卡");
                    break;

                case Business.药品查询:
                    choiceModel.HasAuthFlow = false;
                    engine.JumpAfterFlow(null,
                        CreateJump,
                        new FormContext(A.MedicineQuery, A.YP.Query), param.Name);
                    break;

                case Business.项目查询:
                    choiceModel.HasAuthFlow = false;
                    engine.JumpAfterFlow(null,
                        CreateJump,
                        new FormContext(A.ChargeItemsQuery, A.XM.Query), param.Name);
                    break;

                case Business.已缴费明细:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        CreateJump,
                        new FormContext(A.PayCostQuery, A.JFJL.Date), param.Name);
                    break;

                case Business.住院一日清单:
                    choiceModel.AuthContext = A.ZhuYuan_Context;
                    engine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.InPatientNo),
                        CreateJump,
                        new FormContext(A.InDayDetailList_Context, A.ZYYRQD.Date), param.Name);
                    break;

                case Business.检验结果:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        CreateJump,
                        new FormContext(A.DiagReportQuery, A.JYJL.Date), param.Name);
                    break;

                case Business.影像结果:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        CreateJump,
                        new FormContext(A.PacsReportQuery, A.YXBG.Date), param.Name);
                    break;
                case Business.交易记录查询:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        CreateJump,
                        new FormContext(A.ReChargeQuery, A.CZJL.Date), param.Name);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual Result CheckReceiptPrinter()
        {
            var choiceModel = GetInstance<IChoiceModel>();
            switch (choiceModel.Business)
            {
                case Business.建档:
                case Business.挂号:
                case Business.预约:
                case Business.取号:
                case Business.缴费:
                case Business.充值:
                case Business.住院押金:
                    return GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
            }
            return Result.Success();
        }

        #region JumpAction

        protected virtual Task<Result<FormContext>> CreateJump()
        {
            return null;
            //return Task.Run(() => Result<FormContext>.Success(default(FormContext)));
        }

        protected virtual Task<Result<FormContext>> RegisterJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 现场挂号");
                Thread.Sleep(100);
                var regdateModel = GetInstance<IRegDateModel>();
                regdateModel.RegDate = DateTimeCore.Now.ToString("yyyy-MM-dd");
                return Result<FormContext>.Success(null);
            });
        }

        protected virtual Task<Result<FormContext>> AppointJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 预约挂号");
                Thread.Sleep(100);
                return Result<FormContext>.Success(null);
            });
        }

        protected virtual void FillTakeNumRequest(req挂号预约记录查询 req)
        {
            var patientModel = GetInstance<IPatientModel>();
            var cardModel = GetInstance<ICardModel>();

            req.patientId = patientModel.当前病人信息?.patientId;
            req.patientName = patientModel.当前病人信息?.name;
            req.startDate = DateTimeCore.Now.ToString("yyyy-MM-dd");
            req.endDate = DateTimeCore.Now.AddDays(7).ToString("yyyy-MM-dd");
            req.searchType = "1";
            req.cardNo = cardModel.CardNo;
            req.cardType = ((int)cardModel.CardType).ToString();
        }
        protected virtual Task<Result<FormContext>> TakeNumJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 预约取号");
                var recordModel = GetInstance<IAppoRecordModel>();
                var takeNumModel = GetInstance<ITakeNumModel>();
                lp.ChangeText("正在查询预约记录，请稍候...");

                recordModel.Req挂号预约记录查询 = new req挂号预约记录查询 { };
                FillTakeNumRequest(recordModel.Req挂号预约记录查询);

                recordModel.Res挂号预约记录查询 = DataHandlerEx.挂号预约记录查询(recordModel.Req挂号预约记录查询);
                if (recordModel.Res挂号预约记录查询?.success ?? false)
                {
                    if (recordModel.Res挂号预约记录查询?.data?.Count > 1)
                    {
                        return Result<FormContext>.Success(default(FormContext));
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
                            new PayInfoItem("挂号金额：", record.regAmount.In元(), true)
                        };
                        return Result<FormContext>.Success(new FormContext(A.QuHao_Context, A.QH.TakeNum));
                    }
                    ShowAlert(false, "预约记录查询", "没有获得预约记录信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }
                ShowAlert(false, "预约记录查询", "没有获得预约记录信息", debugInfo: recordModel.Res挂号预约记录查询?.msg);
                return Result<FormContext>.Fail("");
            });
        }

        protected virtual Task<Result<FormContext>> BillPayJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 结算");
                var billRecordModel = GetInstance<IBillRecordModel>();
                var patientModel = GetInstance<IPatientModel>();
                var cardModel = GetInstance<ICardModel>();
                lp.ChangeText("正在查询待缴费信息，请稍候...");
                billRecordModel.Req获取缴费概要信息 = new req获取缴费概要信息
                {
                    patientId = patientModel.Res病人信息查询?.data[patientModel.PatientInfoIndex].patientId,
                    cardType = ((int)cardModel.CardType).ToString(),
                    cardNo = cardModel.CardNo,
                    billType = ""
                };
                billRecordModel.Res获取缴费概要信息 = DataHandlerEx.获取缴费概要信息(billRecordModel.Req获取缴费概要信息);
                if (billRecordModel.Res获取缴费概要信息?.success ?? false)
                {
                    if (billRecordModel.Res获取缴费概要信息?.data?.Count > 0)
                    {
                        return Result<FormContext>.Success(default(FormContext));
                    }
                    ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }
                ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息", debugInfo: billRecordModel.Res获取缴费概要信息?.msg);
                return Result<FormContext>.Fail("");
            });
        }

        protected virtual Task<Result<FormContext>> RechargeJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 预缴金充值");
                Thread.Sleep(100);
                return Result<FormContext>.Success(null);
            });
        }

        protected virtual Task<Result<FormContext>> IpRechargeJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 住院预缴金充值");
                var patientModel = GetInstance<IPatientModel>();
                if (patientModel.住院患者信息.status == "入院")
                {
                    return Result<FormContext>.Success(default(FormContext));
                }
                else
                {
                    ShowAlert(false, "住院缴押金", "出院患者不能缴押金");
                    return Result<FormContext>.Fail("");
                }
            });
        }

        protected virtual Task<Result<FormContext>> RealAuthJump()
        {
            return null;
        }

        protected virtual Task<Result<FormContext>> BiometricJump()
        {
            return null;
        }

        #endregion JumpAction

        #region OnButton

        protected virtual void OnInRecharge(ChoiceButtonInfo param)
        {
            var choiceModel = GetInstance<IChoiceModel>();
            //choiceModel.HasAuthFlow = false;
            choiceModel.AuthContext = A.ZhuYuan_Context;
            NavigationEngine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.InPatientNo), this.IpRechargeJump,
                new FormContext(A.IpRecharge_Context, A.ZYCZ.RechargeWay), param.Name);
        }

        #endregion OnButton

        #region SetButton
        
        public virtual void OnSetButton()
        {
            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();

            LayoutRule = config.GetValue("LayoutRule");
            IsNewMode = config.GetValue("IsNewMode") == "1";
            if (!IsNewMode)
            {
                Data = GetButtons(resource, config);
                return;
            }
            if (ButtonStack.Count > 0)
            {
                InitButtonsLayOut();
                return;
            }
            PushButtonsLayOut(GetButtonsNew(resource, config));
        }

        protected static List<ChoiceButtonInfo> GetButtons(IResourceEngine resource, IConfigurationManager config)
        {
            var buttons = new List<ChoiceButtonInfo>();
            var k = Enum.GetValues(typeof(Business));
            foreach (Business buttonInfo in k)
            {
                var v = config.GetValue($"Functions:{buttonInfo}:Visabled");
                if (v != "1") continue;
                buttons.Add(new ChoiceButtonInfo
                {
                    Name = config.GetValue($"Functions:{buttonInfo}:Name") ?? "未定义",
                    ButtonBusiness = buttonInfo,
                    Order = config.GetValueInt($"Functions:{buttonInfo}:Order"),
                    IsEnabled = config.GetValueInt($"Functions:{buttonInfo}:IsEnabled") == 1,
                    ImageSource = resource.GetImageResource(config.GetValue($"Functions:{buttonInfo}:ImageName"))
                });
            }

            return buttons.OrderBy(b => b.Order).ToList();
        }

        protected List<ChoiceButtonInfo> GetButtonsNew(IResourceEngine resource, IConfigurationManager config)
        {
            var buttons = new List<ChoiceButtonInfo>();
            var homeModules = config.GetValues("主页");
            var busninesses = Enum.GetNames(typeof(Business));
            foreach (Section buttonInfo in homeModules)
            {
                var key = $"主页:{buttonInfo.Key}";
                var v = config.GetValue($"{key}:Visabled");
                if (v != "1") continue;
                var info = new ChoiceButtonInfo
                {
                    Name = config.GetValue($"{key}:Name") ?? "未定义",
                    Order = config.GetValueInt($"{key}:Order"),
                    IsEnabled = config.GetValueInt($"{key}:IsEnabled") == 1,
                    ImageSource = resource.GetImageResource(config.GetValue($"{key}:ImageName")),
                    ButtonBusiness = busninesses.Contains(buttonInfo.Key)
                        ? (Business)Enum.Parse(typeof(Business), buttonInfo.Key)
                        : Business.未定义
                };
                var subModules = config.GetValues($"{key}:SubModules");
                if (subModules.Length > 0)
                    ReadSubModules(subModules, info, key, config, resource);
                buttons.Add(info);
            }

            return buttons;
        }

        private void ReadSubModules(Section[] subModules, ChoiceButtonInfo info, string key, IConfigurationManager config, IResourceEngine resource)
        {
            foreach (var subModule in subModules)
            {
                var subkey = $"{key}:SubModules:{subModule.Key}";
                var v = config.GetValue($"{subkey}:Visabled");
                if (v != "1") continue;
                var name = config.GetValue($"{subkey}:Name") ?? "未定义";
                var businiess = Enum.GetNames(typeof(Business)).Contains(subModule.Key)
                    ? (Business)Enum.Parse(typeof(Business), subModule.Key)
                    : Business.未定义;
                var subInfo = new ChoiceButtonInfo
                {
                    Name = name,
                    Order = config.GetValueInt($"{subkey}:Order"),
                    IsEnabled = config.GetValueInt($"{subkey}:IsEnabled") == 1,
                    ImageSource = resource.GetImageResource(config.GetValue($"{subkey}:ImageName")),
                    ButtonBusiness = businiess
                };
                info.SubModules.Add(subInfo);
                var newsubModules = config.GetValues($"{subkey}:SubModules");
                if (newsubModules.Length > 0)
                {
                    ReadSubModules(newsubModules, subInfo, subkey, config, resource);
                }
            }
        }

        public Stack<List<ChoiceButtonInfo>> ButtonStack = new Stack<List<ChoiceButtonInfo>>();

        public void PushButtonsLayOut(List<ChoiceButtonInfo> bts)
        {
            ButtonStack.Push(bts);
            InitButtonsLayOut();
        }

        public void PopButtonsLayOut()
        {
            ButtonStack.Pop();
            InitButtonsLayOut();
        }

        public void InitButtonsLayOut()
        {
            Data = ButtonStack.FirstOrDefault()?.OrderBy(p => p.Order).ToList();
        }

        #endregion

        #region DataBindings


        private string _content;
        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }

        private IReadOnlyCollection<ChoiceButtonInfo> _data;
        public IReadOnlyCollection<ChoiceButtonInfo> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        private string _layOutRule;
        public string LayoutRule
        {
            get { return _layOutRule; }
            set
            {
                _layOutRule = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}