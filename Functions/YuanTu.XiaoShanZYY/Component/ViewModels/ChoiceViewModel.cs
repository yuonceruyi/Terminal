using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Core.Navigating;
using YuanTu.Default;
using YuanTu.Default.Component.Advertisement.ViewModels;
using YuanTu.XiaoShanZYY.Component.BillPay.Models;
using YuanTu.XiaoShanZYY.Dto;

namespace YuanTu.XiaoShanZYY.Component.ViewModels
{
    public class ChoiceViewModel : Default.Clinic.Component.ViewModels.ChoiceViewModel
    {
        public ChoiceViewModel()
        {
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            _cacheService = new CacheService(root, null, true);
        }

        private readonly CacheService _cacheService;

        public override void OnSet()
        {
            var videoPath = Default.Clinic.Startup.VideoPath;
            if (string.IsNullOrWhiteSpace(videoPath))
                return;
            if (Uri.TryCreate(videoPath, UriKind.Absolute, out var uri) && uri.Scheme != Uri.UriSchemeFile)
            {
                // 来自远端
                //_cacheService.Request(videoPath);
                //if (Uri.TryCreate(_cacheService.Get(videoPath, out var ready), UriKind.Absolute, out var videoUri))
                //    VideoUri = videoUri;
                var lst=SimpleVideoCacheManager.MergeVideoList();
                VideoUri = new Uri(lst.FirstOrDefault()??"",UriKind.Absolute);
            }
            else
            {
                // 来自本地文件
                if ( Uri.TryCreate(videoPath, UriKind.Absolute, out var videoUri))
                    VideoUri = videoUri;
            }
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            var main = GetInstance<MainWindowViewModel>();
            main.ClinicTopBorderHeight = 640;
            base.OnEntered(navigationContext);
        }

        //private void MigrateVideoResource(string serverUrl)
        //{
        //    var http=new HttpClient();
        //    http.GetAsync()
        //}

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
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.HICard),
                        RegisterJump,
                        new FormContext(A.XianChang_Context, A.XC.Wether), param.Name);
                    break;

                case Business.预约:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.HICard),
                        AppointJump,
                        new FormContext(A.YuYue_Context, A.YY.Wether), param.Name);
                    break;

                case Business.取号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.HICard),
                        TakeNumJump,
                        new FormContext(A.QuHao_Context, A.QH.Query), param.Name);
                    break;

                case Business.缴费:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.HICard),
                        BillPayJump,
                        new FormContext(A.JiaoFei_Context, A.JF.BillRecord), param.Name);
                    break;

                case Business.充值:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.HICard),
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
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.HICard),
                        PrintAgainJump,
                        new FormContext(A.BuDa_Context, A.JFBD.PayCostRecord), param.Name);
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

        private Task<Result<FormContext>> PrintAgainJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 现场挂号");
                Thread.Sleep(100);
                var service = GetInstance<PrintAgain.Models.IPrintAgainService>();

                var result = service.补打查询();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "补打查询", "补打查询失败:\n" + result.Message);
                    return Result<FormContext>.Fail("");
                }

                return Result<FormContext>.Success(null);
            });
        }

        protected override Task<Result<FormContext>> RegisterJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 现场挂号");
                Thread.Sleep(100);
                var register = GetInstance<Register.Models.IRegisterModel>();
                register.RegDate = DateTimeCore.Today.ToString("yyyy-MM-dd");
                register.RegMode = "1";
                return Result<FormContext>.Success(null);
            });
        }

        protected override Task<Result<FormContext>> AppointJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 预约挂号");
                Thread.Sleep(100);
                var register = GetInstance<Register.Models.IRegisterModel>();
                register.RegDate = DateTimeCore.Today.AddDays(1).ToString("yyyy-MM-dd");
                register.RegMode = "2";
                return Result<FormContext>.Success(null);
            });
        }

        protected override Task<Result<FormContext>> BillPayJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 结算");
                var billPayService = GetInstance<IBillPayService>();
                lp.ChangeText("正在查询待缴费信息，请稍候...");
                var result = billPayService.获取费用明细();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "待缴费信息查询", "待缴费信息查询失败:"+result.Message);
                    return Result<FormContext>.Fail("");
                }
                return Result<FormContext>.Success(default(FormContext));
            });
        }

        protected override Task<Result<FormContext>> TakeNumJump()
        {
            return null;
        }
    }
}