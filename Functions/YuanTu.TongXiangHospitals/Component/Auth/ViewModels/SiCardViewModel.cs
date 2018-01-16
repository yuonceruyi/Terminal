using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Core.Systems;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.TongXiangHospitals.HealthInsurance;
using YuanTu.TongXiangHospitals.HealthInsurance.Model;
using YuanTu.TongXiangHospitals.HealthInsurance.Request;
using YuanTu.TongXiangHospitals.HealthInsurance.Response;
using YuanTu.TongXiangHospitals.HealthInsurance.Service;

namespace YuanTu.TongXiangHospitals.Component.Auth.ViewModels
{
    public class SiCardViewModel : Default.Component.Auth.ViewModels.SiCardViewModel
    {
        /// <summary>
        ///     未建档返回码
        /// </summary>
        private const long NoCreateCode = 3;

        private const string PasswordWinTitle = "请按回车键";
        private const long 卡片尚未插入 = -200023;

        private bool _monitorRunning;
        private bool _readCardRunning;

        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {
        }

        [Dependency]
        public ISiService SiService { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public ISiModel SiModel { get; set; }

        [Dependency]
        public ICreateModel CreateModel { get; set; }

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("插社保卡");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            SiModel.NeedCreate = false;
            StartReadSiCard();
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            StopSiMonitor();
            _readCardRunning = false;
            return base.OnLeaving(navigationContext);
        }

