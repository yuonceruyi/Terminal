using System.Net;

namespace YuanTu.ISO8583.Interfaces
{
    public interface IConfig
    {
        IPAddress Address { get; set; }
        int Port { get; set; }

        string TPDU { get; set; }
        string Head { get; set; }
        string MerchantId { get; set; }
        string TerminalId { get; set; }

        int BatchNo { get; set; }
        int TransSeq { get; set; }
        bool IsLogon { get; set; }
        int MainKeyIndex { get; set; }


        string AcqInst { get; set; }
        string Field_2F01 { get; set; }
        string Field_48 { get; set; }
    }
}