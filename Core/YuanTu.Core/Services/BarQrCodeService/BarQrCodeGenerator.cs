using System.Drawing;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Services;
using ZXing;
using ZXing.Common;

namespace YuanTu.Core.Services.BarQrCodeService
{
    public  class BarQrCodeGenerator: IBarQrCodeGenerator
    {
        public Image Generate(string content, Image mergeImage, BarQrcodeFormat format)
        {
            var barcodeWriter = new BarcodeWriter { Format = (BarcodeFormat)(int)format };
            var opts = new EncodingOptions
            {
                Height = 1024,
                Width = 1024,
            };
            opts.Hints[EncodeHintType.ERROR_CORRECTION] = ZXing.QrCode.Internal.ErrorCorrectionLevel.H;
            opts.Hints[EncodeHintType.CHARACTER_SET] = "utf-8";
            opts.Hints[EncodeHintType.MARGIN] = 0;
            barcodeWriter.Options = opts;
            //barcodeWriter.Renderer=new BitmapRenderer() {Background = Color.Transparent,Foreground = Color.Blue};
            var bitmap = barcodeWriter.Write(content);
            return MergerImg(bitmap, mergeImage);
        }

        public Image QrcodeGenerate(string content, Image mergeImage = null)
        {
            return Generate(content, mergeImage, BarQrcodeFormat.QR_CODE);
        }

        public Image BarcodeGenerate(string content, BarQrcodeFormat format = BarQrcodeFormat.CODE_128)
        {
            return Generate(content, null, format);
        }

        private static Bitmap MergerImg(Image qrCode, Image margeImg)
        {
            if (margeImg == null)
            {
                return (Bitmap)qrCode;
            }
            var backgroudImg = new Bitmap(qrCode.Width, qrCode.Height);
            // 
            using (var g = Graphics.FromImage(backgroudImg))
            {
                //�������,��������Ϊ��ɫ
                g.Clear(Color.White);
                g.DrawImage(qrCode, 0, 0, qrCode.Width, qrCode.Height);
                //ȷ���м�ͼ��Ϊ��ά���1/7,
                var size = new Size(qrCode.Width / 7, qrCode.Height / 7);
                var left = (qrCode.Width - size.Width) / 2;
                var top = (qrCode.Height - size.Height) / 2;
                g.DrawImage(margeImg, left, top, size.Width, size.Height);
            }
            return backgroudImg;
        }

        public string ServiceName => "����ZXing�Ķ�ά������";
    }
}