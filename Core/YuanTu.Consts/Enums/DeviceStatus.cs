using System;

namespace YuanTu.Consts.Enums
{
    /// <summary>
    /// 设备状态
    /// </summary>
    [Flags]
    public enum DeviceStatus
    {
        /// <summary>
        /// 未初始化
        /// </summary>
        UnInitialized = 1,
        /// <summary>
        /// 未连接
        /// </summary>
        Disconnect = 2,
        /// <summary>
        /// 已连接
        /// </summary>
        Connected = 4,
        /// <summary>
        /// 空闲
        /// </summary>
        Idle = 8,
        /// <summary>
        /// 正在工作
        /// </summary>
        Busy = 16,
        /// <summary>
        /// 异常
        /// </summary>
        Error = 32,
    }
}