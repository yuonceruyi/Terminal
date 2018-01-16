using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using System.Threading;

namespace YuanTu.Devices.FingerPrint
{
    public interface IFingerPrintDevice : IDevice
    {
        Result<Bitmap> GetFingerImage();
        Result<byte[]> GetFingerTemplate();
    }

    public class ZKFingerPrintDevice : IFingerPrintDevice
    {
        private readonly byte[] _imageBuffer = new byte[640 * 480];
        private DeviceStatus _currentStatus = DeviceStatus.UnInitialized;

        private IntPtr _handle;
        private bool _isConnectd;
        public string DeviceName => "ZK";
        public string DeviceId => DeviceName + "_FP";

        public Result<DeviceStatus> GetDeviceStatus()
        {
            return Result<DeviceStatus>.Success(_currentStatus);
        }

        public Result Connect()
        {
            Logger.Main.Info($"[准备链接指纹仪] 当前状态=" + _isConnectd.ToString());
            if (_isConnectd)
                return Result.Success();
            _handle = ZKFPModule_Connect("protocol=USB,vendor-id=6997,product-id=289");
            if (_handle == IntPtr.Zero)
            {
                Logger.Device.Error($"[指纹识别器{DeviceId}]连接异常 ");
                return Result.Fail("指纹识别器连接失败");
            }
            _isConnectd = true;
            _currentStatus = DeviceStatus.Connected;
            Logger.Main.Info($"[连接指纹仪] 成功");
            return Result.Success();
        }

        public Result Initialize()
        {
            //GetFingerImage();
            return Result.Success();
        }

        public Result UnInitialize()
        {
            return Result.Success();
        }

        public Result DisConnect()
        {
            Logger.Main.Info($"[准备断开指纹仪] 当前状态=" + _isConnectd.ToString());
            _isConnectd = false;
            Thread.Sleep(200);
            var x = ZKFPModule_Disconnect(_handle);
            Logger.Main.Info($"[断开指纹仪] 结果=" + x.ToString());
            _handle = IntPtr.Zero;
            _currentStatus = DeviceStatus.Disconnect;
            return Result.Success();
        }

        public Result<Bitmap> GetFingerImage()
        {
            var width = 0;
            var height = 0;
            var size = 640 * 480;
            lock (_imageBuffer)
            {
                var ret = ZKFPModule_GetFingerImage(_handle, ref width, ref height, _imageBuffer, ref size);
                if (ret != 0)
                    return Result<Bitmap>.Fail("");

                var ms = new MemoryStream();
                BitmapFormat.GetBitmap(_imageBuffer, width, height, ms);
                return Result<Bitmap>.Success(new Bitmap(ms));
            }
        }

        public Result<byte[]> GetFingerTemplate()
        {
            byte[] imageTmp = new byte[2048];
            lock (imageTmp)
            {
                var size = 2028;
                var ret = -99;
                var times = 0;
                while (ret != 0 && times < 500)
                {
                    times++;
                    if (_isConnectd)
                    {
                        ret = ZKFPModule_ScanTemplate(_handle, imageTmp, ref size);
                    }
                    else
                    {
                        break;
                    }
                    Thread.Sleep(100);
                }
                Logger.Main.Info($"[读取指纹特征] 次数=" + times.ToString() + "结果=" + ret.ToString());
                if (ret != 0)
                    return Result<byte[]>.Fail("");

                return Result<byte[]>.Success(imageTmp);
            }
        }

        #region DllImport

        const string DllPath = "External\\ZKFP\\ZKFPModule.dll";

