using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;
using YuanTu.Default;
using YuanTu.Default.Component.Advertisement.ViewModels;
using YuanTu.YuHangFYBJY.Dto;

namespace YuanTu.YuHangFYBJY.Component.ViewModels
{

    public class ChoiceViewModel : Default.Clinic.Component.ViewModels.ChoiceViewModel
    {
        private readonly CacheService _cacheService;
        public ChoiceViewModel()
        {
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            _cacheService = new CacheService(root, null, true);
        }

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
                var lst = SimpleVideoCacheManager.MergeVideoList();
                VideoUri = new Uri(lst.FirstOrDefault() ?? "", UriKind.Absolute);
            }
            else
            {
                // 来自本地文件
                if (Uri.TryCreate(videoPath, UriKind.Absolute, out var videoUri))
                    VideoUri = videoUri;
            }
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            var main = GetInstance<MainWindowViewModel>();
            main.ClinicTopBorderHeight = 640;
        }

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
                    startDate = ""/*DateTimeCore.Now.ToString("yyyy-MM-dd")*/,
                    endDate ="" /*DateTimeCore.Now.AddDays(7).ToString("yyyy-MM-dd")*/,
                    searchType = "1",
                    cardNo = cardModel.CardNo,
                    cardType = ((int)cardModel.CardType).ToString()
                };
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




    }

}