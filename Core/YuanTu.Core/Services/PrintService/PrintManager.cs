using System;
using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Advertisement;
using YuanTu.Core.Advertisement.Data;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using static YuanTu.Core.Services.PrintService.BarCode;

namespace YuanTu.Core.Services.PrintService
{
    public class PrintManager : IPrintManager
    {
        [Dependency]
        public IPrintModel PrintModel { get; set; }

        public string ServiceName => "控制打印";

        public virtual Result Print()
        {
            var ad = AdvertisePrintables();
            if (PrintModel.PrintInfo.PrintablesList != null)
            {
                foreach (var result in PrintModel.PrintInfo.PrintablesList.
                    Select(printables =>
                    {
                        var cp = new Queue<IPrintable>(printables);
                        if (ad?.Any() ?? false)
                        {
                            foreach (var printable in ad)
                            {
                                cp.Enqueue(printable);
                            }
                        }
                        return
                            new PrintHelper(PrintModel.PrintInfo.PrinterName, cp).Print();
                    }).
                    Where(result => !result.IsSuccess))
                {
                    return result;
                }
            }
            if (PrintModel.PrintInfo.Printables != null)
            {
                var cp = new Queue<IPrintable>(PrintModel.PrintInfo.Printables);
                if (ad?.Any() ?? false)
                {
                    foreach (var printable in ad)
                    {
                        cp.Enqueue(printable);
                    }
                }
                return new PrintHelper(PrintModel.PrintInfo.PrinterName, cp).Print();

            }
            return Result.Success();
        }

        public virtual Result QuickPrint(string printerName, Queue<IPrintable> printables)
        {
            var ad = AdvertisePrintables();
            var cp = new Queue<IPrintable>(printables);
            if (ad?.Any() ?? false)
            {
                foreach (var printable in ad)
                {
                    cp.Enqueue(printable);
                }
            }
            return new PrintHelper(printerName, cp).Print();
        }

        public virtual Queue<IPrintable> NewQueue(string title)
        {
            var queue = new Queue<IPrintable>();
            queue.Enqueue(new PrintItemText
            {
                Text = FrameworkConst.HospitalName /*医院名称*/,
                StringFormat = PrintConfig.Center,
                Font = PrintConfig.HeaderFont
            });
            if (!string.IsNullOrWhiteSpace(title))
            {
                queue.Enqueue(new PrintItemGap {Gap = 5f});
                queue.Enqueue(new PrintItemText
                {Text = title + "凭条", StringFormat = PrintConfig.Center, Font = PrintConfig.HeaderFont2});
            }
            queue.Enqueue(new PrintItemGap {Gap = 10f});
            return queue;
        }

        public virtual IPrintable[] AdvertisePrintables()
        {
            try
            {
                var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
                var enabled = config.GetValueInt("Advertisement:TicketEnable") == 1;
                if (enabled)
                {
                    var url = config.GetValue("Advertisement:Url");
                    AdvertisementHandler.AdServerUrl = url;
                    //string idNo,string cardNo,int age,Sex sex
                    var cardInfo = ServiceLocator.Current.GetInstance<ICardModel>();
                    var patientInfo = ServiceLocator.Current.GetInstance<IPatientModel>();

                    var cardNo = cardInfo?.CardNo;
                    var idNo = patientInfo.当前病人信息?.idNo ?? patientInfo.住院患者信息?.idNo;
                    var sex = (patientInfo.当前病人信息?.sex ?? patientInfo.住院患者信息?.sex).SafeToSex();
                    var age = 0;
                    if (idNo?.Length == 18)
                    {
                        var birth = DateTime.ParseExact(idNo.Substring(6, 8), "yyyyMMdd", null);
                        age = (DateTimeCore.Now.Year - birth.Year) +
                              ((DateTimeCore.Now.Month - birth.Month) > 0 ? 1 : 0);
                    }
                    var ret = AdvertisementHandler.获取广告(new GetAdvertisementReq()
                    {
                        idNo = idNo,
                        cardNo = cardNo,
                        age = age,
                        sex = sex
                    });
                    if (ret?.success ?? false)
                    {
                        var bts = Convert.FromBase64String(ret.data.picStr);
                        var img = Image.FromStream(new MemoryStream(bts));
                        return new IPrintable[]
                        {
                            new PrintItemImage
                            {
                                Align = ImageAlign.Center,
                                Image = img,
                                //Height = qrCode.Height / 1.5f,
                                //Width = qrCode.Width / 1.5f
                                Height = img.Height/1.5f,
                                Width = img.Width/1.5f
                            }
                        };

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Printer.Error($"[{ServiceName}]获取广告时发生异常,{ex.Message} {ex.StackTrace}");
            }

            return null;
        }


    }
}