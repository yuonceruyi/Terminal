using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Default.House.Device
{
    public  interface IDeviceService:IService
    {
        Result StartMeasure();

        Result StopMeasure();
    }
}
