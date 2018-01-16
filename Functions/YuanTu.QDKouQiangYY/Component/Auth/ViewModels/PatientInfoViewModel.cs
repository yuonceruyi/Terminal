using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Core.Extension;
using YuanTu.Devices.CardReader;
using YuanTu.Consts.Gateway;
using YuanTu.Consts;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Consts.Models.Print;
using YuanTu.QDArea.Card;
using YuanTu.QDArea;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tools;
using YuanTu.Core.Log;
using System.Windows.Input;
using Prism.Commands;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Default.Component.Auth.Dialog.Views;

namespace YuanTu.QDKouQiangYY.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : YuanTu.Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser) : base(rfCardDispenser)
        {

        }
        public ICommand ModifyNameCommand { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            ModifyNameCommand = new DelegateCommand(ModifyNameCmd);

            if (ChoiceModel.Business == Business.建档 && CardModel.CardType == CardType.身份证)
            {
                Name = IdCardModel.Name;
                Sex = IdCardModel.Sex.ToString();
                Birth = IdCardModel.Birthday.ToString("yyyy-MM-dd");
                Phone = null;
                IdNo = IdCardModel.IdCardNo.Mask(14, 3);
                IsAuth = false;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;
                NameBorderThick = 0;
            }
            else if (ChoiceModel.Business == Business.建档 && CardModel.CardType == CardType.NoCard)
            {
                Name = IdCardModel.Name;
                Sex = IdCardModel.Sex.ToString();
                Birth = IdCardModel.Birthday.ToString("yyyy-MM-dd");
                Phone = null;
                IdNo = IdCardModel.IdCardNo.Mask(14, 3);
                IsAuth = false;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;
                NameBorderThick = 1;
                ModifyNameCommand.Execute(null);
            }
            else if (CardModel.ExternalCardInfo == "建档" && CardModel.CardType == CardType.社保卡)
            {
                Name = IdCardModel.Name;
                Sex = IdCardModel.Sex.ToString();
                Birth = IdCardModel.Birthday.ToString("yyyy-MM-dd");
                #region 已建档患者，查询电话信息
                PatientModel.Req病人信息查询 = new req病人信息查询
                {
                    cardNo = IdCardModel.IdCardNo,
                    patientName = IdCardModel.Name,
                    cardType = ((int)CardType.身份证).ToString()
                };
                PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
                var patientInfo = PatientModel.Res病人信息查询.data?[PatientModel.PatientInfoIndex];
                Phone = patientInfo?.phone;
                #endregion
                IdNo = IdCardModel.IdCardNo.Mask(14, 3);
                IsAuth = false;
                ShowUpdatePhone = false;
                NameBorderThick = 0;
                if (string.IsNullOrEmpty(Phone))
                {
                    CanUpdatePhone = true;
                }
                else
                {
                    CanUpdatePhone = false;
                }
            }
            else
            {
                IsAuth = true;
                ShowUpdatePhone = false;
                CanUpdatePhone = false;
                NameBorderThick = 0;
                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                Name = patientInfo.name;
                Sex = patientInfo.sex;
                Birth = patientInfo.birthday.SafeToSplit(' ', 1)[0];
                Phone = patientInfo.phone.Mask(3, 4);
                IdNo = patientInfo.idNo.Mask(14, 3);
                GuardIdNo = patientInfo.guardianNo.Mask(14, 3);
            }
        }


        public override bool OnLeaving(NavigationContext navigationContext)
        {
            ShowMask(false);
            return base.OnLeaving(navigationContext);
        }



        protected virtual void CreatePatient(LoadingProcesser lp)
        {
            if (CardModel.CardType == CardType.社保卡)
            {
                //  DoCommand(lp =>
                // {
                lp.ChangeText("正在建档，请稍候...");
                CreateModel.Req病人建档发卡 = new req病人建档发卡
                {
                    operId = FrameworkConst.OperatorId,
                    cardNo = CardModel.CardNo,
                    cardType = "10",
                    name = IdCardModel.Name,
                    sex = IdCardModel.Sex.ToString(),
                    birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                    idNo = IdCardModel.IdCardNo,
                    idType = "1",
                    nation = IdCardModel.Nation,
                    address = IdCardModel.Address,
                    phone = Phone,
#pragma warning disable 612
                    guardianName = null,
                    school = null,
#pragma warning restore 612
                    //pwd = "123456",
                    tradeMode = "CA",
                    cash = null,
                    accountNo = null,
                    patientType = null,
                    setupType = ((int)CreateModel.CreateType).ToString()
                };
                CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);
                if (CreateModel.Res病人建档发卡.success)
                {
                    PatientModel.Req病人信息查询 = new req病人信息查询
                    {
                        cardNo = CardModel.CardNo,
                        cardType = ((int)CardModel.CardType).ToString()
                    };
                    PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
                    Next();
                }
                else
                {
                    ShowAlert(false, "建档发卡", "建档发卡失败：" + CreateModel.Res病人建档发卡.msg);
                }
                return;
                //  });
            }
            else
            {
                //                DoCommand(lp =>
                //                {
                lp.ChangeText("正在准备就诊卡，请稍候...");
                //todo 发卡机发卡
                if (!GetNewCardNo()) return;

                CreateModel.Req病人建档发卡 = new req病人建档发卡
                {
                    operId = FrameworkConst.OperatorId,
                    cardNo = CardModel.CardNo,
                    cardType = "2",
                    //name = IdCardModel.Name,
                    name = Name.Trim(),
                    sex = IdCardModel.Sex.ToString(),
                    birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                    idNo = IdCardModel.IdCardNo,
                    idType = "1", //测试必传
                    nation = IdCardModel.Nation,
                    address = IdCardModel.Address,
                    phone = Phone,
#pragma warning disable 612
                    guardianName = null,
                    school = null,
#pragma warning restore 612
                    pwd = "123456",
                    tradeMode = "CA",
                    cash = null,
                    accountNo = null,
                    patientType = null,
                    //bankCardNo = pos?.CardNo,
                    //bankTime = pos?.TransTime,
                    //bankDate = pos?.TransDate,
                    //posTransNo = pos?.Trace,
                    //bankTransNo = pos?.Ref,
                    //deviceInfo = pos?.TId,
                    //sellerAccountNo = pos?.MId,
                    //setupType = ChaKa.GrardId ? "2" : "1",
                    setupType = "1"
                };
                lp.ChangeText("正在建档，请稍候...");
                CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);
                if (CreateModel.Res病人建档发卡?.success ?? false)
                {
                    lp.ChangeText("正在发卡，请及时取卡。");
                    WriteCard();
                    PrintCard();
                    //PrintModel.SetPrintInfo(true, "建档发卡成功", $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分建档",
                    //  ConfigurationManager.GetValue("Printer:Receipt"), null);
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "建档发卡成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分建档",
                        TipImage = "提示_发卡"
                    });



                    PlaySound(SoundMapping.取走卡片);

                    ChangeNavigationContent($"{IdCardModel.Name}\r\n卡号{CardModel.CardNo}");
                    Next();
                }
                else
                {
                    ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
                }
                //                });
            }
        }
        public override void UpdateConfirm()
        {
            if (string.IsNullOrWhiteSpace(NewPhone))
            {
                ShowAlert(false, "温馨提示", "请输入手机号");
                return;
            }
            if (!NewPhone.IsHandset())
            {
                ShowAlert(false, "温馨提示", "请输入正确的手机号");
                return;
            }
            if (ChoiceModel.Business == Business.建档 ||
                CardModel.CardType == CardType.社保卡 &&
                CardModel.ExternalCardInfo == "建档")
            {
                Phone = NewPhone;
                CreateModel.Phone = NewPhone;
                ShowUpdatePhone = false;
                return;
            }
        }
        public override void Confirm()
        {
            if (Name.IsNullOrWhiteSpace())
            {
                ModifyNameCommand.Execute(null);
                return;
            }
            if (Phone.IsNullOrWhiteSpace())
            {
                ShowUpdatePhone = true;
                return;
            }
            DoCommand(lp =>
            {
                if (ChoiceModel.Business == Business.建档 ||
                    CardModel.CardType == CardType.社保卡 &&
                    CardModel.ExternalCardInfo == "建档")
                {
                    switch (CreateModel.CreateType)
                    {
                        case CreateType.成人:
                            CreatePatient(lp);
                            break;
                        case CreateType.儿童:
                            Navigate(A.CK.InfoEx);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    return;
                }
                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                ChangeNavigationContent($"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}")
                ;
                Next();
            });
        }
        public virtual void ModifyNameCmd()
        {
            if (CardModel.CardType == CardType.NoCard)
            {
                Name = "";
                ShowMask(true, new FullInputBoard()
                {
                    SelectWords = p => { Name = p; },
                    KeyAction = p => { StartTimer(); if (p == KeyType.CloseKey) ShowMask(false); }

                }, 0.1, pt => { ShowMask(false); });
            }
        }
        private void PrintCard()
        {
            if (ConfigQD.M1Local)
            {
                return;
            }
            var PrintText = new List<ZbrPrintTextItem>
            {
                new ZbrPrintTextItem()
                {
                    X = 160,
                    Y = 55,
                    Text = Name
                },
                new ZbrPrintTextItem()
                {
                    X = 550,
                    Y = 55,
                    FontSize = 11,
                    Text = CreateModel.Res病人建档发卡.data.patientCard
                }
            };

            var PrintCode = new List<ZbrPrintCodeItem>();

            _rfCardDispenser.PrintContent(PrintText, PrintCode);
        }
        protected override bool GetNewCardNo()
        {
            try
            {
                if (ConfigQD.M1Local)
                {
                    CardModel.CardNo = DateTimeCore.Now.ToString("MMddhh24mm");
                    return true;
                }

                if (!_rfCardDispenser.Connect().IsSuccess)
                {
                    ReportService.发卡器离线(null, ErrorSolution.发卡器离线);
                    ShowAlert(false, "建档发卡", "发卡机连接失败");
                    return false;
                }
                if (!_rfCardDispenser.Initialize().IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机初始化失败");
                    return false;
                }
                var result = _rfCardDispenser.EnterCard();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机读卡号失败");
                    return false;
                }
                CardModel.CardNo = BitConverter.ToUInt32(result.Value, 0).ToString();
                return true;
            }
            catch (Exception ex)
            {
                ShowAlert(false, "建档发卡", "发卡机读卡失败");
                Logger.Main.Error($"[建档发卡]{ex.Message + ex.StackTrace}");
                return false;
            }
        }
        protected void WriteCard()
        {
            M1DispenserRW.RfCardDispenser = _rfCardDispenser;
            var now = DateTimeCore.Now.ToString("yyyyMMdd");
            var eIdType = EnumM1IdType.身份证;

            try
            {
                Result result;
                result = M1DispenserRW.WriteCommChunk0(CreateModel.Res病人建档发卡.data.patientCard, FrameworkConst.HospitalAreaCode, EnumM1Valid.启用);
                if (!result.IsSuccess)
                {
                    Logger.Main.Error($"[写卡失败] 物理卡号：{CardModel.CardNo} 写入地址:4扇区0块");
                }
                result = M1DispenserRW.WriteCommChunk1(now, "99991231", now);
                if (!result.IsSuccess)
                {
                    Logger.Main.Error($"[写卡失败] 物理卡号：{CardModel.CardNo} 写入地址:4扇区1块");
                }
                result = M1DispenserRW.WriteCommChunk2(CreateModel.Res病人建档发卡.data.platformId);
                if (!result.IsSuccess)
                {
                    Logger.Main.Error($"[写卡失败] 物理卡号：{CardModel.CardNo} 写入地址:4扇区2块");
                }
                result = M1DispenserRW.WritePatientChunk0(Name, IdCardModel.Sex.ToString());
                if (!result.IsSuccess)
                {
                    Logger.Main.Error($"[写卡失败] 物理卡号：{CardModel.CardNo} 写入地址:5扇区0块");
                }
                result = M1DispenserRW.WritePatientChunk1(eIdType, IdCardModel.IdCardNo, Phone);
                if (!result.IsSuccess)
                {
                    Logger.Main.Error($"[写卡失败] 物理卡号：{CardModel.CardNo} 写入地址:5扇区1、2块");
                }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"[建档发卡]写卡失败：{ex.Message + ex.StackTrace}");
            }
        }

        #region Binding
        private int nameBorderThick;
        public int NameBorderThick
        {
            get { return nameBorderThick; }
            set
            {
                nameBorderThick = value;
                OnPropertyChanged();
            }
        }
        #endregion Binding
    }
}
