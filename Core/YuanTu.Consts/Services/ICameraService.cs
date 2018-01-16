using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Consts.Services
{
    public interface ICameraService : IService
    {
        Result<string[]> SnapShot(string reason);

        Result<string[]> StartRecording(string reason);

        Result StopRecording();
    }
}