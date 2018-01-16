using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.QDArea.QueueSignIn
{
    class CommandLine
    {
        public static void ParseCmd(string name, ref string value)
        {
            var prefix = $"{name}=";
            var argName = Environment.GetCommandLineArgs().FirstOrDefault(one => one.StartsWith(prefix));
            if (!string.IsNullOrEmpty(argName))
                value = argName.Substring(prefix.Length);
        }

        public static string ParseCmd(string name, string defaultValue)
        {
            ParseCmd(name, ref defaultValue);
            return defaultValue;
        }
    }
}
