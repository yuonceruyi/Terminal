using System;
using Prism.Commands;
using Prism.Regions;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Devices.CardReader;
using YuanTu.ZheJiangZhongLiuHospital.ICBC;
using YuanTu.ZheJiangZhongLiuHospital.Minghua;
using YuanTu.Core.Gateway;
using YuanTu.Core.Log;
using Newtonsoft.Json;

namespace YuanTu.ZheJiangZhongLiuHospital.Component.Auth.ViewModels
{
    public class SiCardViewModel : YuanTu.Default.Component.Auth.ViewModels.SiCardViewModel
    {

        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        public override void OnSet()
        {
            GifUrl = ResourceEngine.GetImageResourceUri("社保卡动画");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            PlaySound(SoundMapping.请插入医保卡);
        }
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public IYiBaoCardContent YiBaoCardContent { get; set; }

        public override void Confirm()
        {
            DoCommand(lp =>
            {
                var reader = new MingHuaX3CardReader();
                var cardContent = reader.Read();
                if (cardContent && !string.IsNullOrEmpty(cardContent.Value?.CardNume ?? "") && !string.IsNullOrEmpty(cardContent.Value?.CardId ?? ""))
                {
                    if (cardContent.Value.CardId.StartsWith("3301"))
                    {
                        CardModel.CardNo = cardContent.Value.CardNume;
                    }
                    else
                    {
                        //CardModel.CardNo = $"{cardContent.Value.CardNume}{cardContent.Value.CardId.Substring(0, 6)}";
                        if (ChoiceModel.Business == Business.取号 || ChoiceModel.Business == Business.缴费 || ChoiceModel.Business == Business.挂号)
                        {
                            ShowAlert(false, "温馨提示", $"省医保卡和一卡通用户请到窗口处理");
                            return;
                        }
                        
                    }
                    Injection(cardContent.Value);
                    CardModel.CardType = CardType.社保卡;
                    PatientModel.Req病人信息查询 = new req病人信息查询
                    {
                        cardNo = CardModel.CardNo,
                        cardType = ((int)CardModel.CardType).ToString()
                    };
                    PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
                    if (PatientModel.Res病人信息查询.success)
                    {
                        //if (FrameworkConst.DoubleClick)
                        //{
                        //    YiBaoCardContent.Name = "hq";
                        //    YiBaoCardContent.Nation = "123";
                        //    YiBaoCardContent.PId = "234567890-";
                        //    YiBaoCardContent.Sex = "1";
                        //    PatientModel.Res病人信息查询.success = false;
                        //    Navigate(A.CK.Info);
                        //    return;
                        //}

                        if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                        {
                            ShowAlert(false, "温馨提示", $"未找到社保卡号为:{cardContent.Value?.CardNume}病人");
                            return;
                        }

                        //if (ChoiceModel.Business == Business.缴费 || ChoiceModel.Business == Business.签到)
                        //{
                            Navigate(A.CK.Info);
                            return;
                        //}
                        //var req = new Req查询虚拟账户余额()
                        //{
                        //    Chanel = "1",
                        //    AccountNo = PatientModel.当前病人信息.accountNo,
                        //    AccountId = $"{PatientModel.当前病人信息.name}^{PatientModel.当前病人信息.idNo}",

                        //    OperId = FrameworkConst.OperatorId,
                        //    DeviceInfo = FrameworkConst.OperatorId,
                        //    TradeSerial = DateTimeCore.Now.ToString("yyyyMMddHHmmssffff"),


                        //    Rsv1 = "",
                        //    Rsv2 = "",
                        //};

                        //var qResult = PConnection.Handle<Res查询虚拟账户余额>(req);
                        //if (qResult.IsSuccess)
                        //{
                        //    var res = qResult.Value;
                        //    PatientModel.当前病人信息.accBalance = res.Remain;
                        //    Navigate(A.CK.Info);
                        //    return; ;
                        //}
                    }
                    if (!DataHandler.UnKnowErrorCode.Contains(PatientModel.Res病人信息查询.code))
                    {
                        Navigate(A.CK.Info);
                        return;
                    }
                    ShowAlert(false, "温馨提示", $"病人信息查询失败:{PatientModel.Res病人信息查询.msg}");
                }
                ShowAlert(false, "温馨提示", "读卡失败,请按照提示重新插卡");
            });
        }

        private Uri _gifUrl;

        public Uri GifUrl
        {
            get { return _gifUrl; }
            set { _gifUrl = value; OnPropertyChanged(); }
        }

        public ICommand MediaEndedCommand
        {
            get
            {
                return new DelegateCommand<object>((sender) =>
                {
                    MediaElement media = (MediaElement)sender;
                    media.LoadedBehavior = MediaState.Manual;
                    media.Position = TimeSpan.FromMilliseconds(1);
                    media.Play();
                });
            }

        }

        public void Injection(YiBaoCardContent model)
        {
            YiBaoCardContent.Birthday = model.Birthday.Substring(4).Insert(4, "-").Insert(7, "-");
            YiBaoCardContent.Birthplace = null;
            YiBaoCardContent.CardData = model.CardData;
            YiBaoCardContent.CardId = model.CardId;
            YiBaoCardContent.CardNume = model.CardNume;
            YiBaoCardContent.CardType = model.CardType;
            YiBaoCardContent.CardValidData = model.CardValidData;
            YiBaoCardContent.Fkjg = model.Fkjg;
            YiBaoCardContent.GfVer = model.GfVer;
            YiBaoCardContent.Name = model.Name;
            YiBaoCardContent.Nation = model.Nation.Substring(4);
            YiBaoCardContent.PId = model.PId;
            YiBaoCardContent.Sex = model.Sex.Replace("\u0001","");
        }
    }
}