using System.Drawing;

namespace YuanTu.Consts.Models.Print
{
    public class ZbrPrintTextItem
    {
        public ZbrPrintTextItem(int x = 100, int y = 100, string text = null, string font = "黑体", int fontSize = 12,
            FontStyle fontStyle = FontStyle.Bold)
        {
            X = x;
            Y = y;
            Text = text ?? string.Empty;
            Font = font;
            FontSize = fontSize;
            FontStyle = fontStyle;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public string Text { get; set; }
        public string Font { get; set; }
        public int FontSize { get; set; }
        public FontStyle FontStyle { get; set; }
    }
}