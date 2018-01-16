using YuanTu.Consts.Enums;

namespace YuanTu.Consts.FrameworkBase
{
    /// <summary>
    /// 硬件最终抽象接口
    /// </summary>
    public interface IDevice:IDependency
    {
        /// <summary>
        /// 硬件名称
        /// </summary>
        string DeviceName { get; }
        /// <summary>
        /// 硬件唯一标识(整个系统中唯一)
        /// </summary>
        string DeviceId { get; }
       
        /// <summary>
        /// 获取设备实时状态
        /// </summary>
        /// <returns></returns>
        Result<DeviceStatus> GetDeviceStatus();
        /// <summary>
        /// 1.连接设备
        /// </summary>
        /// <returns></returns>
        Result Connect();
        /// <summary>
        /// 2.初始化
        /// </summary>
        /// <returns></returns>
        Result Initialize();
        /// <summary>
        /// 3.反初始化
        /// </summary>
        /// <returns></returns>
        Result UnInitialize();
        /// <summary>
        /// 4.断开连接
        /// </summary>
        /// <returns></returns>
        Result DisConnect();

    }
}
