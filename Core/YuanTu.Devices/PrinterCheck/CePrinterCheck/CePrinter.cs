using System;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.Devices.PrinterCheck.CePrinterCheck
{
    public class CePrinter
    {
        public static Result<uint> CallInitUsb()
        {
            var libError = uint.MaxValue;
            var prnConnectedEnum = uint.MaxValue;    // The number of connected CUSTOM printers            
            try
            {
                prnConnectedEnum = IntercomModule.CePrnInitUsb(ref libError);
            }
            catch (Exception ex)
            {
                return Result<uint>.Fail(ex.Message);
            }
            var logMessage = ($"[CePrinter]Library Error:{libError.ToString()} - Number of Connected Printers:{prnConnectedEnum.ToString()}");
            Logger.Device.Info(logMessage);
            return prnConnectedEnum <= 0 ? Result<uint>.Fail($"[CePrinter]CallInitUsb:No printer connected") :
                                           Result<uint>.Success(prnConnectedEnum);
        }

        public static  Result<int> CallInterfaceNumUsb(string prnName)
        {
            uint libError;
            var prnDevNum = uint.MaxValue;
            var prnDevNumResult = int.MaxValue;

            if (string.IsNullOrEmpty(prnName))
            {
                // Printer name is empty
                return Result<int>.Fail($"[CePrinter]CallInterfaceNumUsb:prnName:{prnName} Printer name is mandatory");
            }
            try
            {
                libError = IntercomModule.CePrnGetInterfaceNumUsb(prnName, ref prnDevNum);
                prnDevNumResult = Convert.ToInt32(prnDevNum);
            }
            catch (Exception ex)
            {
                return Result<int>.Fail($"[CePrinter]CePrnGetInterfaceNumUsb:prnName:{prnName} {ex.Message}");
            }

            var logMessage =$"Library Error:{libError} - Printer Name:'{prnName}' - Printer DevNum:{prnDevNum}";
            Logger.Device.Info(logMessage);

            // Verify if an error happened
            if (libError != 0)
            {// Error. Set the device number to an invalid value
                return Result<int>.Fail($"[CePrinter]CePrnGetInterfaceNumUsb:prnName:{prnName} Wrong printer name");
            }
            return Result<int>.Success(prnDevNumResult);
        }
        public static Result CallPrnGetFullModelUsb(int prnDevNum, ref uint prnModel)
        {
            var libError = uint.MaxValue;
            var sysError = uint.MaxValue;
            try
            {
                // Get printer model from the printer
                libError = IntercomModule.CePrnGetFullModelUsb(prnDevNum, ref prnModel, ref sysError);
            }
            catch (Exception ex)
            {
                return Result.Fail($"[CePrinter]CePrnGetFullModelUsb:{ex.Message}");
            }

            var logMessage =$"Library Error:{libError} - Printer Model:{prnModel} - System Error:{sysError}";
         
            Logger.Device.Info(logMessage);

            if (libError == 0)
            {
                // No communication error. The printer is connected
                return Result.Success();
            }
            return Result.Fail($"[CePrinter]CePrnGetFullModelUsb:Has communication error");
        }
        public static Result CallSetPrinterModelUsb(int prnDevNum, uint _prnModel)
        {
            uint libError;
            try
            {
                libError = IntercomModule.SetUsbPrinterModelUsb(prnDevNum, _prnModel);
            }
            catch (Exception ex)
            {
                return Result.Fail($"[CePrinter]SetUsbPrinterModelUsb:{ex.Message}");
            }

            var logMessage = $"Library Error:{libError} - Set printer model.";
            
            Logger.Device.Info(logMessage);

            return Result.Success();
        }
        public static Result<uint> CePrnGetStsUsb(int prnDevNum)
        {
            var libError = uint.MaxValue;
            var sysError = uint.MaxValue;
            uint prnStatus = 0;

            try
            {
                // Get staus from printer
                libError = IntercomModule.CePrnGetStsUsb(prnDevNum, ref prnStatus, ref sysError);
            }
            catch (Exception ex)
            {
                return Result<uint>.Fail($"[CePrinter]CePrnGetStsUsb:{ex.Message}");
            }

            var logMessage =
                $"Library Error:{libError} - Printer Status 0x{prnStatus.ToString("X8")} - System Error: 0x{sysError.ToString("X8")}";
           Logger.Device.Info(logMessage);
           

            // prnStatus contains current printer status
            return Result<uint>.Success(prnStatus);
        }
        public static Result<Status> DecodePrintStatus(uint code)
        {
            // More than one of the following status can be segnaled at the same time.
            // To know how to decode other possible printer status, see the note into
            // #region Consts
            var paperEnd = Convert.ToBoolean(code & NOPAPER);
            var nearpaperEnd = Convert.ToBoolean(code & NEARPAPEREND);
            var ticketOut = Convert.ToBoolean(code & TICKETOUT);
            var paperJam = Convert.ToBoolean(code & PAPERJAM);
            var coverOpen = Convert.ToBoolean((code & NOCOVER) | (code & NOHEAD));
            return Result<Status>.Success(
                new Status
            {
                PaperEnd = paperEnd,
                NearpaperEnd = nearpaperEnd,
                TicketOut = ticketOut,
                PaperJam = paperJam,
                CoverOpen = coverOpen
            });
        }

        public static uint _prnModel;
        public static int _prnDevNum;
       
        #region Consts
        //******************************************************************
        //* ATTENTION:                                                     
        //* The following constants are samples of a generic PRINTER STATUS.
        //* Please verify into your Printer "Commands Manual" at command 
        //* DLE EOT 20 (0x10 0x04 0x14, get FULL STATUS) to verify the 
        //* right correspondance of FULL printer status, and to get
        //* information about other printer status (overtemperature, power
        //* error etc).
        //*
        //******************************************************************
        private const uint NOPAPER = 0x00000001;
        private const uint NEARPAPEREND = 0x00000004;
        private const uint TICKETOUT = 0x00000020;
        private const uint NOHEAD = 0x00000100;
        private const uint NOCOVER = 0x00000200;
        private const uint PAPERJAM = 0x00400000;
        #endregion
    }
}
