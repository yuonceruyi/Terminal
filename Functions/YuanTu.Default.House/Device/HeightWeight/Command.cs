namespace YuanTu.Default.House.Device.HeightWeight
{
    public class Command
    {
        public byte[] Head { get; set; }
        public byte Function { get; set; }
        public byte[] Height { get; set; } = new byte[2];
        public byte[] Weihgt { get; set; } = new byte[2];
        public byte[] Extend { get; set; }
        

    }
}
