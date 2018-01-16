using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.UserCenter.Auth;
using YuanTu.Core.Extension;
using YuanTu.Default.House.HealthManager;
using YuanTu.Consts.UserCenter;
using YuanTu.Devices.CardReader;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;

namespace YuanTu.QingDao.House.Component.Auth
{
    public class LoginViewModel:Default.House.Component.Auth.ViewModels.LoginViewModel
    {
        [Dependency]
        public IAuthModel AuthModel { get; set; }
        public LoginViewModel(IIdCardReader[] idCardReaders, IMagCardReader[] magCardReaders, IRFCardReader[] rfCardReader) : base(idCardReaders, magCardReaders, rfCardReader)
        {
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            HideNavigating = true;
            _needReadIdCard = ChoiceModel.Business != Business.预约 && ChoiceModel.Business != Business.挂号;
            Tips = _needReadIdCard ? "请按提示刷区域诊疗卡或者身份证" : "请按提示刷区域诊疗卡";
            TipImage = ResourceEngine.GetImageResourceUri(_needReadIdCard ? "登录示例_House" : "登录示例2_House");
            StartRead();

            PlaySound(_needReadIdCard ? SoundMapping.请将身份证或区域诊疗卡放置于感应区 : SoundMapping.请将区域诊疗卡放置于感应区);
        }
        public override void OnGetInfo(string cardNo)
        {
            //查询健康后台用户信息
            DoCommand(p =>
            {
                p.ChangeText("正在查询您的健康档案，请稍候...");
                if (CardModel.CardType == CardType.身份证)
                {
                    var req = new req查询是否已建档
                    {
                        name = IdCardModel.Name,
                        idNo = IdCardModel.IdCardNo,
                        cardNo = null,
                        cardType = null,
                        sex = IdCardModel.Sex.ToString(),
                        age = IdCardModel.Birthday.Age().ToString(),
                        birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                        nation = IdCardModel.Nation,
                        addr = IdCardModel.Address,
                        expire = IdCardModel.ExpireDate.ToString("yyyy-MM-dd"),
                        photo = string.Empty
                    };
                    var res = HealthDataHandlerEx.查询是否已建档(req);
                    if (!(res?.success ?? false))
                    {
                        ShowAlert(false, "温馨提示", "健康档案信息查询失败", debugInfo: res?.msg);
                        Preview();
                        return;
                    }
                    HealthModel.Res查询是否已建档 = res;
                    Next();
                }
                else if (CardModel.CardType == CardType.就诊卡)
                {
                    p.ChangeText("正在查询您的就诊人信息，请稍候...");
                    var reqQueryPatient = new req查询就诊人
                    {
                        cardNo = cardNo,
                        //cardType = ((int)AuthModel.CardType).ToString(),
                        cardType ="2",
                        unionId = FrameworkConst.UnionId
                    };
                    var resQueryPatient = DataHandlerEx.查询就诊人(reqQueryPatient);

                    if (resQueryPatient.success)
                    {
                        if (resQueryPatient.data == null )
                        {
                            ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                            Preview();
                            return;
                        }
                        CardModel.CardNo = cardNo;
                        AuthModel.当前就诊人信息 = resQueryPatient.data;

                        if (ChoiceModel.Business == Business.健康服务 || ChoiceModel.Business == Business.体测查询)
                        {
                            p.ChangeText("正在查询您的健康档案，请稍候...");
                            var patientInfo = AuthModel.当前就诊人信息;
                            var req = new req查询是否已建档
                            {
                                name = patientInfo.patientName,
                                idNo = patientInfo.idNo,
                                cardNo = CardModel.CardNo,
                                cardType = ((int)CardModel.CardType).ToString(),
                                sex = patientInfo.sex == 1 ? "男" : "女",
                                age = null,
                                birthday = patientInfo.birthday,
                                nation = null,
                                addr = null,
                                expire = null,
                                photo = null,
                                phone = patientInfo.phoneNum
                            };

                            var resQueryUser = HealthDataHandlerEx.查询是否已建档(req);
                            if (!(resQueryUser?.success ?? false))
                            {
                                ShowAlert(false, "温馨提示", "健康档案信息查询失败", debugInfo: resQueryUser?.msg);
                                Preview();
                                return;
                            }
                            HealthModel.Res查询是否已建档 = resQueryUser;
                        }


                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: resQueryPatient.msg);
                        Preview();
                    }
                }
            });
        }
    }

    
}
