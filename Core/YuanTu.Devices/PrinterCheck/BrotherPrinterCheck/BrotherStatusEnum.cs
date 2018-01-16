using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.Devices.PrinterCheck.BrotherPrinterCheck
{
    public enum BrotherStatusEnum
    {
        准备 = 10001,
        睡眠 = 40000,
        墨粉不足 = 10006,
        正在打印 = 10023,

        暂停 = 10002,
        等待 = 10003,
        工作取消 = 10007,
        超出内存 = 30016,

        无墨 = 40010,

        请替换墨粉 = 40038,
        Mp异常 = 41000,
        手动送纸 = 41100,
        T1异常 = 41200,
        T2异常 = 41300,

        定影仪错误 = 50076,

        存储已满 = 60000,
        过热 = 60003,
        硒鼓异常 = 60005,
        纸张大小错误 = 60023,
        无法打印 = 60030,
        无法双面打印 = 60120,
        卡纸 = 65016,

        纸张大小的错误 = 70007,
        日志已满 = 70001,
        扫描异常 = 70400,

        前机盖未关闭 = 40021,
        后机盖未关闭 = 42104,
        打印机后机盖未关闭 = 60021,
        无纸 = 41280,
    }
}
