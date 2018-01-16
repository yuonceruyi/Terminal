using System;
using System.Collections.Generic;
using System.Text;
using YuanTu.ChongQingArea.Component.Auth.Views;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Devices.CardReader;
using YuanTu.Devices.FingerPrint;

namespace YuanTu.ChongQingArea.Component.Auth.ViewModels
{
    public class PatientInfoExViewModel : Default.Component.Auth.ViewModels.PatientInfoExViewModel
    {
        private IFingerPrintDevice[] _fingerPrintDevices;
        public PatientInfoExViewModel(IRFCardDispenser[] rfCardDispenser, IFingerPrintDevice[] fingerPrintDevices) : base(rfCardDispenser)
        {
            _fingerPrintDevices = fingerPrintDevices;
        }
        public override void Confirm()
        {
            if (Name.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "温馨提示", "请输入就诊人姓名");
                return;
            }
            if (!Name.IsHanzi())
            {
                ShowAlert(false, "温馨提示", $"您输入的姓名[{Name}]中存在特殊，请重新输入！如确实有特殊字符，请到窗口处理");
                return;
            }
            if (!IsBoy && !IsGirl)
            {
                ShowAlert(false, "温馨提示", "请选择就诊人性别");
                return;
            }
            if (DateTime.CompareTo(DateTimeCore.Now) > 0)
            {
                ShowAlert(false, "温馨提示", "出生日期不能大于当前日期");
                return;
            }

            if (Startup.Biometric)
                CreatePatient2();
            else
                DoCommand(CreatePatient);
        }

        public List<string> fingetDataList = new List<string>();
        private void CreatePatient2()
        {
            var viewModel = new FingerPrintViewModel(_fingerPrintDevices)
            {
                Action = (lp, s) =>
                {
                    fingetDataList = s;
                    ShowMask(false);
                    CreatePatient(lp);
                },
                ResourceEngine = ResourceEngine,
            };
            var element = new FingerPrintView()
            {
                DataContext = viewModel,
            };
            viewModel.View = element;
            ShowMask(true, element, 0.4, p =>
            {
                viewModel.OnLeaving(null);
                ShowMask(false);
            });
            viewModel.OnSet();
            viewModel.OnEntered(null);
        }

        protected void CreatePatient(LoadingProcesser lp)
        {
            lp.ChangeText("正在准备就诊卡，请稍候...");
            if (GetNewCardNo())
            {
                string cn = CardModel.CardNo;
                Logger.Main.Info($"读卡号={CardModel.CardNo}");
                if (WriteCardNos(cn)) { }
                else
                {
                    ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: "读卡号失败");
                    return;
                }
            }
            else
            {
                ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: "读卡号失败");
                return;
            }

            CreateModel.Req病人建档发卡 = new req病人建档发卡
            {
                operId = FrameworkConst.OperatorId,
                cardNo = CardModel.CardNo,
                cardType = "2", //发就诊卡
                name = Name.Trim(),
                sex = IsBoy ? Sex.男.ToString() : Sex.女.ToString(),
                birthday = DateTime.ToString("yyyy-MM-dd"),
                idNo = "",
                idType = "1", //测试必传
                nation = IdCardModel.Nation,
                address = IdCardModel.Address,
                phone = CreateModel.Phone,
#pragma warning disable 612
                guardianName = IdCardModel.Name,
                guardianNo = IdCardModel.IdCardNo,
#pragma warning restore 612
                pwd = "123456",
                tradeMode = "CA",
                setupType = ((int)CreateModel.CreateType).ToString(),
                extend = IdCardModel.GrantDept, //职业
            };
            lp.ChangeText("正在建档，请稍候...");
            CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);
            if (CreateModel.Res病人建档发卡?.success ?? false)
            {
                #region 指纹上传
                try
                {
                    Logger.Main.Info("开始指纹上传");
                    var 指纹 = new req指纹信息上传()
                    {
                        idNo = IdCardModel.IdCardNo,
                        //name = IdCardModel.Name,
                        name = Name.Trim(),
                        guardianName = IdCardModel.Name,
                        guardianNo= IdCardModel.IdCardNo,
                        cardNo = CardModel.CardNo,
                        cardType = "2",
                        sex = IdCardModel.Sex.ToString(),
                        address = IdCardModel.Address,
                        phone = CreateModel.Phone,
                        rightFinger = fingetDataList[0],
                        leftFinger = fingetDataList[1],
                    };
                    var r = DataHandlerEx.指纹信息上传(指纹);
                    Logger.Main.Info("指纹上传" + (r.success ? "成功" : "失败"));
                }
                catch (Exception ex)
                {
                    Logger.Main.Info("指纹上传异常 " + ex.Message + "\r\n" + ex.StackTrace);
                }
                #endregion
                lp.ChangeText("正在发卡，请及时取卡。");
                if (!FrameworkConst.DoubleClick)
                    PrintCard();
                PrintModel.SetPrintInfo(true, new PrintInfo
                {
                    TypeMsg = "建档发卡成功",
                    TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分建档",
                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                    Printables = CreatePrintables(),
                    TipImage = "提示_凭条和发卡"
                });

                ChangeNavigationContent($"{Name}\r\n卡号{CardModel.CardNo}");
                Next();
            }
            else
            {
                ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
            }
        }
        protected bool WriteCardNos(string sCardNo)
        {
            try
            {
                sCardNo = sCardNo.PadLeft(10, '0');
                Logger.Main.Info($"准备写卡 卡号={sCardNo}");
                var b01 = sCardNo.StringToByte();
                Logger.Main.Info($"卡号StringToByte完成");
                var r_B01 = _rfCardDispenser.ReadBlock(0x00, 0x01, true, Startup.SiKey);
                if (r_B01.IsSuccess)
                {
                    Logger.Main.Info($"读01块成功 {r_B01.Value.ByteToString()}");
                    if (r_B01.IsSuccess && r_B01.Value == b01)
                    {
                        Logger.Main.Info($"卡号相同，无需写卡");
                        return true;
                    }
                    Logger.Main.Info($"需写卡");
                    var w_B01 = _rfCardDispenser.WirteBlock(0x00, 0x01, true, Startup.SiKey, b01);
                    Logger.Main.Info($"写卡完成 结果={w_B01.IsSuccess}");
                    return w_B01.IsSuccess;
                }
                else
                { return false; }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"异常 {ex.Message}\r\n{ex.StackTrace}");
                return false;
            }
        }

        protected override void PrintCard()
        {
            var printText = new List<ZbrPrintTextItem>
            {
                new ZbrPrintTextItem
                {
                    X = 160,
                    Y = 55,
                    Text = Name
                },
                new ZbrPrintTextItem
                {
                    X = 550,
                    Y = 55,
                    FontSize = 11,
                    Text = CreateModel.Res病人建档发卡.data.patientCard
                }
            };
            _rfCardDispenser.PrintContent(printText);
        }
        protected override Queue<IPrintable> CreatePrintables()
        {
            var queue = PrintManager.NewQueue("自助发卡");

            var sb = new StringBuilder();
            sb.Append($"状态：办卡成功\n");
            sb.Append($"发卡单位：{FrameworkConst.HospitalName}\n");
            sb.Append($"姓名：{Name}\n");
            //sb.Append($"就诊卡号：{CardModel.CardNo}\n");
            sb.Append($"就诊卡号：{CreateModel.Res病人建档发卡.data.patientCard}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请及时取走卡片\n");
            sb.Append($"请妥善保管好您的个人信息\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}