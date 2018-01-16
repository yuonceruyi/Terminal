using System;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Devices.MKeyBoard
{
    public interface IMKeyboard : IDevice
    {
        int MasterKeyId { get; set; } 
        int PinKeyId { get; set; }
        int MacKeyId { get; set; } 

        Result LoadWorkKey(string pin, string pinchk, string mac, string macchk);

        Result<string> CalcMac(string text, KMode kMode, MacMode macMode);

        Result<string> BeforeAddPin(string cardNo = "000000000000");

        Result SetKeyAction(Action<KeyText> keyAction);
    }
}