﻿using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Default.Component.Auth.ViewModels;

namespace YuanTu.YiWuBeiYuan.Component.Auth.ViewModels
{
    public class ChoiceViewModel: YuanTu.Default.Component.Auth.ViewModels.ChoiceViewModel
    {
        protected override void Confirm(Info i)
        {
            var button = (CardTypeModel)i.Tag;
            CardModel.CardType = button.CardType;
            switch (button.CardType)
            {
                case CardType.就诊卡:
                case CardType.社保卡:
                    Navigate(A.CK.Card);
                    break;
                default:
                    base.Confirm(i);
                    break;
            }
        }
    }
}