        [DllImport(DllPath)]
        private static extern IntPtr ZKFPModule_Connect(string lpParams);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_Disconnect(IntPtr handle);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_EnrollUserByScan(IntPtr handle, int nUserId);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_GetFingerImage(IntPtr handle, ref int width, ref int heigth,
            byte[] imgData, ref int dataSize);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_ClearDB(IntPtr handle);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_DisableDevice(IntPtr handle);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_EnableDevice(IntPtr handle);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_FreeScan(IntPtr handle, ref int userId, ref int index);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_EnrollTemplateByImage(IntPtr handle, int userId, byte[] data,
            int dataSize);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_IdentifyByImage(IntPtr handle, byte[] imgData, int dataSize,
            ref int userId, ref int index);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_SetTime(IntPtr handle, int year, int month, int day, int hour, int minute,
            int second);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_GetTime(IntPtr handle, ref int year, ref int month, ref int day,
            ref int hour, ref int minute, ref int second);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_ScanTemplate(IntPtr handle, byte[] mSzTemplate, ref int nLength);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_ReadAllLogs(IntPtr handle, byte[] logData, ref int dataSize);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_GetStatus(IntPtr handle);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_GetParameter(IntPtr handle, int flag, ref int value);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_SetParameter(IntPtr handle, int flag, int value);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_SaveParameter(IntPtr handle);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_DeleteAllUsers(IntPtr handle);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_DeleteUser(IntPtr handle, int userId);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_ReadAllUser(IntPtr handle, byte[] userData, ref int dataSize);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_GetUser(IntPtr handle, int userId, StringBuilder name, string password,
            ref ushort secLevel, ref uint pin2,
            byte[] privilege, byte[] figerprintNum, byte[] card);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_ModifyUser(IntPtr handle, int userId, string name, string password,
            ushort secLevel, uint pin2, byte privilege,
            byte figerprintNum, string card);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_SetTemplates(IntPtr handle, int userId, int flag, byte[] data,
            int dataSize);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_DeleteTemplates(IntPtr handle, int userId, int index, int flag);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_DeleteAllTemplates(IntPtr handle);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_ReadTemplates(IntPtr handle, int userId, int index, int flag, byte[] data,
            ref int dataSize);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_ReadAllTemplates(IntPtr handle, byte[] data, ref int dataSize);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_Verify(IntPtr handle, int userId);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_DeleteAllLogs(IntPtr handle);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_Reset(IntPtr handle);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_Upgrade(IntPtr handle, byte[] fw, int size);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_UploadTemplatesFileData(IntPtr handle, byte[] tmpData, int size);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_UploadUserFileData(IntPtr handle, byte[] userData, int size);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_DownloadUserFileData(IntPtr handle, byte[] userData, ref int size);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_DownloadTemplatesFileData(IntPtr handle, byte[] tmpData, ref int size);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_TimeAnalyse(uint date, uint time,
            ref int year, ref int month, ref int day,
            ref int hour, ref int minute, ref int second);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_BufferToProtocol(int inDataType, byte[] data, int size, byte[] descData,
            ref int descDataSize);

        [DllImport(DllPath)]
        private static extern int ZKFPModule_ProtocolToBuffer(int inDataType, byte[] descData, byte[] data,
            ref int size);

        #endregion DllImport
    }

    public class WeFingerPrintDevice : IFingerPrintDevice
    {

        public string DeviceName => "WE";

        public string DeviceId => DeviceName + "_FP";

        public Result Connect()
        {
            var ret = WEDll.FPIDevDetect();
            Logger.Device.Info($"[维尔指纹]检测设备返回码:{ret}");
            return ret == 0 ? Result.Success() : Result.Fail("打开设备失败");
        }

        public Result DisConnect()
        {
            Logger.Device.Info($"[维尔指纹] 关闭");
            return Result.Success();
        }

        public Result<DeviceStatus> GetDeviceStatus()
        {
            return Result<DeviceStatus>.Success(DeviceStatus.Connected);
        }

        public Result<Bitmap> GetFingerImage()
        {
            Logger.Device.Info($"[维尔指纹] 获取指纹图片");
            int w = 0;
            int h = 0;
            byte[] data = new byte[80000];
            if (WEDll.FPIGetImageData(0, ref w, ref h, data) == 0)
            {
                var ms = new MemoryStream();
                BitmapFormat.GetBitmap(data, 152, 200, ms);
                Logger.Device.Info($"[维尔指纹] 获取指纹图片成功");
                return Result<Bitmap>.Success(new Bitmap(ms));
            }
            Logger.Device.Info($"[维尔指纹] 获取指纹图片失败");
            return Result<Bitmap>.Fail("获取图片失败");
        }

        public Result<byte[]> GetFingerTemplate()
        {
            return Result<byte[]>.Fail("获取特征失败");
        }

        public Result Initialize()
        {
            return Result.Success();
        }

        public Result UnInitialize()
        {
            return Result.Success();
        }

        #region DLL

        const string DllPath = "External\\WEFP\\libFPLDevWL.dll";

        internal class WEDll
        {
            [DllImport(DllPath)]
            public static extern int FPIGetImageData(int nPort, ref int piImageWidth, ref int piImageHeight, byte[] psImageData);

            [DllImport(DllPath)]
            public static extern int FPIDevDetect();

        }

        #endregion

    }

    public class BitmapFormat
    {
        /*******************************************
        * 函数名称：RotatePic
        * 函数功能：旋转图片，目的是保存和显示的图片与按的指纹方向不同
        * 函数入参：BmpBuf---旋转前的指纹字符串
        * 函数出参：ResBuf---旋转后的指纹字符串
        * 函数返回：无
        *********************************************/

