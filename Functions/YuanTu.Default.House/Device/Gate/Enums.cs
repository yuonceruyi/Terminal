using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.Default.House.Device.Gate
{
    public enum Enums
    {
        开闸门成功=1,
        开闸门失败=2,
        开闸门接收命令错误=4,
        关闸门成功=8,
        关闸门失败 = 16,
        关闭时遇到障碍物后开启闸门成功=32,
        关闭时遇到障碍物后开启闸门失败=64,
        关闸门接收命令错误=128
    }

    public enum ServiceStatus
    {
        Disconnected,
        Openning,
        OpenFailed,
        Opened,
        Closing,
        CloseFailed,
        Closed,
    }
}