        protected virtual void StartReadSiCard()
        {
            Task.Run(() =>
            {
                try
                {
                    //先关闭医保接口,尝试解决医保读卡失败的问题.
                    CloseSiInterface();

                    _readCardRunning = true;
                    while (_readCardRunning)
                    {
                        Thread.Sleep(500);
                        var result = MtReader.Read();
                        if (!_readCardRunning)
                            break;
                        if (!result.IsSuccess)
                        {
                            //如果是卡片尚未插入
                            if (result.ResultCode == 卡片尚未插入)
                                continue;
                            ShowAlert(false, "温馨提示", $"读卡失败，原因：{result.Message}");
                            if (!FrameworkConst.Local)
                                Preview();
                            return;
                        }
                        _readCardRunning = false;
                        Logger.Main.Info($"读医保卡成功 返回串 {result.Value}");
                        SiModel.CardHardInfo = result.Value;
                        var strings = result.Value.Trim().Trim('$').Split('~');
                        CardModel.CardNo = strings[1];
                        SiModel.OutCardNo = strings[3];
                        Logger.Main.Info($"读医保卡成功 卡号 {CardModel.CardNo}");
                        QueryInfo();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Main.Error($"读医保卡失败 {ex.Message} {ex.StackTrace}");
                    ShowAlert(false, "温馨提示", "读卡失败，请重试！");
                    if (!FrameworkConst.Local)
                        Preview();
                }
            });
        }

        protected virtual void QueryInfo()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在获取医保个人信息，请稍候...");
                var result = GetSiPatInfo();
                if (!result.IsSuccess)
                    return;
                if (ChoiceModel.Business == Business.建档)
                {
                    Next();
                }
                else
                {
                    lp.ChangeText("正在获取HIS个人信息，请稍候...");
                    result = GetHisPatInfo();
                    if (!result.IsSuccess)
                        return;
                    Next();
                }
            });
        }

        public override void Confirm()
        {
            try
            {
                Logger.Main.Info("准备开始社保读卡");
                DoCommand(lp =>
                {
                    lp.ChangeText("正在进行读卡，请稍候...");
                    //先关闭医保接口,尝试解决医保读卡失败的问题.
                    CloseSiInterface();
                    Logger.Main.Info("正在进行社保读卡");
                    var res = MtReader.Read();
                    if (!res.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", $"读卡失败，原因：{res.Message}");
                        return;
                    }
                    Logger.Main.Info($"读医保卡成功 返回串 {res.Value}");
                    SiModel.CardHardInfo = res.Value;
                    var strings = res.Value.Trim().Trim('$').Split('~');
                    CardModel.CardNo = strings[1];
                    SiModel.OutCardNo = strings[3];
                    Logger.Main.Info($"读医保卡成功 卡号 {CardModel.CardNo}");

                    if (ChoiceModel.Business == Business.建档)
                    {
                        lp.ChangeText("正在获取医保个人信息，请稍候...");
                        var result = GetSiPatInfo();
                        if (!result.IsSuccess)
                            return;
                        Next();
                    }
                    else
                    {
                        lp.ChangeText("正在获取医保个人信息，请稍候...");
                        var result = GetSiPatInfo();
                        if (!result.IsSuccess)
                            return;
                        lp.ChangeText("正在获取HIS个人信息，请稍候...");
                        result = GetHisPatInfo();
                        if (!result.IsSuccess)
                            return;
                        Next();
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"读医保卡失败 {ex.Message} {ex.StackTrace}");
                ShowAlert(false, "温馨提示", "读卡失败，请重试！");
            }
        }

        public virtual Result GetHisPatInfo()
        {
            PatientModel.Req病人信息查询 = new req病人信息查询
            {
                cardNo = CardModel.CardNo,
                cardType = ((int) CardModel.CardType).ToString(),
                secrityNo = SiModel.CardHardInfo,
                extend = SiModel.SiPatientInfo
            };
            PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

            if (PatientModel.Res病人信息查询.success)
            {
                if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                    return Result.Fail("未查询到病人的信息(列表为空)");
                }
                return Result.Success();
            }
            if (PatientModel.Res病人信息查询.code == NoCreateCode)
            {
                //未建档特殊处理
                SiModel.NeedCreate = true;
                CreateModel.CreateType = CreateType.成人;
                return Result.Success();
            }
            ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
            return Result.Fail("未查询到病人的信息");
        }

        public virtual Result GetSiPatInfo()
        {
            var res = SiService.Initialize();
            if (!res.IsSuccess)
            {
                ShowAlert(false, "获取医保信息", $"{res.Message}");
                return Result.Fail(res.Message);
            }
            var reqSiPatientInfo = new Req获取参保人信息
            {
                卡类型 = "1",
                Ic卡信息 = CardModel.CardNo,
                现金支付方式 = "1",
                银行卡信息 = "",
                读卡方式 = "10"
            };
            StartSiMonitor();
            res = SiService.GetSiPatientInfo(reqSiPatientInfo);
            StopSiMonitor();
            if (!res.IsSuccess)
            {
                ShowAlert(false, "获取医保信息", $"{res.Message}");
                return Result.Fail(res.Message);
            }
            SiModel.SiPatientInfo = SiModel.RetMessage;
            return Result.Success();
        }

        protected virtual void StartSiMonitor()
        {
            _monitorRunning = true;
            Task.Factory.StartNew(() =>
            {
                while (_monitorRunning)
                {
                    Thread.Sleep(200);
                    if(CheckPasswordWindow())
                        _monitorRunning = false;
                }
            });
        }

        protected bool CheckPasswordWindow()
        {
            //获取密码框
            var pwdPtr = WindowHelper.FindWindow(null, PasswordWinTitle);
            if (pwdPtr == IntPtr.Zero)
                return false;
            WindowHelper.SendKey(pwdPtr, WindowHelper.VK_RETURN);
            ShowAlert(false, "温馨提示", "有密码的社保卡请到人工窗口办理业务");
            return true;
        }

        protected virtual void StopSiMonitor()
        {
            _monitorRunning = false;
        }

        protected virtual void CloseSiInterface()
        {
            var ret = SiService.Close();
            if (!ret.IsSuccess)
                Logger.Main.Warn($"医保读卡前，医保接口关闭失败");
            Logger.Main.Info($"医保读卡前，医保接口关闭成功");
        }

        public override void DoubleClick()
        {
            if (ChoiceModel.Business == Business.建档)
            {
                SiModel.医保个人基本信息 = new 个人基本信息
                {
                    姓名 = "远图测试医保",
                    性别 = "1",
                    公民身份号 = "342901199110312060",
                    出生日期 = "2016-11-11",
                    单位名称 = "浙江远图"
                };
                CardModel.CardNo = "1234567890";
                Next();
            }
            else
            {
                var ret = InputTextView.ShowDialogView("输入测试卡号");
                CardModel.CardNo = ret.Value;
                SiModel.CardHardInfo =
                    "1~330483D156000005000C4B7BF1AC651C~3~F24680759~330205197303024829~姜磊芬~2~330483~330400817818~";
                SiModel.SiPatientInfo =
                    "$$0~~~~~330483D156000005000C4B7BF1AC651C%%0026800%%姜磊芬%%2%%01%%19730302%%330205197303024829%%自收自支事业单位%%桐乡市第一人民医院%%330483%%桐乡市%%11%%0%%0%%0%%1%%%%910.23%%2006.67%%0%%1119.81%%0%%0%%0%%0%%0%%0%%0%%0%%%%20~000000011000000~~~$$";
                DoCommand(ctx =>
                {
                    PatientModel.Req病人信息查询 = new req病人信息查询
                    {
                        cardNo = CardModel.CardNo,
                        cardType = ((int) CardModel.CardType).ToString(),
                        extend = SiModel.SiPatientInfo,
                        secrityNo = SiModel.CardHardInfo
                    };
                    PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

                    if (PatientModel.Res病人信息查询.success)
                    {
                        if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                        {
                            ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                            return;
                        }
                        Next();
                    }
                    else
                    {
                        if (PatientModel.Res病人信息查询.code == NoCreateCode)
                        {
                            //未建档特殊处理
                            SiModel.NeedCreate = true;
                            CreateModel.CreateType = CreateType.成人;
                            Next();
                            return;
                        }
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                    }
                });
            }
        }
    }
}