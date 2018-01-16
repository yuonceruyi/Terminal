using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.ChongQingArea.Component.Auth.Models;
using YuanTu.ChongQingArea.Services;
using YuanTu.ChongQingArea.SiHandler;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Gateway;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.Consts.Models.Register;
using YuanTu.ChongQingArea.Models.Payment;

namespace YuanTu.ChongQingArea.Clinic.Component.Auth.ViewModels
{
    public class ZjSiCardViewModel : Default.Component.Auth.ViewModels.SiCardViewModel
    {
        public ZjSiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders, IRFCardDispenser[] rfCardDispenser) : base(icCardReaders, rfCpuCardReaders)
        {
            _rfCardDispenser = rfCardDispenser?.FirstOrDefault(p => p.DeviceId == "ZBR_RF");
        }
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public ICreateModel CreateModel { get; set; }

        [Dependency]
        public ISiModel SiModel { get; set; }

        [Dependency]
        public IRegisterModel RegisterModel { get; set; }

        [Dependency]
        public IScheduleModel ScheduleModel { get; set; }

        [Dependency]
        public IPaymentModels PaymentModel { get; set; }

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("提示_诊间社保卡");
            CardUri = ResourceEngine.GetImageResourceUri("动画素材_社保卡");
        }

        protected IRFCardDispenser _rfCardDispenser;
        public override void Confirm()
        {
            Logger.Main.Info("进入医保卡身份验证");
            DoCommand(lp =>
            {
                lp.ChangeText("正在进行读卡，请稍候...");
                var result = SiInterface.读卡交易(new Req读卡交易());
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "读医保卡失败", $"读医保卡失败:{result.Message}");
                    return;
                }

                var res = result.Value;
                var cardNo = res.社保卡卡号;
                if (cardNo.IsNullOrWhiteSpace())
                {
                    ShowAlert(false, "病人信息查询", "卡片信息为空，请确认插卡方向是否正确和卡片是否有效");
                    return;
                }
                SiModel.社保卡卡号 = cardNo;
                lp.ChangeText("正在获取医保个人信息，请稍候...");
                var qResult = SiInterface.获取人员基本信息(new Req获取人员基本信息()
                {
                    险种类别 = "1",
                    社保卡卡号 = cardNo,
                });
                if (!qResult.IsSuccess)
                {
                    ShowAlert(false, "病人信息查询", "医保人员基本信息查询失败:" + qResult.Message);
                    return;
                }
                var siInfo = qResult.Value;
                SiModel.Res获取人员基本信息 = siInfo;

                if (SiModel.UseSi)
                {//使用社保支付，再来到此页面，说明是其他持卡类型患者插入社保卡
                    Logger.Main.Info("进入验证HIS个人信息");
                    lp.ChangeText("正在校验HIS个人信息，请稍候...");
                    var zh = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                    Logger.Main.Info($"信息校验 社保卡:{SiModel.Res获取人员基本信息.姓名}|{SiModel.Res获取人员基本信息.身份证号} 账户:{zh.name}|{zh.idNo}");
                    if (zh.idNo == SiModel.Res获取人员基本信息.身份证号
                        && zh.name == SiModel.Res获取人员基本信息.姓名)
                    {
                        Logger.Main.Info("身份验证成功");
                        try
                        {
                            //CardModel.CardNo = cardNo;
                            SiModel.ConfirmSi?.BeginInvoke(cp =>
                            {
                                Logger.Main.Info("开始SiModel.ConfirmSi");
                                var rest = SiModel.ConfirmSi?.EndInvoke(cp);
                                Logger.Main.Info("结束SiModel.ConfirmSi");
                                if (rest?.IsSuccess ?? false)
                                {
                                    Logger.Main.Info("SiModel.ConfirmSi成功");
                                    PaymentModel.NoPay = (PaymentModel.Self == 0);

                                    if (PaymentModel.FundPayment > 0)
                                    {
                                        PaymentModel.RightList = new List<PayInfoItem>()
                                        {
                                            new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                                            new PayInfoItem("医保统筹支付：",PaymentModel.FundPayment.In元()),
                                            new PayInfoItem("医保帐户支付：",PaymentModel.AccountPay.In元()),
                                        };
                                    }
                                    else if (PaymentModel.FundPayment < 0)
                                    {
                                        PaymentModel.RightList = new List<PayInfoItem>()
                                        {
                                            new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                                            new PayInfoItem("医保统筹支付：",PaymentModel.FundPayment.In元()),
                                            new PayInfoItem("医保帐户支付：",PaymentModel.AccountPay.In元()),
                                            new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
                                        };
                                    }
                                    else if (PaymentModel.CivilServant > 0)
                                    {
                                        PaymentModel.RightList = new List<PayInfoItem>()
                                        {
                                            new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                                            new PayInfoItem("医保统筹支付：",PaymentModel.FundPayment.In元()),
                                            new PayInfoItem("医保帐户支付：",PaymentModel.AccountPay.In元()),
                                            new PayInfoItem("公务员补：",PaymentModel.CivilServant.In元()),
                                            new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
                                        };
                                    }
                                    else
                                    {
                                        PaymentModel.RightList = new List<PayInfoItem>()
                                        {
                                            new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                                            new PayInfoItem("医保统筹支付：",PaymentModel.FundPayment.In元()),
                                            new PayInfoItem("医保帐户支付：",PaymentModel.AccountPay.In元()),
                                            new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
                                        };
                                    }

                                    if (ChoiceModel.Business == Business.挂号)
                                    {
                                        StackNavigate(A.XianChang_Context, A.XC.Confirm);
                                    }
                                    else if (ChoiceModel.Business == Business.缴费)
                                    {
                                        StackNavigate(A.JiaoFei_Context, A.JF.Confirm);

                                    }
                                    else if (ChoiceModel.Business == Business.出院结算)
                                    {
                                        StackNavigate(B.ChuYuanJieSuan_Context, B.CY.Confirm);
                                    }
                                    else if (ChoiceModel.Business == Business.取号)
                                    {
                                        StackNavigate(A.QuHao_Context, A.QH.Confirm);
                                    }
                                    else
                                    {
                                        Next();
                                    }
                                }
                                else
                                {
                                    Logger.Main.Info("SiModel.ConfirmSi失败");
                                    ShowAlert(false, "医保预结算", "医保预结算失败:" + rest?.Message);

                                    if (ChoiceModel.Business == Business.挂号)
                                    {
                                        UnlockSource();
                                    }
                                    return;
                                }
                            }, null);
                        }
                        catch (Exception ex)
                        {
                            Logger.Main.Info("报错 " + ex.Message + "\r\r" + ex.StackTrace);
                            if (ChoiceModel.Business == Business.挂号)
                            {
                                UnlockSource();
                            }
                            return;
                        }
                    }
                    else
                    {
                        Logger.Main.Info("身份验证失败");
                        ShowAlert(false, "身份验证", "社保卡与就诊卡身份不一致，不能进行社保结算");
                        if (ChoiceModel.Business == Business.挂号)
                        {
                            UnlockSource();
                        }
                        return;
                    }
                    //PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                    //PatientModel.Res病人信息查询.data[0].idNo
                }
                else//其他业务
                {
                    CardModel.CardNo = cardNo;
                    Logger.Main.Info("正常读社保卡 cardNo=" + cardNo);
                    lp.ChangeText("正在获取HIS个人信息，请稍候...");
                    Query(siInfo);
                }
            });
        }

        /// <summary>
        /// 挂号解锁
        /// </summary>
        private void UnlockSource()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在解除锁号,请稍后......");
                var scheduleInfo = ScheduleModel.所选排班;
                var lock01 = new req挂号解锁
                {
                    operId = FrameworkConst.OperatorId,
                    medDate = scheduleInfo.medDate,
                    scheduleId = scheduleInfo.scheduleId,
                    lockId = RegisterModel.Res挂号锁号?.data?.lockId,
                };
                RegisterModel.Res挂号解锁 = DataHandlerEx.挂号解锁(lock01);
            });
        }

        protected virtual void Query(Res获取人员基本信息 siInfo)
        {
            IdCardModel.Name = siInfo.姓名;
            IdCardModel.Sex = siInfo.性别.SafeToSex();
            IdCardModel.IdCardNo = siInfo.身份证号;
            IdCardModel.Address = siInfo.住址;
            IdCardModel.Birthday = Convert.ToDateTime(siInfo.身份证号.Substring(6, 8).SafeConvertToDate("yyyyMMdd", "yyyy-MM-dd"));
            IdCardModel.Nation = siInfo.民族;

            Logger.Main.Info($"信息查询{siInfo.姓名}|{siInfo.身份证号}");
            PatientModel.Req病人信息查询 = new req病人信息查询
            {
                Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                cardNo = CardModel.CardNo,
                cardType = ((int)CardType.社保卡).ToString(),
                patientName = siInfo.姓名
            };
            var res = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
            PatientModel.Res病人信息查询 = res;
            SiModel.NeedCreate = false;
            //查询如果失败，进入自动激活或者提示首页建档流程
            if ((!res.success) || (!res.data.Any()))
            {
                var code = res.code;
                if (DataHandler.UnKnowErrorCode.Contains(code))
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                }
                else//用身份证查一次
                {
                    var req = new req病人信息查询
                    {
                        Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                        cardNo = siInfo.身份证号,
                        cardType = ((int)CardType.身份证).ToString(),
                        patientName = siInfo.姓名
                    };
                    var patRes = DataHandlerEx.病人信息查询(req);
                    if (patRes.success)//平台有卡，自动激活社保卡
                    {
                        Logger.Main.Info("新社保卡自动激活");
                        if (ActivitySICard(patRes.data[0]))
                        {
                            Navigate(A.CK.Info);
                        }
                        return;
                    }
                    ShowAlert(false, "病人信息查询", "您未在本系统中建过档，请到发卡设备进行建档发卡", debugInfo: PatientModel.Res病人信息查询.msg, extend: new AlertExModel()
                    {
                        HideCallback = tp =>
                        {
                            Navigate(A.Home);
                        }
                    });
                }

            }
            else
            {
                Navigate(A.CK.Info);
            }
        }
        private bool ActivitySICard(病人信息 info)
        {
            var Req社保自动激活 = new req病人建档发卡
            {
                operId = FrameworkConst.OperatorId,
                cardNo = CardModel.CardNo,
                cardType = ((int)CardType.社保卡).ToString(),
                name = info.name,
                sex = info.sex,
                birthday = DateTime.Parse(info.birthday).ToString("yyyy-MM-dd"),
                idNo = info.idNo,
                idType = "1", //测试必传
                address = info.address,
                phone = info.phone,
                setupType = "1",

            };

            var 社保激活 = DataHandlerEx.病人建档发卡(Req社保自动激活);
            if (!社保激活.success)
            {
                ShowAlert(false, "建档发卡", "自动激活失败，请重试", debugInfo: 社保激活?.msg);
                return false;
            }
            else
            {
                var res = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
                PatientModel.Res病人信息查询 = res;
                if (!res.success)
                {
                    ShowAlert(false, "建档发卡", "社保卡自动激活失败，请重试", debugInfo: res.msg);
                    return false;
                }
                return true;
            }
        }

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试身份证号和姓名");
            if (!ret.IsSuccess)
                return;
            var list = ret.Value.Replace("\r\n", "\n").Split('\n');
            if (list.Length < 2)
                return;
            CardModel.CardNo = list[0];
            IdCardModel.Name = list[1];

            IdCardModel.Sex = Convert.ToInt32(CardModel.CardNo[16]) % 2 == 0 ? Sex.女 : Sex.男;
            IdCardModel.IdCardNo = CardModel.CardNo;
            IdCardModel.Address = "浙江杭州西湖";
            IdCardModel.Birthday = DateTime.Parse(
                $"{CardModel.CardNo.Substring(6, 4)}-{CardModel.CardNo.Substring(10, 2)}-{CardModel.CardNo.Substring(12, 2)}");
            IdCardModel.Nation = "汉";
            IdCardModel.GrantDept = "远图";
            IdCardModel.ExpireDate = DateTimeCore.Now;
            IdCardModel.EffectiveDate = DateTimeCore.Now.AddYears(10);
            Confirm();
        }
        //[Dependency]
        //public IPaymentModel PaymentModel { get; set; }
        [Dependency]
        public IReSendModel ReSendModel { get; set; }
        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }
        [Dependency]
        public IPrintModel PrintModel { get; set; }
        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }
        [Dependency]
        public IPrintManager PrintManager { get; set; }

        protected Result WriteCardNo(string cardNo)
        {
            cardNo = cardNo.PadLeft(10, '0');
            var readRet = _rfCardDispenser.ReadBlock(0x00, 0x01, true, Startup.SiKey);
            if (readRet && readRet.Value.ByteToString() == cardNo) { return Result.Success(); }

            var cardNoBts = cardNo.StringToByte();
            var writeRet = _rfCardDispenser.WirteBlock(0x00, 0x01, true, Startup.SiKey, cardNoBts);
            if (writeRet)
            {
                return Result.Success();
            }
            Logger.Device.Error($"[发卡写卡]写卡失败,{writeRet.Message}");
            return writeRet;
        }
        protected void PrintCard()
        {
            var printText = new List<ZbrPrintTextItem>
            {
                new ZbrPrintTextItem()
                {
                    X = 160,
                    Y = 55,
                    Text = ReSendModel.name
                },
                new ZbrPrintTextItem()
                {
                    X = 550,
                    Y = 55,
                    FontSize = 11,
                    Text =  ReSendModel.Res补卡查询.data[0].patientCard
                }
            };
            _rfCardDispenser.PrintContent(printText);
        }
    }
}