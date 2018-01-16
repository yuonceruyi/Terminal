using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.Consts
{
    public class CommandLineSwitches
    {
        public static bool AllowMulitipleInstances { get; }

        static CommandLineSwitches()
        {
            var cmd = Environment.GetCommandLineArgs();
            AllowMulitipleInstances = cmd.Contains(nameof(AllowMulitipleInstances));
        }
    }
}
