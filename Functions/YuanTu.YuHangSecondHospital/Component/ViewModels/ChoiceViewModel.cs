using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Core.Navigating;
using YuanTu.YuHangSecondHospital.Component.Dialog;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.YuHangSecondHospital.Component.ViewModels
{

    public class ChoiceViewModel : Default.Component.ViewModels.ChoiceViewModel
    {
        /**
        public new DelegateCommand<CategoryInfo> Command { get; set; }
        public DelegateCommand<ChoiceButtonInfo> SubCommand { get; set; }
        public ICommand SubCancelCommand { get; set; }

        public ChoiceViewModel()
        {
            Command = new DelegateCommand<CategoryInfo>(Do, info => info.IsEnabled);
            SubCommand = new DelegateCommand<ChoiceButtonInfo>(SubDo, info => info.IsEnabled);
            SubCancelCommand = new DelegateCommand(()=> ShowSubChoice = false);
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            ShowSubChoice = false;
            return base.OnLeaving(navigationContext);
        }

        public override void OnSetButton()
        {
            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();

            var bts = new List<CategoryInfo>();
            var k = Enum.GetValues(typeof(Business));
            foreach (Business buttonInfo in k)
            {
                var needAdd = true;
                var v = config.GetValue($"Functions:{buttonInfo}:Visabled");
                if (v != "1") continue;
                var category = config.GetValue($"Functions:{buttonInfo}:Category");

                var count = bts.Count;
                if (count >= 1)
                {
                    for (var i = 0; i < count; i++)
                    {
                        if (bts[i].Name == category)
                        {
                            bts[i].ContainsBusiness.Add(new ChoiceButtonInfo
                            {
                                Name = config.GetValue($"Functions:{buttonInfo}:Name"),
                                ButtonBusiness = buttonInfo,
                                Order = config.GetValueInt($"Functions:{buttonInfo}:Order"),
                                IsEnabled = config.GetValueInt($"Functions:{buttonInfo}:IsEnabled") == 1,
                                ImageSource =
                                    resource.GetImageResource(config.GetValue($"Functions:{buttonInfo}:ImageName"))
                            });
                            needAdd = false;
                            break;
                        }
                    }
                }

                if (needAdd)
                {
                    bts.Add(new CategoryInfo
                    {
                        Name = config.GetValue($"Functions:{buttonInfo}:Category"),
                        ContainsBusiness = new List<ChoiceButtonInfo>
                        {
                            new ChoiceButtonInfo
                            {
                                Name = config.GetValue($"Functions:{buttonInfo}:Name"),
                                ButtonBusiness = buttonInfo,
                                Order = config.GetValueInt($"Functions:{buttonInfo}:Order"),
                                IsEnabled = config.GetValueInt($"Functions:{buttonInfo}:IsEnabled") == 1,
                                ImageSource =
                                    resource.GetImageResource(config.GetValue($"Functions:{buttonInfo}:ImageName"))
                            }
                        },
                        Order = config.GetValueInt($"Functions:{buttonInfo}:Order"),
                        IsEnabled = config.GetValueInt($"Functions:{buttonInfo}:IsEnabled") == 1,
                        ImageSource = resource.GetImageResource(config.GetValue($"Functions:{buttonInfo}:ImageName"))
                    });
                }

            }
            Data = bts.OrderBy(p => p.Order).ToList();
        }

        protected virtual void Do(CategoryInfo param)
        {
            if (param.ContainsBusiness.Count <= 1)
            {
                SubDo(param.ContainsBusiness[0]);
                return;
            }
            DataSub = param.ContainsBusiness;
            ShowSubChoice = true;
        }

        protected virtual void SubDo(ChoiceButtonInfo param)
        {
            var choiceModel = GetInstance<IChoiceModel>();
            choiceModel.Business = param.ButtonBusiness;
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var engine = NavigationEngine;
            var result = CheckPrinter();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "打印机检测", result.Message);
                return;
            }
            switch (param.ButtonBusiness)
            {
                case Business.建档:

                    var config = GetInstance<IConfigurationManager>();
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
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), AppointJump,
                        new FormContext(A.YuYue_Context, A.YY.Date), param.Name);
                    break;

                case Business.取号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), TakeNumJump,
                        new FormContext(A.QuHao_Context, A.QH.Record), param.Name);
                    break;

                case Business.缴费:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), BillPayJump,
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
                case Business.未定义:
                    break;
                case Business.出院结算:
                    break;
                case Business.外院卡注册:
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
                case Business.住院一日清单:
                    choiceModel.AuthContext = A.ZhuYuan_Context;
                    engine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.InPatientNo),
                        CreateJump,
                        new FormContext(A.InDayDetailList_Context, A.ZYYRQD.Date), param.Name);
                    break;
                case Business.住院押金查询:
                    break;
                case Business.住院床位查询:
                    break;
                case Business.执业资格查询:
                    break;
                case Business.交易记录查询:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        CreateJump,
                        new FormContext(A.ReChargeQuery, A.CZJL.Date), param.Name);
                    break;
                case Business.门诊排班查询:
                    break;
                case Business.材料费查询:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IReadOnlyCollection<CategoryInfo> _data;
        private IReadOnlyCollection<ChoiceButtonInfo> _dataSub;
        private bool _showSubChoice;
        public new IReadOnlyCollection<CategoryInfo> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        public IReadOnlyCollection<ChoiceButtonInfo> DataSub
        {
            get { return _dataSub; }
            set
            {
                _dataSub = value;
                OnPropertyChanged();
            }
        }

        public bool ShowSubChoice
        {
            get { return _showSubChoice; }
            set
            {
                _showSubChoice = value;
                if (value)
                {
                    ShowMask(true, new SubChoiceView { DataContext = this });
                }
                else
                {
                    ShowMask(false);
                }
                OnPropertyChanged();
            }
        }
    }

    public class CategoryInfo
    {
        public string Name { get; set; }
        public List<ChoiceButtonInfo> ContainsBusiness { get; set; }
        public int Order { get; set; }
        public bool Visabled { get; set; }
        public bool IsEnabled { get; set; }
        public ImageSource ImageSource { get; set; }
    }
    **/


        protected override void Do(ChoiceButtonInfo param)
        {
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
            switch (param.ButtonBusiness)
            {
                case Business.建档:

                    var config = GetInstance<IConfigurationManager>();
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
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), AppointJump,
                        new FormContext(A.YuYue_Context, A.YY.Date), param.Name);
                    break;

                case Business.取号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), TakeNumJump,
                        new FormContext(A.QuHao_Context, InnerA.QH.Date), param.Name);
                    break;

                case Business.缴费:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), BillPayJump,
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
                Thread.Sleep(100);
                return Result<FormContext>.Success(null);
            });
        }



    }

}