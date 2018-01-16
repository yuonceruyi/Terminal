using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;

namespace YuanTu.ZheJiangHospitalSanDun.Component.Auth.ViewModels
{
    class PatientInfoViewModel : Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser) : base(rfCardDispenser)
        {
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            CanUpdatePhone = false;
            if (ChoiceModel.Business == Business.建档 && CardModel.CardType == CardType.身份证)
            {
                Balance = null;
            }
            else if (ChoiceModel.Business == Business.建档 && CardModel.CardType == CardType.社保卡)
            {
                //todo 社保卡建档处理
            }
            else
            {
                var patientInfo = PatientModel.当前病人信息;
                Balance = patientInfo.accBalance.In元();
            }
        }

        protected override bool GetNewCardNo()
        {
            try
            {
                if (FrameworkConst.DoubleClick && FrameworkConst.VirtualHardWare)
                    return Invoke(() =>
                    {
                        var ret = InputTextView.ShowDialogView("输入物理卡号");
                        if (ret.IsSuccess)
                        {
                            CardModel.CardNo = ret.Value;
                            return true;
                        }
                        return false;
                    });

                if (!_magCardDispenser.Connect().IsSuccess)
                {
                    ReportService.发卡器离线(null, ErrorSolution.发卡器离线);
                    ShowAlert(false, "建档发卡", "发卡机连接失败");
                    return false;
                }
                if (!_magCardDispenser.Initialize().IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机初始化失败");
                    return false;
                }
                int retry = 3;
                Result<Dictionary<TrackRoad, string>> result;
                do
                {
                    result = _magCardDispenser.EnterCard(TrackRoad.Trace2);
                    if (result.IsSuccess)
                        break;
                    //读卡失败时,回收卡片重新发卡
                    if (!_magCardDispenser.MoveCard(CardPosF6.吞入, "发卡机读卡号失败，故回收卡片").IsSuccess)
                    {
                        ShowAlert(false, "建档发卡", "发卡机读卡号失败后回收卡失败");
                        return false;
                    }
                } while (retry-- > 0);
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机取新卡失败");
                    return false;
                }

                var track = result.Value[TrackRoad.Trace2];
                if (!track.Contains('='))
                {
                    ShowAlert(false, "建档发卡", "发卡机读卡号格式不对 请确认卡箱内卡的类型");
                    if (!_magCardDispenser.MoveCard(CardPosF6.吞入, "发卡机读卡号失败，故回收卡片").IsSuccess)
                        ShowAlert(false, "建档发卡", "发卡机读卡号失败后回收卡失败");
                    return false;
                }
                CardModel.CardNo = track;
                return true;
            }
            catch (Exception ex)
            {
                ShowAlert(false, "建档发卡", "发卡机读卡失败");
                Logger.Main.Error($"[建档发卡]{ex.Message + ex.StackTrace}");
                return false;
            }
        }

        protected override void CreatePatient()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在准备就诊卡，请稍候...");
                if (!GetNewCardNo())
                    return;

                var track = CardModel.CardNo;
                var cardNo = track.Substring(0, track.IndexOf('='));
                var req = new req病人建档发卡
                {
                    cardNo = cardNo,
                    cardType = "2",

                    name = IdCardModel.Name,
                    sex = IdCardModel.Sex.ToString(),
                    birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                    idNo = IdCardModel.IdCardNo,
                    idType = "1",
                    nation = IdCardModel.Nation,
                    address = IdCardModel.Address,
                    phone = Phone,
                    setupType = ((int)CreateModel.CreateType).ToString(),
                    extend = $"{IdCardModel.IdCardNo}|{IdCardModel.Birthday:yyyyMMdd}#{track}##"
                };
                CreateModel.Req病人建档发卡 = req;
                lp.ChangeText("正在建档，请稍候...");
                var res = DataHandlerEx.病人建档发卡(req);
                CreateModel.Res病人建档发卡 = res;
                if (res == null || !res.success)
                {
                    //if (CardModel.CardType != CardType.社保卡 && res?.code == PatientCreatedCode)
                    //    _magCardDispenser.MoveCard(CardPosF6.吞入, $"此卡[{CardModel.CardNo}]已建档，故回收卡片");

                    ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: res?.msg);
                    return;
                }

                lp.ChangeText("正在发卡，请及时取卡。");
                if (!FrameworkConst.DoubleClick)
                {
                    var result = _magCardDispenser.MoveCardOut();
                    if (!result.IsSuccess)
                        _magCardDispenser.MoveCard(CardPosF6.吞入, "弹卡到前端失败，故回收卡片");
                }

                PrintModel.SetPrintInfo(true, new PrintInfo
                {
                    TypeMsg = "建档成功",
                    TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分建档",
                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                    Printables = base.CreatePrintables(),
                    TipImage = "提示_凭条和发卡"
                });

                ChangeNavigationContent($"{Name}\r\n卡号{CardModel.CardNo}");
                Navigate(A.JD.Print);
            });
        }

        protected override Queue<IPrintable> CreatePrintables()
        {
            var track = CardModel.CardNo;
            var cardNo = track.Substring(0, track.IndexOf('='));
            var queue = PrintManager.NewQueue("自助发卡");

            var sb = new StringBuilder();
            sb.Append($"状态：办卡成功\n");
            sb.Append($"发卡单位：{FrameworkConst.HospitalName}\n");
            sb.Append($"姓名：{IdCardModel.Name}\n");
            sb.Append($"就诊卡号：{cardNo}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的个人信息。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

        private string _balance;

        public string Balance
        {
            get { return _balance; }
            set
            {
                _balance = value; 
                OnPropertyChanged();
            }
        }
    }
}
