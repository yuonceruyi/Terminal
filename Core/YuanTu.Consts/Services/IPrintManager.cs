using System.Collections.Generic;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;

namespace YuanTu.Consts.Services
{
    public interface IPrintManager : IService
    {
        Result Print();
        Result QuickPrint(string printName,Queue<IPrintable> printables);

        Queue<IPrintable> NewQueue(string title);
        IPrintable[] AdvertisePrintables();
        
    }
}