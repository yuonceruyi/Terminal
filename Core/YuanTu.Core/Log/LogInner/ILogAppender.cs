using System.Collections.Generic;

namespace YuanTu.Core.Log
{
    internal interface ILogAppender
    {
        void Insert(SQLiteLogger logger, LogInfo[] infos);
    }
}