﻿using YuanTu.Devices.CardReader;

namespace YuanTu.TongXiangZhouQuanHospital.Component.Auth.ViewModels
{
    public class CardViewModel : TongXiangHospitals.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders) : base(rfCardReaders, magCardReaders)
        {
        }
    }
}
