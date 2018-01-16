using System;
using System.Collections.Generic;
using System.Linq;
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
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;

namespace YuanTu.YiWuBeiYuan.Component.ViewModels
{
    public class ChoiceViewModel : YiWuArea.Component.ViewModels.ChoiceViewModel
    {
        private VideoPlayerState _videoPlayerState;
        private Uri _videoUri;

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

        public override void OnSet()
        {
            base.OnSet();
            if (!string.IsNullOrWhiteSpace(Default.Clinic.Startup.VideoPath))
            {
                var uri = new Uri(Default.Clinic.Startup.VideoPath, UriKind.RelativeOrAbsolute);
                VideoUri = uri;
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
            var result = CheckReceiptPrinter();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "打印机检测", result.Message);
                return;
            }
           
            var choiceModel = GetInstance<IChoiceModel>();

            choiceModel.Business = param.ButtonBusiness;
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var engine = NavigationEngine;
            var fctx=new FormContext(A.ChaKa_Context,A.CK.Card);
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

        protected override Result CheckReceiptPrinter()
        {
            return Result.Success();
        }

    }
}