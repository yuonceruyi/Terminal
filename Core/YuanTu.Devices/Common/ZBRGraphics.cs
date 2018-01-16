using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YuanTu.Devices.UnSafeMethods;

namespace YuanTu.Devices.Common
{
    public class ZBRGraphics
    {
        #region Print Spooler

        // Checks to see if any jobs are in the print spooler -----------------------------------------------

        public int IsPrinterReady(byte[] drvName, out int errValue)
        {
            return ZBRGDIIsPrinterReady(drvName, out errValue);
        }

        #endregion Print Spooler

        #region SDK DLL Version

        // Gets ZBRGraphics.dll Version ---------------------------------------------------------------------

        public void GetSDKVer(out int major, out int minor, out int engLevel)
        {
            ZBRGDIGetSDKVer(out major, out minor, out engLevel);
        }

        #endregion SDK DLL Version

        #region Print

        // Prints the graphic buffer ------------------------------------------------------------------------

        public int PrintGraphics(out int errValue)
        {
            return ZBRGDIPrintGraphics(_hDC, out errValue);
        }

        #endregion Print

        #region Private Variables

        // Private declarations -----------------------------------------------------------------------------

        private IntPtr _hDC;

        public IntPtr HDC
        {
            get { return _hDC; }
        }

        #endregion Private Variables

        #region Constructor

        // Class Initialization -----------------------------------------------------------------------------

        #endregion Constructor

        

        #region Graphics Initialization

        // Creates a device context and initializes a graphic buffer ----------------------------------------

        public int InitGraphics(byte[] drvName, out int errValue)
        {
            return ZBRGDIInitGraphics(drvName, out _hDC, out errValue);
        }

        // Releases the device context and the graphic buffer -----------------------------------------------

        public int CloseGraphics(out int errValue)
        {
            return ZBRGDICloseGraphics(_hDC, out errValue);
        }

        // Clears the graphic buffer ------------------------------------------------------------------------

        public int ClearGraphics(out int errValue)
        {
            return ZBRGDIClearGraphics(out errValue);
        }

        #endregion Graphics Initialization

        #region Draw

        // Draws text into the graphic buffer ---------------------------------------------------------------

        public int DrawText(int x, int y, byte[] text, byte[] font, int fontSize, int fontStyle,
            int textColor, out int errValue)
        {
            return ZBRGDIDrawText(x, y, text, font, fontSize, fontStyle, textColor, out errValue);
        }

        // Draws a line into the graphic buffer ---------------------------------------------------------------

        public int DrawLine(int x1, int y1, int x2, int y2, int color, float thickness,
            out int err)
        {
            return ZBRGDIDrawLine(x1, y1, x2, y2, color, thickness, out err);
        }

        // Places a file image into the graphic buffer ------------------------------------------------------

        public int DrawImage(byte[] filename, int x, int y, int width, int height, out int errValue)
        {
            return ZBRGDIDrawImageRect(filename, x, y, width, height, out errValue);
        }

        // Draws a barcode into the monochrome buffer -------------------------------------------------------

        public int DrawBarcode(int x, int y, int rotation, int barcodeType, int widthRatio, int multiplier,
            int height, int textUnder, byte[] barcodeData, out int errValue)
        {
            return ZBRGDIDrawBarCode(x, y, rotation, barcodeType, widthRatio, multiplier, height, textUnder,
                barcodeData, out errValue);
        }

        #endregion Draw
    }
}
