using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.Devices.Printer
{
    /// <summary>
    /// 打印机基础接口
    /// </summary>
    public interface IPrintable
    {
        /// <summary>
        /// 检测打印机
        /// </summary>
        /// <param name="context">打印机上下文</param>
        Attempt<int> Detect(PrintableContext context);

        /// <summary>
        /// 连接打印机
        /// </summary>
        /// <param name="context">打印机上下文</param>
        Attempt<int> Connect(PrintableContext context);

        /// <summary>
        /// 断开打印机链接
        /// </summary>
        Attempt<int> Disconnect();

        /// <summary>
        /// 打印对象
        /// </summary>
        Attempt<int> Print(object printable, IPrintableFormatter formatter = null);

        /// <summary>
        /// 测试打印机打印
        /// </summary>
        Attempt<int> PrintExample();

        /// <summary>
        /// 获取打印机上下文
        /// </summary>
        PrintableContext GetContext();

        bool Available(PrintableContext context);

        #region 打印命令

        /// <summary>
        /// 向前走纸
        /// </summary>
        /// <param name="count">走纸行数</param>
        IPrintable FeedLine(int count = 0);

        /// <summary>
        /// 设置字符的行高
        /// </summary>
        /// <param name="distince">指定行高点数,可以为 0 到 255,-1时会设置为1/6英寸</param>
        /// <remarks>
        /// 1、如果把行高设置为0，则打印机使用内部的默认行高值，即1/6英寸。如果打印头纵向分辨率为180dpi 则相当于 31 点高
        /// 2、如果行高被设置为小于当前的字符高度，则打印机将使用当前字符高度为行高
        /// </remarks>
        IPrintable SetLineSpacing(int distince = -1);

        /// <summary>
        /// 设置字符的右间距（相邻两个字符的间隙距离）
        /// </summary>
        /// <param name="distince">指定右间距的点数,可以为 0 到 255</param>
        /// <remarks>
        /// 1、字符右间距的设置在标准模式和页模式或标签模式是独立的
        /// 2、如果字符放大，则字符右间距同倍放大
        /// </remarks>
        IPrintable SetRightSpacing(int distince = 0);

        /// <summary>
        /// 切纸
        /// </summary>
        IPrintable CutPaper(int mode = 65, int distince = 0);

        /// <summary>
        /// 设置加粗模式
        /// </summary>
        /// <param name="enabled">是否开启加粗</param>
        /// <param name="single">字符加粗</param>
        IPrintable SetBold(bool enabled = true, bool single = false);

        /// <summary>
        /// 设置下划线模式
        /// </summary>
        /// <param name="enabled">是否开启下划线</param>
        /// <param name="boldUnderline">下划线是否加粗</param>
        IPrintable SetUnderline(bool enabled = true, bool boldUnderline = false);

        /// <summary>
        /// 设置倒置打印
        /// </summary>
        /// <param name="enabled">是否开启倒置打印</param>
        IPrintable SetUpsideDown(bool enabled = true);

        /// <summary>
        /// 顺时针旋转90度
        /// </summary>
        /// <param name="enabled">是否开启</param>
        IPrintable ClockwiseRotate90Dgree(bool enabled = true);

        /// <summary>
        /// 逆时针旋转90度
        /// </summary>
        /// <param name="enabled">是否开启</param>
        IPrintable CounterClockwiseRotate90Dgree(bool enabled = true);

        /// <summary>
        /// 设置字体大小(水平方向和垂直方向取值都为0-5)
        /// </summary>
        IPrintable SetFontSize(byte width = 0, byte height = 0);

        /// <summary>
        /// 设置黑白反显打印
        /// </summary>
        /// <param name="enabled">是否开启</param>
        IPrintable SetInvertText(bool enabled = true);

        /// <summary>
        /// 设置对齐
        /// </summary>
        /// <param name="mode">0:左对齐，1:居中,2:右对齐</param>
        IPrintable SetAlign(int mode);

        /// <summary>
        /// 输出文字
        /// </summary>
        IPrintable Text(string text);

        /// <summary>
        /// 重置
        /// </summary>
        IPrintable Reset();

        IPrintable SetChineseSpacing(int leftDistince = 0, int rightDistince = 0);

        IPrintable Bitmap(Bitmap image);

        /// <summary>
        /// 页模式
        /// </summary>
        IPrintable PageMode();

        /// <summary>
        /// 标准模式
        /// </summary>
        IPrintable StandardMode();

        IPrintable Eof();

        IPrintable SetMotionUnit(int h, int v);

        IPrintable RawControl(byte[] controlflow);

        #endregion

        void SelfTest();

        void Print();

        //PrintDefination GetDefination();
    }

    public static class PrintableFormatterFactory
    {
        private static readonly List<IPrintableFormatter> Registrer = new List<IPrintableFormatter>();

        public static IPrintableFormatter GetFormatter(object obj)
        {
            var tmp = Registrer;
            var formatter = tmp.FirstOrDefault(p => p.CanHandle(obj));
            return formatter ?? NullPrintableFormatter.Instance;
        }
    }

    public enum PrinterFontTypes
    {
        /// <summary>
        /// 标准
        /// </summary>
        StandardAscii = 0,

        /// <summary>
        /// 压缩
        /// </summary>
        CompressAscii = 1,

        UserDefined = 2,

        /// <summary>
        /// 标准 “宋体”
        /// </summary>
        Chinese = 3
    }

    public enum FontStyle
    {

    }

    /// <summary>
    /// 表示一台打印机
    /// </summary>
    public abstract class PrintableBase : IPrintable
    {
        protected PrintableContext Context;
        protected bool Connected = false;

        protected void EnsureConnected()
        {
            if (!Connected || Context == null)
            {
                throw new Exception("打印机没有连接");
            }
        }

        protected abstract Attempt<int> InternalConnect();

        protected abstract Attempt<int> InternalDisconnect();

        public abstract void InternalSendCommands(byte[] cmds);

        protected virtual Attempt<int> InternalDetectPrinter(PrintableContext context)
        {
            return Attempt.Succeed(0);
        }

        public virtual bool Available(PrintableContext context) { return true; }

        #region Implementation of IPrintable

        public Attempt<int> Detect(PrintableContext context)
        {
            try
            {
                return InternalDetectPrinter(context);
            }
            catch
            {
                return Attempt.Fail(-1);
            }
        }

        public Attempt<int> Connect(PrintableContext context)
        {
            Context = context;
            Connected = false;
            var result = InternalConnect();
            if (!result.Success)
            {
                Context = null;
            }
            else
            {
                Connected = true;
            }
            return result;
        }

        public Attempt<int> Disconnect()
        {
            if (!Connected)
            {
                return Attempt.Succeed(0);
            }
            var result = InternalDisconnect();
            if (result.Success)
            {
                Context = null;
                Connected = false;
            }
            return result;
        }

        public virtual Attempt<int> Print(object printable, IPrintableFormatter formatter = null)
        {
            if (printable == null)
            {
                return Attempt.Succeed(0);
            }
            if (formatter == null)
            {
                formatter = PrintableFormatterFactory.GetFormatter(printable);
            }

            try
            {
                Reset();
                formatter.Format(this, printable);
            }
            catch (Exception ex)
            {
                return Attempt.Fail(-1, ex.Message);
            }
            return Attempt.Succeed(0);
        }

        public virtual Attempt<int> PrintExample()
        {
            SelfTest();
            return Attempt.Succeed(0);
        }

        public abstract PrintableContext GetContext();

        public virtual IPrintable FeedLine(int count = 0)
        {
            EnsureConnected();
            if (count < 0) count = 1;
            if (count > 255) count = 255;
            Context.Append((byte)0x0A);
            Context.Append(new byte[] { 0x1B, 0x64, (byte)count });
            return this;
        }

        public virtual IPrintable SetLineSpacing(int distince = -1)
        {
            EnsureConnected();
            if (distince < 0 || distince > 255)
            {
                Context.Append(new byte[] { 0x1B, 0x32 });
            }
            else
            {
                Context.Append(new byte[] { 0x1B, 0x33, (byte)distince });
            }
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="distince"></param>
        /// <returns></returns>
        public virtual IPrintable SetLeftSpacing(int nL = 0, int nH = 0)
        {
            EnsureConnected();
            Context.Append(new byte[] { 0x1D, 0x4C, (byte)nL, (byte)nH });

            return this;
        }
        public virtual IPrintable SetRightSpacing(int distince = 0)
        {
            EnsureConnected();
            if (distince < 0 || distince > 255)
            {
                distince = 0;
            }
            Context.Append(new byte[] { 0x1B, 0x20, (byte)distince });
            return this;
        }

        public virtual IPrintable CutPaper(int mode = 65, int distince = 0)
        {
            EnsureConnected();
            if (mode == -1)
            {
                Context.Append(new byte[] { (byte)'\n', 0x1B, 0x69 });
            }
            else
            {
                if (mode == 65 || mode == 66)
                {
                    Context.Append(new byte[] { (byte)'\n', 0x1D, 0x56, (byte)mode, (byte)distince });
                }
                else
                {
                    Context.Append(new byte[] { (byte)'\n', 0x1D, 0x56, (byte)mode });
                }
            }
            return this;
        }

        public virtual IPrintable SetBold(bool enabled = true, bool single = false)
        {
            EnsureConnected();
            Context.Append(new byte[] { 0x1B, single ? (byte)0x47 : (byte)0x45, enabled ? (byte)1 : (byte)0 });
            return this;
        }

        public virtual IPrintable SetUnderline(bool enabled = true, bool boldUnderline = false)
        {
            EnsureConnected();
            Context.Append(new byte[] { 0x1C, 0x2D, enabled ? (boldUnderline ? (byte)2 : (byte)1) : (byte)0 });
            return this;
        }

        public virtual IPrintable SetUpsideDown(bool enabled = true)
        {
            EnsureConnected();
            Context.Append(new byte[] { 0x1B, 0x7B, enabled ? (byte)1 : (byte)0 });
            return this;
        }

        public virtual IPrintable ClockwiseRotate90Dgree(bool enabled = true)
        {
            EnsureConnected();
            Context.Append(new byte[] { 0x1B, 0x56, enabled ? (byte)1 : (byte)0 });
            return this;
        }

        public virtual IPrintable CounterClockwiseRotate90Dgree(bool enabled = true)
        {
            EnsureConnected();
            if (enabled)
            {
                Context.Append(new byte[] { 0x1B, 0x12 });
            }
            else
            {
                Context.Append(new byte[] { 0x1B, 0x56, 0 });
            }
            return this;
        }

        public virtual IPrintable SetFontSize(byte width = 0, byte height = 0)
        {
            EnsureConnected();
            width = (byte)(width * 16);
            var n = (width & 0xF0) | (height & 0x0F);
            Context.Append(new byte[] { 0x1D, 0x21, (byte)n });
            return this;
        }

        public virtual IPrintable SetInvertText(bool enabled = true)
        {
            EnsureConnected();
            Context.Append(new byte[] { 0x1D, 0x42, enabled ? (byte)1 : (byte)0 });
            return this;
        }

        public virtual IPrintable SetAlign(int mode)
        {
            EnsureConnected();
            Context.Append(new byte[] { 0x1B, 0x61, (byte)mode });
            return this;
        }

        public virtual IPrintable Text(string text)
        {
            EnsureConnected();
            //Context.Append(Encoding.GetEncoding("GB2312").GetBytes(text ?? ""));
            Context.Append(Encoding.Default.GetBytes(text ?? ""));
            //Context.Append((byte)0x0A);
            return this;
        }

        public virtual IPrintable Reset()
        {
            EnsureConnected();
            Context.Append(new byte[] { 0x1B, 0x40 });
            SetAlign(0);
            return this;
        }

        public IPrintable SetChineseSpacing(int leftDistince = 0, int rightDistince = 0)
        {
            EnsureConnected();
            if (leftDistince < 0 || leftDistince > 255)
            {
                leftDistince = 0;
            }
            if (rightDistince < 0 || rightDistince > 255)
            {
                rightDistince = 0;
            }
            Context.Append(new byte[] { 0x1C, 0x53, (byte)leftDistince, (byte)rightDistince });
            return this;
        }

        public IPrintable Bitmap(Bitmap image)
        {
            EnsureConnected();

            //Context.Append(new byte[] {0x1B, 0x2A, (byte) 33});

            BitmapData data = GetBitmapData(image);
            BitArray dots = data.Dots;
            byte[] width = BitConverter.GetBytes(data.Width);

            int offset = 0;
            MemoryStream stream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write((char)0x1B);
            bw.Write('@');

            bw.Write((char)0x1B);
            bw.Write('3');
            bw.Write((byte)24);

            while (offset < data.Height)
            {
                bw.Write((char)0x1B);
                bw.Write('*');         // bit-image mode
                bw.Write((byte)33);    // 24-dot double-density
                bw.Write(width[0]);  // width low byte
                bw.Write(width[1]);  // width high byte

                for (int x = 0; x < data.Width; ++x)
                {
                    for (int k = 0; k < 3; ++k)
                    {
                        byte slice = 0;
                        for (int b = 0; b < 8; ++b)
                        {
                            int y = (((offset / 8) + k) * 8) + b;
                            // Calculate the location of the pixel we want in the bit array.
                            // It'll be at (y * width) + x.
                            int i = (y * data.Width) + x;

                            // If the image is shorter than 24 dots, pad with zero.
                            bool v = false;
                            if (i < dots.Length)
                            {
                                v = dots[i];
                            }
                            slice |= (byte)((v ? 1 : 0) << (7 - b));
                        }

                        bw.Write(slice);
                    }
                }
                offset += 24;
                bw.Write((char)0x0A);
            }
            // Restore the line spacing to the default of 30 dots.
            bw.Write((char)0x1B);
            bw.Write('3');
            bw.Write((byte)30);

            bw.Flush();
            byte[] bytes = stream.ToArray();

            Context.Append(bytes);

            return this;
        }

        public IPrintable PageMode()
        {
            EnsureConnected();
            Context.Append(new byte[] { 0x0A, 0x1B, 0x4C });
            return this;
        }

        public IPrintable StandardMode()
        {
            EnsureConnected();
            Context.Append(new byte[] { 0x0A, 0x1B, 0x53 });
            return this;
        }

        public IPrintable Eof()
        {
            EnsureConnected();
            Context.Append(new byte[] { 0x0A });
            return this;
        }

        public IPrintable SetMotionUnit(int h, int v)
        {
            EnsureConnected();
            Context.Append(new byte[] { 0x1D, 0x50, (byte)h, (byte)v });
            return this;
        }

        public IPrintable RawControl(byte[] controlflow)
        {
            EnsureConnected();
            Context.Append(controlflow);
            return this;
        }

        public IPrintable SetPrintArea(int x0, int y0, int dx, int dy)
        {
            return null;
        }


        public virtual void SelfTest()
        {
            EnsureConnected();
            Context.Append(new byte[] { 29, 40, 65, 2, 0, 0, 2 });
        }

        public void Print()
        {
            foreach (var cmd in Context.InternalBuffer)
            {
                InternalSendCommands(cmd);
            }
            if (Context.Buffer.Count > 0)
            {
                InternalSendCommands(Context.Buffer.ToArray());
            }
            Context.InternalBuffer.Clear();
            Context.Buffer.Clear();
        }

        //public PrintDefination GetDefination() {
        //    return Context == null ? PrintDefination.Empty : Context.PrintDefination;
        //}

        #endregion

        public BitmapData GetBitmapData(Bitmap bitmap)
        {
            using (bitmap)
            {
                const int threshold = 127;
                var index = 0;
                const double multiplier = 570;
                var scale = (double)(multiplier / (double)bitmap.Width);
                var xheight = (int)(bitmap.Height * scale);
                var xwidth = (int)(bitmap.Width * scale);
                var dimensions = xwidth * xheight;
                var dots = new BitArray(dimensions);

                for (var y = 0; y < xheight; y++)
                {
                    for (var x = 0; x < xwidth; x++)
                    {
                        var _x = (int)(x / scale);
                        var _y = (int)(y / scale);
                        var color = bitmap.GetPixel(_x, _y);
                        var luminance = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                        dots[index] = (luminance < threshold);
                        index++;
                    }
                }

                return new BitmapData
                {
                    Dots = dots,
                    Height = (int)(bitmap.Height * scale),
                    Width = (int)(bitmap.Width * scale)
                };
            }
        }

        public class BitmapData
        {
            public BitArray Dots { get; set; }

            public int Height { get; set; }

            public int Width { get; set; }
        }
    }

}

