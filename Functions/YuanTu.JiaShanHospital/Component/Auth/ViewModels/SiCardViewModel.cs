using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
using YuanTu.Devices.CardReader;
using YuanTu.JiaShanHospital.HealthInsurance;
using YuanTu.JiaShanHospital.HealthInsurance.Model;
using YuanTu.JiaShanHospital.HealthInsurance.Request;
using YuanTu.JiaShanHospital.HealthInsurance.Service;

namespace YuanTu.JiaShanHospital.Component.Auth.ViewModels
{
    public class SiCardViewModel : Default.Component.Auth.ViewModels.SiCardViewModel
    {
        [Dependency]
        public ISiModel SiModel { get; set; }
        [Dependency]
        public ISiService SiService { get; set; }
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }
        [Dependency]
        public ICreateModel CreateModel { get; set; }
        /// <summary>
        ///     未建档返回码
        /// </summary>
        private const long NoCreateCode = 3;
        private bool _readCardRunning;

        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {
        }
        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("社保卡插卡口");
            CardUri = ResourceEngine.GetImageResourceUri("社保卡");
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            SiModel.NeedCreate = false;
            StartReadSiCard();

        }
        public override void Confirm()
        {
            
        }

        protected virtual void StartReadSiCard()
        {
            Task.Run(() =>
            {
                try
                {
                    //先关闭医保接口,尝试解决医保读卡失败的问题.
                    //CloseSiInterface();
                    _readCardRunning = true;
                    while (_readCardRunning)
                    {
                        Thread.Sleep(1000);
                        var result = MTReader();
                        if (!_readCardRunning)
                            break;
                        if (!result.IsSuccess)
                        {
                            continue;
                        }
                        _readCardRunning = false;
                        Logger.Main.Info($"读医保卡成功 返回串 {result.Value}");
                        SiModel.CardHardInfo = result.Value;
                        var strArr = result.Value.Trim().Trim('$').Split('~');
                        CardModel.CardNo = strArr[2].Length<8? strArr[3]: strArr[2];
                        Logger.Main.Info($"读医保卡成功 卡号 {CardModel.CardNo}");
                        CardModel.CardType = CardType.社保卡;
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
                if (ChoiceModel.Business == Business.建档)
                {
                    Next();
                }
                else
                {
                    lp.ChangeText("正在获取HIS个人信息，请稍候...");
                     var result = GetHisPatInfo();
                    if (!result.IsSuccess)
                        return;
                    Next();
                }
            });
        }
        public virtual Result GetHisPatInfo()
        {
            PatientModel.Req病人信息查询 = new req病人信息查询
            {
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),
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
                CardModel.CardNo = PatientModel.Res病人信息查询.data[0].cardNo;
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

        private Result<string> MTReader()
        {
            var da = new Process
            {
                StartInfo =
                {
                    FileName = "MT_CardRead",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Arguments = "1"
                }
            };
            da.Start();
            var outputStr = da.StandardOutput.ReadToEnd().Trim();
            Logger.Device.Info($"明泰读卡返回{outputStr}");
            if (!string.IsNullOrEmpty(outputStr) && outputStr.Split('|').Length==2)
            {
                if (bool.Parse(outputStr.Split('|')[0]))
                {
                    return Result<string>.Success(outputStr.Split('|')[1]);
                }
            }
            return Result<string>.Fail("");
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            _readCardRunning = false;
            return base.OnLeaving(navigationContext);
        }
    }
}
