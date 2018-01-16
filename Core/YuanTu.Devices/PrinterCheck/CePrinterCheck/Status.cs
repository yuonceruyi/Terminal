namespace YuanTu.Devices.PrinterCheck.CePrinterCheck
{
    public class Status
    {
        // Verify if a paper end is segnaled.
        public bool PaperEnd { get; set; }
        // Verify if a near paper end is segnaled.
        public bool NearpaperEnd { get; set; }
        // Verify if a ticket out is segnaled.
        public bool TicketOut { get; set; }
        // Verify if a paper jam is segnaled.
        public bool PaperJam { get; set; }
        // Verify if a cover open / Head up status is segnaled.
        public bool CoverOpen { get; set; }
    }
}
