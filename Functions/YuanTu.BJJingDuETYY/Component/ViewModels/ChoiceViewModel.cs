using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using YuanTu.BJArea;
using YuanTu.BJArea.Enums;
using YuanTu.BJArea.Models.Register;
using YuanTu.BJArea.Models.TakeNum;
using YuanTu.BJJingDuETYY.Component.TakeNum.Services;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Models.GateWayStatus;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Devices.PrinterCheck;

namespace YuanTu.BJJingDuETYY.Component.ViewModels
{
    public class ChoiceViewModel : YuanTu.Default.Component.ViewModels.ChoiceViewModel
    {
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

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

        public int Volume => GetInstance<IConfigurationManager>().GetValueInt("Clinic:Volume", 80);

        protected override void Do(ChoiceButtonInfo param)
        {
            Result result;
            var choiceModel = GetInstance<IChoiceModel>();

            choiceModel.Business = param.ButtonBusiness;
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var engine = NavigationEngine;

            if (FrameworkConst.DeviceType != "YT-740")
            {
                result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
                //if (!result.IsSuccess)
                //{
                //    ShowAlert(false, "打印机检测", result.Message);
                //    return;
                //}
            }

            switch (param.ButtonBusiness)
            {
                case Business.建档:
                    BefourCreate(engine, param);
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

                case Business.生物信息录入:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        BiometricJump,
                        new FormContext(A.Biometric_Context, A.Bio.Choice), param.Name);
                    break;
                case Business.签到:
                    ShowAlert(true, "签到", "签到请直接刷卡或插入医保卡");
                    break;
                case Business.检验结果:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                       LisPrintJump,
                       new FormContext(A.DiagReportQuery, AInner.JYJL.Print), param.Name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            if (CurrentStrategyType() == DeviceType.Clinic)
            {
                VideoPlayerState = VideoPlayerState.Play;
            }

            #region 解除锁号
            var patientModel = GetInstance<IPatientModel>();
            if (ConfigBJ.ScheduleVersion == "1" && patientModel.当前病人信息 != null)
            {
                //查询预约记录

                var registerModel = GetInstance<IRegisterModel>();
                var recordModel = GetInstance<IAppoRecordModel>();
                var cardModel = GetInstance<ICardModel>();
                var takeNumExtendModel = GetInstance<ITakeNumExtendModel>();
                var regUnLockExtendModel = GetInstance<IRegUnLockExtendModel>();

                takeNumExtendModel.version = ConfigBJ.ScheduleVersion;

                DoCommand(lp =>
                {
                    lp.ChangeText("正在查询锁号记录，请稍候...");
                    recordModel.Req挂号预约记录查询 = new req挂号预约记录查询
                    {
                        patientId = patientModel.当前病人信息?.patientId,
                        patientName = patientModel.当前病人信息?.name,
                        startDate = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                        endDate = DateTimeCore.Now.AddDays(7).ToString("yyyy-MM-dd"),
                        //searchType = "2",//1、预约 2、挂号 3 加号4 诊间加号//北京修改
                        cardNo = cardModel.CardNo,
                        cardType = ((int)cardModel.CardType).ToString(),
                        status = "101",//锁号状态的
                        appoNo = "",//传空
                        regMode = "",
                        extend = takeNumExtendModel.ToJsonString(),
                    };
                    recordModel.Res挂号预约记录查询 = DataHandlerEx.挂号预约记录查询(recordModel.Req挂号预约记录查询);
                    if (recordModel.Res挂号预约记录查询?.success ?? false)
                    {
                        if (recordModel.Res挂号预约记录查询?.data.Where(p => p.status == "101").ToList().Count > 0)
                        {
                            lp.ChangeText("正在解除锁号记录，请稍候...");

                            regUnLockExtendModel.version = ConfigBJ.ScheduleVersion;
                            foreach (var obj in recordModel.Res挂号预约记录查询.data.Where(obj => !obj.extend.IsNullOrWhiteSpace() && obj.status == "101"))
                            {
                                registerModel.Req挂号解锁 = new req挂号解锁
                                {
                                    lockId = obj.extend,
                                    extend = regUnLockExtendModel.ToJsonString(),
                                };
                                registerModel.Res挂号解锁 = DataHandlerEx.挂号解锁(registerModel.Req挂号解锁);
                                //不处理返回值
                            }
                        }

                    }
                });
            }
            #endregion
        }
        public override void OnSet()
        {
            base.OnSet();
            if (CurrentStrategyType() == DeviceType.Clinic)
            {
                if (!string.IsNullOrWhiteSpace(YuanTu.Default.Clinic.Startup.VideoPath))
                {
                    var uri = new Uri(YuanTu.Default.Clinic.Startup.VideoPath, UriKind.Absolute);
                    VideoUri = uri;
                }
            }
        }
        public override bool OnLeaving(NavigationContext navigationContext)
        {
            if (CurrentStrategyType() == DeviceType.Clinic)
            {
                VideoPlayerState = VideoPlayerState.Pause;
            }
            return base.OnLeaving(navigationContext);
        }
        protected override Task<Result<FormContext>> RegisterJump()
        {
            var camera = GetInstance<ICameraService>();
            camera.SnapShot("主界面 现场挂号");
            Thread.Sleep(100);
            return Task.Run(() => Result<FormContext>.Success(default(FormContext)));
        }
        protected override Task<Result<FormContext>> TakeNumJump()
        {
            var patientModel = GetInstance<IPatientModel>();
            var recordModel = GetInstance<IAppoRecordModel>();
            var cardModel = GetInstance<ICardModel>();
            var takeNumModel = GetInstance<ITakeNumModel>();
            var takeNumExtendModel = GetInstance<ITakeNumExtendModel>();

            takeNumExtendModel.version = ConfigBJ.ScheduleVersion;
            var lp = GetInstance<LoadingProcesser>();
            lp.ChangeText("正在查询预约记录，请稍候...");
            recordModel.Req挂号预约记录查询 = new req挂号预约记录查询
            {
                patientId = patientModel.当前病人信息?.patientId,
                patientName = patientModel.当前病人信息?.name,
                startDate = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                endDate = DateTimeCore.Now.AddDays(7).ToString("yyyy-MM-dd"),
                searchType = "1",//1、预约 2、挂号 3 加号4 诊间加号
                cardNo = cardModel.CardNo,
                cardType = ((int)cardModel.CardType).ToString(),
                status = "",//传空，获取所有状态
                appoNo = "",//传空
                regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                extend = takeNumExtendModel.ToJsonString(),
            };
            recordModel.Res挂号预约记录查询 = DataHandlerEx.挂号预约记录查询(recordModel.Req挂号预约记录查询);
            if (recordModel.Res挂号预约记录查询?.success ?? false)
            {
                if (recordModel.Res挂号预约记录查询?.data?.Count > 1)
                {
                    return Task.Run(() => Result<FormContext>.Success(default(FormContext)));
                }
                if (recordModel.Res挂号预约记录查询?.data?.Count == 1)
                {
                    var ret = RecordInfoHelper.GetStatusEnables(recordModel.Res挂号预约记录查询.data.FirstOrDefault()?.status);
                    if (!ret.IsSuccess || !ret.Value.Item1)
                    {
                        ShowAlert(false, "预约记录查询", $"无法取号，该预约记录状态为:{ret.Value.Item2}");
                        return Task.Run(() => Result<FormContext>.Fail(""));
                    }
                    recordModel.所选记录 = recordModel.Res挂号预约记录查询.data.FirstOrDefault();
                    var record = recordModel.所选记录;

                    var payStatus = "";
                    if (!record.payStatus.IsNullOrWhiteSpace())
                    {
                        payStatus = ((ApptPayStatus)Enum.Parse(typeof(ApptPayStatus), record.payStatus)).GetEnumDescription();
                    }

                    takeNumModel.List = new List<PayInfoItem>
                        {
                            new PayInfoItem("就诊日期：", record.medDate.SafeConvertToDate("yyyy-MM-dd","yyyy年MM月dd日")),
                            new PayInfoItem("就诊科室：", record.deptName),
                            new PayInfoItem("就诊医生：", record.doctName),
                            new PayInfoItem("就诊时段：", record.medAmPm.SafeToAmPm()),
                            new PayInfoItem("就诊序号：", record.appoNo),
                            new PayInfoItem("挂号金额：", $"{record.regAmount.In元()} {payStatus}", true),
                            new PayInfoItem("预约状态：", RecordInfoHelper.GetStatusEnables(record.status).Value.Item2)
                        };
                    return Task.Run(() => Result<FormContext>.Success(default(FormContext)));
                }
                ShowAlert(false, "预约记录查询", "没有获得预约记录信息(列表为空)");
                return Task.Run(() => Result<FormContext>.Fail(""));
            }
            ShowAlert(false, "预约记录查询", "没有获得预约记录信息\r\n请确认身份证号/监护人身份证号+姓名是否与预约时的就诊人信息一致。");
            return Task.Run(() => Result<FormContext>.Fail(""));

        }
        protected override Task<Result<FormContext>> BillPayJump()
        {
            var camera = GetInstance<ICameraService>();
            camera.SnapShot("主界面 结算");
            var billRecordModel = GetInstance<IBillRecordModel>();
            var patientModel = GetInstance<IPatientModel>();
            var cardModel = GetInstance<ICardModel>();
            var lp = GetInstance<LoadingProcesser>();
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
                    return Task.Run(() => Result<FormContext>.Success(default(FormContext)));
                }
                ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息(列表为空)");
                return Task.Run(() => Result<FormContext>.Fail(""));
            }
            ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息", debugInfo: billRecordModel.Res获取缴费概要信息?.msg);
            return Task.Run(() => Result<FormContext>.Fail(""));
        }
        protected override Task<Result<FormContext>> RechargeJump()
        {
            var patientModel = GetInstance<IPatientModel>();
            var gateWayStatusModel = GetInstance<IGateWayStatusModel>();

            //临时卡不允许充值
            if (string.IsNullOrWhiteSpace(patientModel.Res病人信息查询?.data[patientModel.PatientInfoIndex]?.idNo) &&
                string.IsNullOrWhiteSpace(patientModel.Res病人信息查询?.data[patientModel.PatientInfoIndex]?.guardianNo))
            {
                ShowAlert(false, "自助充值", "临时卡不允许充值，请到人工窗口补充身份信息；\r\n或直接进行银联或医保缴费");
                return Task.Run(() => Result<FormContext>.Fail(""));
            }

            //脱机状态下不允许充值
            gateWayStatusModel.Res查询网关状态 = DataHandlerEx.查询网关状态(new req查询网关状态());
            if (gateWayStatusModel.Res查询网关状态?.success ?? false)
            {
                if (gateWayStatusModel.Res查询网关状态?.data?.platform == "true")
                {
                    ShowAlert(false, "自助充值", "脱机状态下账户不可用，不可充值；\r\n请直接进行银联或医保缴费");
                    return Task.Run(() => Result<FormContext>.Fail(""));
                }
            }

            return Task.Run(() => Result<FormContext>.Success(default(FormContext)));
        }
        protected override Task<Result<FormContext>> AppointJump()
        {
            var patientModel = GetInstance<IPatientModel>();

            //临时卡不允许预约
            if (string.IsNullOrWhiteSpace(patientModel.Res病人信息查询?.data[patientModel.PatientInfoIndex]?.idNo) &&
                string.IsNullOrWhiteSpace(patientModel.Res病人信息查询?.data[patientModel.PatientInfoIndex]?.guardianNo))
            {
                ShowAlert(false, "自助预约", "临时卡不允许预约，请到人工窗口补充身份信息\r\n");
                return Task.Run(() => Result<FormContext>.Fail(""));
            }
            else
            {
                return Task.Run(() => Result<FormContext>.Success(default(FormContext)));
            }
        }

        public Task<Result<FormContext>> LisPrintJump()
        {
            var config = GetInstance<IConfigurationManager>();
            var cardInfo = GetInstance<ICardModel>();
            var patientInfo = GetInstance<IPatientModel>();
            var path = config.GetValue("Lis:Path");
            var tipsPath= config.GetValue("Lis:TipsPath");
            var tips = "";
            if (File.Exists(path))
            {
                var wd = Path.GetDirectoryName(path);
                var fname = Path.GetFileName(path);
                var patientId = patientInfo?.Res病人信息查询.data[0].patientId ?? "";
                var pinfo = new ProcessStartInfo()
                {
                    WorkingDirectory = wd,
                    FileName = fname,
                    Arguments = patientId,
                };
                Logger.Main.Info($"[检验单打印]卡号:{cardInfo?.CardNo} patientId:{patientId} 打印引擎路径:{wd} 执行文件:{fname} ");
                var pro=Process.Start(pinfo);
                SetForegroundWindow(pro.MainWindowHandle);
                Thread.Sleep(1000);
                SetForegroundWindow(pro.MainWindowHandle);
                pro.WaitForExit(20 * 1000);
                if (File.Exists(tipsPath))
                {
                    using (var sr = new StreamReader(tipsPath))
                    {
                        tips = sr.ReadToEnd();
                    }
                    File.Delete(tipsPath);
                }
                var printModel = GetInstance<IPrintModel>();
                printModel.SetPrintInfo(true, new PrintInfo
                {
                    TypeMsg = "查询成功",
                    TipMsg= tips,
                });
                //Navigate(A.JF.Print);
                return Task.Run(() => Result<FormContext>.Success(null));
            }
            ShowAlert(false, "LIS配置异常", "当前自助终端没有配置报告打印系统，请联系工作人员处理！");
            Navigate(A.Home);
            return Task.Run(() => Result<FormContext>.Fail(null));

        }
        protected void BefourCreate(NavigationEngine engine, ChoiceButtonInfo param)
        {
            ShowConfirm("门诊一般知情同意书", GetLoanAggrement(), bb =>
            {
                if (bb)
                {

                    var config = GetInstance<IConfigurationManager>();
                    if (config.GetValue("SelectCreateType") == "1")
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Select),
                            CreateJump,
                            new FormContext(A.JianDang_Context, A.JD.Print), param.Name);
                    else
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.IDCard),
                            CreateJump,
                            new FormContext(A.JianDang_Context, A.JD.Print), param.Name);
                }
                else { }
            }
                   , extend: ConfirmExModel.Build("同意", "不同意", false));
        }
        private static DockPanel GetLoanAggrement()
        {
            #region 内容
            string tttt = "";

            using (var sr = new StreamReader("知情同意书.txt"))
            {
                tttt = sr.ReadToEnd();
            }
            var rt = new RichTextBox();
            rt.IsReadOnly = true;
            rt.FontSize = 20;
            rt.AppendText(tttt);

            var viewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                Content = rt,
                Height = 640,
                //Width=400,
                ToolTip = 0,
                Margin=new Thickness(0,0,0,50),
            };
            #endregion
            #region 翻页
            var pi=(int)viewer.ToolTip;
            var ps = viewer.Height-30;
            var upbtn = new Button()
            {
                Width = 180,
                Height = 44,
                Content = "上一页",
                Margin = new Thickness(10, 10, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(195, 215, 215)),
            };
            
            upbtn.Command= new DelegateCommand<string>(t=> {
                if (pi >0)//非第一页
                { pi--;
                    viewer.ToolTip = pi;
                    viewer.ScrollToVerticalOffset(pi * ps);
                }
            });
            var downbtn = new Button()
            {
                Width = 180,
                Height = 44,
                Content = "下一页",
                Margin = new Thickness(400,10,0,0),
                Background = new SolidColorBrush(Color.FromRgb(195, 215, 215)),
            };
            downbtn.Command = new DelegateCommand<string>(t => {
                if (pi * ps < viewer.ScrollableHeight)//当前页低位置小于总高度
                {
                    pi++;
                    viewer.ToolTip = pi;
                    viewer.ScrollToVerticalOffset(pi * ps);
                }
            });
            var grid = new Grid()
            {
                Height=60,
                Width=800,
                Children = { upbtn, downbtn },
                Margin = new Thickness(-900,640,0,0),
            };
            DockPanel.SetDock(grid, Dock.Bottom);
            #endregion
            #region 签字
                
            #endregion
            var dockPanel = new DockPanel
            {
                Margin = new Thickness(10),
                Children = { viewer,  grid },
                
            };
            return dockPanel;
        }
    }
}
