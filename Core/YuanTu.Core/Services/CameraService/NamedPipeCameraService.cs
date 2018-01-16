using CameraLib;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using Result = YuanTu.Consts.FrameworkBase.Result;

namespace YuanTu.Core.Services.CameraService
{
    public class NamedPipeCameraService : ICameraService
    {
        private readonly CameraClient _cameraClient = new CameraClient();
        public string ServiceName => "命名管道控制摄像头";

        public Result<string[]> SnapShot(string reason)
        {
            SureService();
            var res = _cameraClient.ColdSnapShot(BuildPath(reason, false));
            if (res.IsSuccess)
            {
                if (res.Message.IsNullOrWhiteSpace())
                    return Result<string[]>.Fail(res.Message);
                // todo DataHandlerEx.拍照录像上传()
                var patientInfo = ServiceLocator.Current.GetInstance<IPatientModel>()?.当前病人信息;
                var cardInfo = ServiceLocator.Current.GetInstance<ICardModel>();
                var nameArray = res.Message.ToJsonObject<string[]>();
                var root = AppDomain.CurrentDomain.BaseDirectory;
                Task.Factory.StartNew(() =>
                {
                    foreach (var name in nameArray)
                    {
                        var realType = _prvDic.ContainsKey(reason) ? _prvDic[reason] : -1;
                        Reporter.ReporterDataHandler.拍照录像上传(new req拍照录像上传
                        {
                            fileUrl = name.Replace(root, "").Replace('\\', '/'),
                            logType = realType.ToString(),
                            isVideo = "0",
                            idNo = patientInfo?.idNo.BackNotNullOrEmpty(patientInfo.guardianNo),
                            name = patientInfo?.name,
                            cardNo = cardInfo?.CardNo,
                            cardType = ((int)cardInfo?.CardType).ToString(),
                            macAddr = Systems.NetworkManager.MAC,
                            gmtCreate = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        });
                    }
                });
                return Result<string[]>.Success(res.Message.ToJsonObject<string[]>());
            }
            // TODO resultCode
            return Result<string[]>.Fail(res.Message);
        }

        public Result<string[]> StartRecording(string reason)
        {
            SureService();
            var res = _cameraClient.StartRecording(BuildPath(reason, true));
            if (res.IsSuccess)
            {
                if(res.Message.IsNullOrWhiteSpace())
                    return Result<string[]>.Fail(res.Message);
                // todo DataHandlerEx.拍照录像上传()
                var patientInfo = ServiceLocator.Current.GetInstance<IPatientModel>()?.当前病人信息;
                var cardInfo = ServiceLocator.Current.GetInstance<ICardModel>();
                var nameArray = res.Message.ToJsonObject<string[]>();
                var root = AppDomain.CurrentDomain.BaseDirectory;
                Task.Factory.StartNew(() =>
                {
                    foreach (var name in nameArray)
                    {
                        var realType = _prvDic.ContainsKey(reason) ? _prvDic[reason] : -1;
                        Reporter.ReporterDataHandler.拍照录像上传(new req拍照录像上传
                        {
                            fileUrl = name.Replace(root, "").Replace('\\', '/'),
                            logType = realType.ToString(),
                            isVideo = "1",
                            idNo = patientInfo?.idNo.BackNotNullOrEmpty(patientInfo.guardianNo),
                            name = patientInfo?.name,
                            cardNo = cardInfo?.CardNo,
                            cardType = ((int)cardInfo?.CardType).ToString(),
                            macAddr = Systems.NetworkManager.MAC,
                            gmtCreate = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        });
                    }
                });
                return Result<string[]>.Success(res.Message.ToJsonObject<string[]>());
            }
               
            // TODO resultCode
            return Result<string[]>.Fail(res.Message);
        }

        public Result StopRecording()
        {
            var res = _cameraClient.StopRecording();
            if (res.IsSuccess)
                return Result.Success();
            // TODO resultCode
            return Result.Fail(res.Message);
        }

        protected virtual void SureService()
        {
            if (Debugger.IsAttached)
            {
                return;
            }
            //AppDomain.CurrentDomain.BaseDirectory
            var exeName = "CameraService";
            var cameraPath = @"External\CameraService\CameraService.exe";
            if (File.Exists(cameraPath) && !Process.GetProcesses().Any(p => p.ProcessName.Equals(exeName, StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        WorkingDirectory = Path.GetDirectoryName(cameraPath),
                        FileName = Path.GetFileName(cameraPath),
                        WindowStyle = ProcessWindowStyle.Hidden
                    });
                }
                catch (Exception)
                {
                }
            }
        }

        protected virtual string BuildPath(string reason, bool isVideo = false)
        {
            var basedir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cameras", DateTimeCore.Now.ToString("yyyyMMdd"), isVideo ? "Videos" : "Photos");
            if (!Directory.Exists(basedir))
            {
                Directory.CreateDirectory(basedir);
            }
            return Path.Combine(basedir, $"{reason}_{DateTimeCore.Now.ToString("yyyyMMdd_HHmmss")}");
        }

        private static Dictionary<string, int> _prvDic = new Dictionary<string, int>
        {
            ["主界面 自助发卡"] = 0,
            ["主界面 发病历本"] = 0,
            ["主界面 预缴金充值"] = 1,
            ["主界面 预约挂号"] = 2,
            ["主界面 现场挂号"] = 2,
            ["主界面 预约挂号"] = 3,
            ["主界面 预约取号"] = 4,
            ["主界面 结算"] = 5,
            ["主界面 住院预缴金充值"] = 104,
            // ["主界面 预缴金充值"] =104,
        };
    }
}