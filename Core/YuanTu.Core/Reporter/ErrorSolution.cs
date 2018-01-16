using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.Core.Reporter
{
    public static   class ErrorSolution
    {
        public const string 软件自动更新异常 = "联系自助机开发";
        public const string 软件初始化异常 = "现场实施检查配置文件是否正确或联系自助机开发";
        public const string 读卡器离线 = "现场实施检查端口配置正确并且连接正常";
        public const string 钱箱离线 = "现场实施检查端口配置正确并且连接正常";
        public const string 凭条打印机离线 = "现场实施检查设备管理器能正常识别到打印机并且打印机名称配置正确";
        public const string 发卡器离线 = "现场实施检查端口配置正确并且连接正常";
        public const string 卡已耗尽 = "相关工作人员及时加卡";
        public const string 卡剩余5张 = "相关工作人员及时加卡";
        public const string 卡剩余10张 = "相关工作人员及时加卡";
        public const string 卡剩余20张 = "相关工作人员及时加卡";
        public const string 身份证读卡器离线 = "现场实施用测试程序确认读卡器连接正常";
    }
}
