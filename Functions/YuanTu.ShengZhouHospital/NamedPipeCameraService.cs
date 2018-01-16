using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;

namespace YuanTu.ShengZhouHospital
{
    public class NamedPipeCameraService : ICameraService
    {
        public string ServiceName
        {
            get
            {
                return "NamedPipeCameraService";
            }
        }

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
