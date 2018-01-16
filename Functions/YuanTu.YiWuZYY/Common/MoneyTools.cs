namespace YuanTu.YiWuZYY.Common
{
    public static class MoneyTools
    {
        public static decimal TrimToFen(this decimal fen)
        {
            return fen; //(int)(fen / 10 + 0.5m) * 10;
            //return Math.Round(fen / 10, MidpointRounding.AwayFromZero) * 10;
        }
    }
}
