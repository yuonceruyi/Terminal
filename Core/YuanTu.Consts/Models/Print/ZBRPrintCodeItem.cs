namespace YuanTu.Consts.Models.Print
{
    public class ZbrPrintCodeItem
    {
        public ZbrPrintCodeItem(int x = 300, int y = 300, int rotation = 0, int barCodeType = 7, int widthRatio = 0,
            int multiplier = 4, int height = 94, bool textUnder = false, string barCodeData = null)
        {
            X = x;
            Y = y;
            Rotation = rotation;
            BarCodeType = barCodeType;
            WidthRatio = widthRatio;
            Multiplier = multiplier;
            Height = height;
            TextUnder = textUnder ? 1 : 0;
            BarCodeData = barCodeData ?? string.Empty;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Rotation { get; set; }
        public int BarCodeType { get; set; }
        public int WidthRatio { get; set; }
        public int Multiplier { get; set; }
        public int Height { get; set; }
        public int TextUnder { get; set; }
        public string BarCodeData { get; set; }
    }
}