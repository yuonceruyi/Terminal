using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;

namespace YuanTu.NingXiaHospital
{
    public class NamedPipeCameraService : ICameraService
    {
        public string ServiceName => "NamedPipeCameraService";

        public Result<string[]> SnapShot(string reason)
        {
            return Result<string[]>.Success(new string[100]);
        }

        public Result<string[]> StartRecording(string reason)
        {
            return Result<string[]>.Success(new string[100]);
        }

        public Result StopRecording()
        {
            return Result.Success();
        }
    }
}