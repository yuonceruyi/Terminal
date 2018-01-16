using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace YuanTu.Consts.Models.Print
{
    public enum ImageAlign
    {
        None,
        Left,
        Center,
        Right
    }

    public interface IPrintable
    {
        float GetHeight(Graphics g, float w);

        float Print(Graphics g, float y, float w);

        string GetLogText();
    }

    public class PrintItemBase : IPrintable
    {
        public float X;

        public PrintItemBase()
        {
            X = PrintConfig.DefaultX;
        }

        public virtual float GetHeight(Graphics g, float w)
        {
            return 0;
        }

        public virtual float Print(Graphics g, float y, float w)
        {
            return GetHeight(g, w);
        }
        //用于记录到日志中的辅助方法
        public virtual string GetLogText()
        {
            return "[Error]" + this.GetType();
        }
    }

    public class PrintItemGap : IPrintable
    {
        public float Gap;

        public PrintItemGap()
        {
            Gap = 10;
        }

        public virtual float GetHeight(Graphics g, float w)
        {
            return Gap;
        }

        public virtual float Print(Graphics g, float y, float w)
        {
            return GetHeight(g, w);
        }
        public virtual string GetLogText()
        {
            return "\n";
        }
    }

    public class PrintItemText : PrintItemBase
    {
        public Font Font;
        public StringFormat StringFormat = PrintConfig.Left;
        public string Text;

        public PrintItemText()
        {
            Font = PrintConfig.DefaultFont;
        }

        public override float GetHeight(Graphics g, float w)
        {
            return g.MeasureString(Text, Font, (int)w).Height;
        }

        public override float Print(Graphics g, float y, float w)
        {
            var h = GetHeight(g, w);
            var rect = new RectangleF(X, y, w, h);
            g.DrawString(Text, Font, Brushes.Black, rect, StringFormat);
            return h;
        }

        public override string GetLogText()
        {
            return Text;
        }
    }

    public class PrintItemTriText : PrintItemText
    {
        public string Text2;
        public string Text3;
        public float Text3X = PrintConfig.Default3X;
       
        public PrintItemTriText()
        {
        }
        public PrintItemTriText(string t1, string t2, string t3)
        {
            Text = t1;
            Text2 = t2;
            Text3 = t3;
        }

        public PrintItemTriText(string t1, string t2, string t3, float t4)
        {
            Text = t1;
            Text2 = t2;
            Text3 = t3;
            Text3X = t4;
        }

        public override float GetHeight(Graphics g, float w)
        {
            return new[]
            {
                g.MeasureString(Text, Font, 160).Height,
                g.MeasureString(Text2, Font, 50).Height,
                g.MeasureString(Text3, Font, 80).Height,
            }.Max();
        }
        
        public override float Print(Graphics g, float y, float w)
        {
            var h = GetHeight(g, w);
            g.DrawString(Text, Font, Brushes.Black, new RectangleF(10, y, 160, h));
            g.DrawString(Text2, Font, Brushes.Black, new RectangleF(170, y, 50, h));
            g.DrawString(Text3, Font, Brushes.Black, new RectangleF(Text3X, y, 80, h));

            return h;
        }

        public override string GetLogText()
        {
            return $"{Text} {Text2} {Text3}";
        }
    }

    public class PrintItemRatioText : PrintItemText
    {
        private readonly string[] _textArr;
        public int[] TextSpaceRatios { get; set; } = {4, 3, 3};

        public PrintItemRatioText(params string[] items)
        {
            _textArr = items;
        }

        public override float Print(Graphics g, float y, float w)
        {
            ;
            //设计宽度：300py
            var textarrLen = _textArr.Length;
            var realRatioArr = TextSpaceRatios;
            if (textarrLen < TextSpaceRatios.Length)
            {
                realRatioArr = TextSpaceRatios.Take(textarrLen).ToArray();
            }
            else if (textarrLen > TextSpaceRatios.Length)
            {
                var tmpcount = textarrLen - TextSpaceRatios.Length;
                var tmparr = new int[tmpcount];
                for (int i = 0; i < tmpcount; i++)
                {
                    tmparr[i] = 0;
                }
                realRatioArr = TextSpaceRatios.Concat(tmparr).ToArray();
            }
            var totalpst = realRatioArr.Sum(); //所有权重
            var start = PrintConfig.DefaultX;
            var content = w - start-start;
            var maxHeight = 0f;
            for (int i = 0; i < _textArr.Length; i++)
            {
                var h = GetHeight(_textArr[i],g, w);
                maxHeight = Math.Max(maxHeight, h);
                var width = (float) realRatioArr[i]/totalpst*content;
                var rect = new RectangleF(start, y, width, maxHeight);
                start += width;
                g.DrawString(_textArr[i], Font, Brushes.Black, rect);
            }
            return maxHeight;
        }

        public  float GetHeight(string text,Graphics g, float w)
        {
            return g.MeasureString(text, Font, (int)w).Height;
        }

        public override string GetLogText()
        {
            return string.Join(" ", _textArr);
        }
    }

    public class PrintItemImage : PrintItemBase
    {
        public ImageAlign Align;
        public float Height;
        public Image Image;
        public float Width;

        public override float GetHeight(Graphics g, float w)
        {
            return Height;
        }

        public override float Print(Graphics g, float y, float w)
        {
            var x = X;
            switch (Align)
            {
                case ImageAlign.Center:
                    x = (w - Width) / 2;
                    break;

                case ImageAlign.Right:
                    x = w - Width - X;
                    break;
            }
            g.DrawImage(Image, new RectangleF(x, y, Width, Height));
            return GetHeight(g, w);
        }

        public override string GetLogText()
        {
            return "[图片]";
        }
    }
}
