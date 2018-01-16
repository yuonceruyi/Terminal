using System;

namespace YuanTu.Core.Log
{
    public enum LogType
    {
        Log_Update,
        Log_Main,
        Log_Net, 
        Log_Device,
        [Obsolete]
        Log_Cash, 
        Log_POS, 
        Log_Printer,
        Log_Maintenance,
    }
}