using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Models;
using YuanTu.Core.Reporter;
using YuanTu.Default.Component.Auth.Dialog.Views;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.FuYangRMYY.Component.Auth.Models;

namespace YuanTu.FuYangRMYY.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : YuanTu.Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser) : base(rfCardDispenser)
        {
            
        }

        public override void OnEntered(NavigationContext navigationContext)
        {

            ChangeNavigationContent("");
            TopBottom.InfoItems = null;

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
            }
            else if (ChoiceModel.Business == Business.建档 && CardModel.CardType == CardType.社保卡)
            {
                //todo 社保卡建档处理
            }
            else if (CardModel.CardType == CardType.社保卡&& CardModel.ExternalCardInfo == "建档")//社保卡补全信息
            {
               // var patient = PatientModel.Res病人信息查询?.data?.FirstOrDefault();
                Name = IdCardModel.Name;
                Sex = IdCardModel.Sex.ToString();
                Birth = IdCardModel.Birthday.ToString("yyyy-MM-dd");
                Phone = null;
                IdNo = IdCardModel.IdCardNo.Mask(14, 4);
                IsAuth = false;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;
            }
            else
            {
                IsAuth = true;
                ShowUpdatePhone = false;
                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                Name = patientInfo.name;
                Sex = patientInfo.sex;
                Birth = patientInfo.birthday.SafeToSplit(' ', 1)[0];

                CanUpdatePhone = false;
                Phone = patientInfo.phone.Mask(patientInfo.phone.Length-4, 4);
                IdNo = patientInfo.idNo.Mask(14, 4);
            }

        }

        public override void Confirm()
        {
            if (Phone.IsNullOrWhiteSpace())
            {
                ShowUpdatePhone = true;
                return;
            }
            if (CardModel.CardType == CardType.社保卡 && CardModel.ExternalCardInfo == "建档")
            {
                CreatePatientBySiCard();
                return;
            }
            
            if (ChoiceModel.Business == Business.建档)
            {
                switch (CreateModel.CreateType)
                {
                    case CreateType.成人:
                        CreatePatient();
                        break;

                    case CreateType.儿童:
                        Navigate(A.CK.InfoEx);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return;
            }
            var patientInfo = PatientModel.当前病人信息;
            if (CardModel.CardType==CardType.就诊卡)
            {

                ChangeNavigationContent($"{patientInfo.name}\r\n卡内余额:{patientInfo.accBalance.In元()}");
            }
            else
            {
                var cm = CardModel as CardModel;
                ChangeNavigationContent($"{patientInfo.name}\r\n卡内余额:{patientInfo.accBalance.In元()}\r\n医保余额:{(cm.SiCardInfo?.社保读卡?.账户余额)??"未知"}元");
            }

            var resource = ResourceEngine;
            TopBottom.InfoItems = new ObservableCollection<InfoItem>(new[]
            {
                new InfoItem
                {
                    Title = "姓名",
                    Value = patientInfo.name,
                    Icon = resource.GetImageResourceUri("姓名图标_YellowBlue")
                },
                new InfoItem
                {
                    Title = "余额",
                    Value = patientInfo.accBalance.In元(),
                    Icon = resource.GetImageResourceUri("余额图标_YellowBlue")
                }
            });

            Next();
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
            if (ChoiceModel.Business == Business.建档|| (CardModel.CardType == CardType.社保卡 && CardModel.ExternalCardInfo == "建档"))
            {
                Phone = NewPhone;
                CreateModel.Phone = NewPhone;
                ShowUpdatePhone = false;
                return;
            }
            base.UpdateConfirm();
        }

        protected  void CreatePatientBySiCard()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在系统中建立档案，请稍后...");

                CreateModel.Req病人建档发卡 = new req病人建档发卡
                {
                    cardNo = CardModel.CardNo,
                    cardType = ((int)(CardModel.CardType == CardType.社保卡 ? CardType.社保卡 : CardType.就诊卡)).ToString(),
                    name = IdCardModel.Name,
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
                    cash = "0",
                    accountNo = null,
                    patientType = "医保",//社保卡建档
                    setupType = ((int)CreateModel.CreateType).ToString(),
                };
                CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);
                if (CreateModel.Res病人建档发卡?.success ?? false)
                {
                    lp.ChangeText("获取最新信息，请稍后...");
                    PatientModel.Req病人信息查询 = new req病人信息查询
                    {
                        cardNo = CardModel.CardNo,
                        cardType = ((int)CardModel.CardType).ToString()
                    };
                    PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

                    ChangeNavigationContent($"{IdCardModel.Name}\r\n卡号{CardModel.CardNo}");
                    var sm = GetInstance<IShellViewModel>();
                    sm.Busy.IsBusy = false;
                    Next();
                }
                else
                {
                    ShowAlert(false, "社保卡建档", "系统建档失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
                }
            });
        }
    }
}