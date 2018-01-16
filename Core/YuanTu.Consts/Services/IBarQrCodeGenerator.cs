using System.Drawing;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Consts.Services
{
    /// <summary>
    /// 条码二维码生成
    /// </summary>
    public interface IBarQrCodeGenerator:IService
    {
        /// <summary>
        /// 生成条码或二维码
        /// </summary>
        /// <param name="content">条码内容</param>
        /// <param name="mergeImage">中间嵌入的图片</param>
        /// <param name="format">格式(条码多种格式，或者二维码)</param>
        /// <returns></returns>
        Image Generate(string content,Image mergeImage, BarQrcodeFormat format);

        /// <summary>
        /// 二维码生成
        /// </summary>
        /// <param name="content">生成的内容</param>
        /// <param name="mergeImage">嵌入的小图片</param>
        /// <returns></returns>
        Image QrcodeGenerate(string content, Image mergeImage = null);
        /// <summary>
        /// 条码生成
        /// </summary>
        /// <param name="content">生成的内容</param>
        /// <param name="format">格式</param>
        /// <returns></returns>
        Image BarcodeGenerate(string content, BarQrcodeFormat format = BarQrcodeFormat.CODE_128);
    }

}