        public static void RotatePic(byte[] bmpBuf, int width, int height, ref byte[] resBuf)
        {
            var bmpBuflen = width * height;

            try
            {
                for (var rowLoop = 0; rowLoop < bmpBuflen;)
                {
                    for (var colLoop = 0; colLoop < width; colLoop++)
                        resBuf[rowLoop + colLoop] = bmpBuf[bmpBuflen - rowLoop - width + colLoop];

                    rowLoop = rowLoop + width;
                }
            }
            catch //(Exception ex)
            {
                //ZKCE.SysException.ZKCELogger logger = new ZKCE.SysException.ZKCELogger(ex);
                //logger.Append();
            }
        }

        /*******************************************
        * 函数名称：StructToBytes
        * 函数功能：将结构体转化成无符号字符串数组
        * 函数入参：StructObj---被转化的结构体
        *           Size---被转化的结构体的大小
        * 函数出参：无
        * 函数返回：结构体转化后的数组
        *********************************************/

        public static byte[] StructToBytes(object structObj, int size)
        {
            var structSize = Marshal.SizeOf(structObj);
            var getBytes = new byte[structSize];

            try
            {
                var structPtr = Marshal.AllocHGlobal(structSize);
                Marshal.StructureToPtr(structObj, structPtr, false);
                Marshal.Copy(structPtr, getBytes, 0, structSize);
                Marshal.FreeHGlobal(structPtr);

                if (size == 14)
                {
                    var newBytes = new byte[size];
                    var count = 0;
                    var loop = 0;

                    for (loop = 0; loop < structSize; loop++)
                        if (loop != 2 && loop != 3)
                        {
                            newBytes[count] = getBytes[loop];
                            count++;
                        }

                    return newBytes;
                }
                return getBytes;
            }
            catch // (Exception ex)
            {
                //ZKCE.SysException.ZKCELogger logger = new ZKCE.SysException.ZKCELogger(ex);
                //logger.Append();

                return getBytes;
            }
        }

        /*******************************************
        * 函数名称：GetBitmap
        * 函数功能：将传进来的数据保存为图片
        * 函数入参：buffer---图片数据
        *           nWidth---图片的宽度
        *           nHeight---图片的高度
        * 函数出参：无
        * 函数返回：无
        *********************************************/

        public static void GetBitmap(byte[] buffer, int nWidth, int nHeight, MemoryStream ms)
        {
            ushort mNBitCount = 8;
            var mNColorTableEntries = 256;
            var resBuf = new byte[nWidth * nHeight * 2];

            try
            {
                var colorMask = new Mask[mNColorTableEntries];

                var w = (nWidth + 3) / 4 * 4;
                //图片头信息
                var bmpInfoHeader = new Bitmapinfoheader
                {
                    BiWidth = nWidth,
                    BiHeight = nHeight,
                    BiPlanes = 1,
                    BiBitCount = mNBitCount,
                    BiCompression = 0,
                    BiSizeImage = 0,
                    BiXPelsPerMeter = 0,
                    BiYPelsPerMeter = 0,
                    BiClrUsed = mNColorTableEntries,
                    BiClrImportant = mNColorTableEntries
                };

                bmpInfoHeader.BiSize = Marshal.SizeOf(bmpInfoHeader);

                //文件头信息
                var bmpHeader = new Bitmapfileheader
                {
                    BfType = 0x4D42,
                    BfOffBits = 14 + Marshal.SizeOf(bmpInfoHeader) + bmpInfoHeader.BiClrUsed * 4,
                    BfReserved1 = 0,
                    BfReserved2 = 0
                };
                bmpHeader.BfSize = bmpHeader.BfOffBits +
                                   (w * bmpInfoHeader.BiBitCount + 31) / 32 * 4 * bmpInfoHeader.BiHeight;

                ms.Write(StructToBytes(bmpHeader, 14), 0, 14);
                ms.Write(StructToBytes(bmpInfoHeader, Marshal.SizeOf(bmpInfoHeader)), 0,
                    Marshal.SizeOf(bmpInfoHeader));

                //调试板信息
                for (var colorIndex = 0; colorIndex < mNColorTableEntries; colorIndex++)
                {
                    colorMask[colorIndex].Redmask = (byte)colorIndex;
                    colorMask[colorIndex].Greenmask = (byte)colorIndex;
                    colorMask[colorIndex].Bluemask = (byte)colorIndex;
                    colorMask[colorIndex].RgbReserved = 0;

                    ms.Write(StructToBytes(colorMask[colorIndex], Marshal.SizeOf(colorMask[colorIndex])), 0,
                        Marshal.SizeOf(colorMask[colorIndex]));
                }

                //图片旋转，解决指纹图片倒立的问题
                RotatePic(buffer, nWidth, nHeight, ref resBuf);

                //byte[] filter = null;
                //if (w - nWidth > 0)
                //    filter = new byte[w - nWidth];
                for (var i = 0; i < nHeight; i++)
                {
                    ms.Write(resBuf, i * nWidth, nWidth);
                    if (w - nWidth > 0)
                        ms.Write(resBuf, 0, w - nWidth);
                }
            }
            catch // (Exception ex)
            {
                // ZKCE.SysException.ZKCELogger logger = new ZKCE.SysException.ZKCELogger(ex);
                // logger.Append();
            }
        }

