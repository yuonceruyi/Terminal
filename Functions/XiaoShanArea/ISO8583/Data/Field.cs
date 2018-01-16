using YuanTu.YuHangArea.ISO8583.Enums;

namespace YuanTu.YuHangArea.ISO8583.Data
{
    public class Field
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Length { get; set; }

        public Format Format { get; set; }

        public VarType VarType { get; set; }
    }
}