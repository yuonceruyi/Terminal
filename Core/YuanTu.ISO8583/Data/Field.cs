using YuanTu.ISO8583.Enums;

namespace YuanTu.ISO8583.Data
{
    public class Field
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Length { get; set; }

        public Format Format { get; set; }

        public VarType VarType { get; set; }

        public override string ToString()
        {
            return $"{Id:D3} {Name} {Length:D3} {Format} {VarType}";
        }
    }
}