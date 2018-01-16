using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.Core.Reporter
{
    public class ExceptionModel
    {
        public ErrorCode ErrorCode { get; set; }
        public string ErrorDetail { get; set; }
        public string ErrorSolution { get; set; }

        public ExceptionModel(ErrorCode code, string detail, string solution)
        {
            ErrorCode = code;
            ErrorDetail = detail;
            ErrorSolution = solution;
        }
    }
    public enum ErrorCode
    {
        //1.常规异常(Common)
        [Description("Common_1000001"), ExceptionLevel(Level = 10)]
        内存耗尽,
        [Description("Common_1000002"), ExceptionLevel(Level = 10)]
        软件自动更新异常,
        [Description("Common_1000003"), ExceptionLevel(Level = 10)]
        软件初始化异常,
        [Description("Common_1000004"), ExceptionLevel(Level = 10)]
        软件系统异常,

        //2.读卡器异常(CardReader) 
        [Description("CardReader_2000001"), ExceptionLevel(Level = 10)]
        读卡器离线,
        [Description("CardReader_2000002"), ExceptionLevel(Level = 10)]
        读卡器被占用,

        //3.钱箱异常(MoneyBox)        
        [Description("MoneyBox_3000001"), ExceptionLevel(Level = 10)]
        钱箱离线,
        [Description("MoneyBox_3000002"), ExceptionLevel(Level = 10)]
        钱箱卡币,
        [Description("MoneyBox_3000003"), ExceptionLevel(Level = 10)]
        钱箱已满,

        //4.凭条打印机(RecepitPrinter)
        [Description("RecepitPrinter_4000001"), ExceptionLevel(Level = 10)]
        凭条打印机离线,
        [Description("RecepitPrinter_4000002"), ExceptionLevel(Level = 10)]
        凭条打印机纸将尽,
        [Description("RecepitPrinter_4000003"),ExceptionLevel(Level =10)]
        凭条打印机纸尽,
        [Description("RecepitPrinter_4000004"),ExceptionLevel(Level =10)]
        凭条打印机其他异常,
        [Description("RecepitPrinter_4000005"),ExceptionLevel(Level =10)]
        凭条打印机出纸口有纸,
        [Description("RecepitPrinter_4000006"),ExceptionLevel(Level =10)]
        凭条打印机卡纸,
        [Description("RecepitPrinter_4000007"), ExceptionLevel(Level = 10)]
        凭条打印机胶辊开启,

        //5.发卡器异常(CardDispenser)
        [Description("CardDispenser_5000001"), ExceptionLevel(Level = 10)]
        发卡器离线,
        [Description("CardDispenser_5000002"), ExceptionLevel(Level = 10)]
        发卡器被占用,
        [Description("CardDispenser_5100001"), ExceptionLevel(Level = 10)]
        卡已耗尽,
        [Description("CardDispenser_5100002"), ExceptionLevel(Level = 9)]
        卡剩余5张,
        [Description("CardDispenser_5100003"), ExceptionLevel(Level = 8)]
        卡剩余10张,
        [Description("CardDispenser_5100004"), ExceptionLevel(Level = 7)]
        卡剩余20张,

        //6.身份证读卡器异常(IDCardReader)
        [Description("IDCardReader_6000001"), ExceptionLevel(Level = 10)]
        身份证读卡器离线,

        //7.金属键盘异常(MetalKeyboard)
        [Description("MetalKeyboard_7000001"), ExceptionLevel(Level = 10)]
        金属键盘离线,
        [Description("MetalKeyboard_7000002"), ExceptionLevel(Level = 10)]
        金属键盘被占用,

        //8.社保读卡器异常(InsuranceReader)
        [Description("InsuranceReader_8000001"), ExceptionLevel(Level = 10)]
        社保读卡器离线,
        [Description("InsuranceReader_8000002"), ExceptionLevel(Level = 10)]
        社保读卡器被占用,

        //9.摄像头异常(Camera)
        [Description("Camera_9000001"), ExceptionLevel(Level = 10)]
        未发现摄像头,

        //10.业务异常(Business)
        [Description("Business_A000001"), ExceptionLevel(Level = 10)]
        网关请求超时,
        [Description("Business_A000002"), ExceptionLevel(Level = 10)]
        HIS请求超时,
        [Description("Business_A000003"), ExceptionLevel(Level = 10)]
        第三方请求超时,
        [Description("Business_A000004"), ExceptionLevel(Level = 10)]
        银联请求超时,
        [Description("Business_A000005"), ExceptionLevel(Level = 10)]
        POS签到失败,
        [Description("Business_A000006"), ExceptionLevel(Level = 10)]
        POS冲正失败,
        [Description("Business_A000007"),ExceptionLevel(Level = 10)]
        网关返回异常,
        [Description("Business_A000008"), ExceptionLevel(Level = 10)]
        社保返回异常,
        [Description("Business_A000009"), ExceptionLevel(Level = 10)]
        HIS请求失败,

        //11.健康小屋硬件
        [Description("House_HeightWeight_B000001"), ExceptionLevel(Level = 10)]
        健康小屋闸门离线,
        [Description("House_HeightWeight_B001001"), ExceptionLevel(Level = 10)]
        健康小屋身高体重仪离线,
        [Description("House_HeightWeight_B002001"), ExceptionLevel(Level = 10)]
        健康小屋体脂仪离线,
        [Description("House_HeightWeight_B003001"), ExceptionLevel(Level = 10)]
        健康小屋血压仪离线,
        [Description("House_HeightWeight_B004001"), ExceptionLevel(Level = 10)]
        健康小屋血氧仪离线,
        [Description("House_HeightWeight_B005001"), ExceptionLevel(Level = 10)]
        健康小屋体温仪离线,
        [Description("House_HeightWeight_B006001"), ExceptionLevel(Level = 10)]
        健康小屋心电仪离线,
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ExceptionLevelAttribute : Attribute
    {
        public int Level { get; set; } = 10;

    }
}