        /*******************************************
        * 函数名称：WriteBitmap
        * 函数功能：将传进来的数据保存为图片
        * 函数入参：buffer---图片数据
        *           nWidth---图片的宽度
        *           nHeight---图片的高度
        * 函数出参：无
        * 函数返回：无
        *********************************************/

        public static void WriteBitmap(byte[] buffer, int nWidth, int nHeight)
        {
            ushort mNBitCount = 8;
            var mNColorTableEntries = 256;
            var resBuf = new byte[nWidth * nHeight];

            try
            {
                var colorMask = new Mask[mNColorTableEntries];
                var w = (nWidth + 3) / 4 * 4;
                //图片头信息
                var bmpInfoHeader = new Bitmapinfoheader
                {
                    BiWidth = nWidth,
                    BiHeight = nHeight,
                    BiPlanes = 1,
                    BiBitCount = mNBitCount,
                    BiCompression = 0,
                    BiSizeImage = 0,
                    BiXPelsPerMeter = 0,
                    BiYPelsPerMeter = 0,
                    BiClrUsed = mNColorTableEntries,
                    BiClrImportant = mNColorTableEntries
                };
                bmpInfoHeader.BiSize = Marshal.SizeOf(bmpInfoHeader);
                //文件头信息
                var bmpHeader = new Bitmapfileheader
                {
                    BfType = 0x4D42,
                    BfOffBits = 14 + Marshal.SizeOf(bmpInfoHeader) + bmpInfoHeader.BiClrUsed * 4,
                    BfReserved1 = 0,
                    BfReserved2 = 0
                };
                bmpHeader.BfSize = bmpHeader.BfOffBits +
                                   (w * bmpInfoHeader.BiBitCount + 31) / 32 * 4 * bmpInfoHeader.BiHeight;

                Stream fileStream = File.Open("finger.bmp", FileMode.Create, FileAccess.Write);
                var tmpBinaryWriter = new BinaryWriter(fileStream);

                tmpBinaryWriter.Write(StructToBytes(bmpHeader, 14));
                tmpBinaryWriter.Write(StructToBytes(bmpInfoHeader, Marshal.SizeOf(bmpInfoHeader)));

                //调试板信息
                for (var colorIndex = 0; colorIndex < mNColorTableEntries; colorIndex++)
                {
                    colorMask[colorIndex].Redmask = (byte)colorIndex;
                    colorMask[colorIndex].Greenmask = (byte)colorIndex;
                    colorMask[colorIndex].Bluemask = (byte)colorIndex;
                    colorMask[colorIndex].RgbReserved = 0;

                    tmpBinaryWriter.Write(StructToBytes(colorMask[colorIndex],
                        Marshal.SizeOf(colorMask[colorIndex])));
                }

                //图片旋转，解决指纹图片倒立的问题
                RotatePic(buffer, nWidth, nHeight, ref resBuf);

                //写图片
                //TmpBinaryWriter.Write(ResBuf);
                byte[] filter = null;
                if (w - nWidth > 0)
                    filter = new byte[w - nWidth];
                for (var i = 0; i < nHeight; i++)
                {
                    tmpBinaryWriter.Write(resBuf, i * nWidth, nWidth);
                    if (w - nWidth > 0)
                        tmpBinaryWriter.Write(resBuf, 0, w - nWidth);
                }

                fileStream.Close();
                tmpBinaryWriter.Close();
            }
            catch // (Exception ex)
            {
                //ZKCE.SysException.ZKCELogger logger = new ZKCE.SysException.ZKCELogger(ex);
                //logger.Append();
            }
        }

        public struct Bitmapfileheader
        {
            public ushort BfType;
            public int BfSize;
            public ushort BfReserved1;
            public ushort BfReserved2;
            public int BfOffBits;
        }

        public struct Mask
        {
            public byte Redmask;
            public byte Greenmask;
            public byte Bluemask;
            public byte RgbReserved;
        }

        public struct Bitmapinfoheader
        {
            public int BiSize;
            public int BiWidth;
            public int BiHeight;
            public ushort BiPlanes;
            public ushort BiBitCount;
            public int BiCompression;
            public int BiSizeImage;
            public int BiXPelsPerMeter;
            public int BiYPelsPerMeter;
            public int BiClrUsed;
            public int BiClrImportant;
        }
    }
}