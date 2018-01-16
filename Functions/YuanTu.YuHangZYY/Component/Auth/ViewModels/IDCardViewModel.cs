using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Models;
using YuanTu.Core.Log;
using YuanTu.Devices.CardReader;
using YuanTu.YuHangZYY.Component.Auth.Models;

namespace YuanTu.YuHangZYY.Component.Auth.ViewModels
{
    public class IDCardViewModel : YuanTu.Default.Component.Auth.ViewModels.IDCardViewModel
    {
        public IDCardViewModel(IIdCardReader[] idCardReaders) : base(idCardReaders)
        {
        }


        protected override void OnGetInfo(string idCardNo)
        {
            if (CardModel.ExternalCardInfo == "社保_信息补全")
            {
                var ck = CardModel as CardModel;
                var siCardName = ck.Res读接触卡号.姓名.Trim();
                if (siCardName != IdCardModel.Name)
                {
                    ShowAlert(false, "信息不匹配", $"请持【{siCardName}】的身份证进行信息补全！", extend: new AlertExModel()
                    {
                        HideCallback = tp =>
                        {
                            if (tp == AlertHideType.ButtonClick)
                            {
                                StartRead();
                            }
                            else
                            {
                                Navigate(A.Home);
                            }
                        }
                    });
                    return;
                }
                Next();
            }
            else
            {
                base.OnGetInfo(idCardNo);

            }
        }

        protected override void FillIdCardModel(IdCardDetail detail)
        {
            if (CardModel.ExternalCardInfo != "社保_信息补全")
            {
                CardModel.CardNo = detail.IdCardNo;
            }
            IdCardModel.Name = detail.Name;
            IdCardModel.Sex = detail.Sex;
            IdCardModel.IdCardNo = detail.IdCardNo;
            IdCardModel.Address = detail.Address;
            IdCardModel.Birthday = detail.Birthday;
            IdCardModel.Nation = detail.Nation;
            IdCardModel.GrantDept = detail.GrantDept;
            IdCardModel.ExpireDate = detail.ExpireDate;
            IdCardModel.EffectiveDate = detail.EffectiveDate;
            Logger.Main.Info($"[读取身份证信息成功]{IdCardModel.Name} {IdCardModel.Sex} {IdCardModel.IdCardNo}");
        }
    }
}
