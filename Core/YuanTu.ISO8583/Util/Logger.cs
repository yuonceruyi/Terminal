using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;

namespace YuanTu.ISO8583.Util
{
    public class POSLogger
    {
        static POSLogger()
        {
            string[] list = {"Pos_Main", "Pos_Net", "Pos_Device"};
            var layout = new PatternLayout
            {
                ConversionPattern = "[%date{HH:mm:ss.fff}][%-5level]%message%newline"
            };
            layout.ActivateOptions();
            foreach (var s in list)
            {
                var repo = LogManager.CreateRepository(s);
                var rfAppender = new RollingFileAppender
                {
                    File = "Logs\\" + s + ".log",
                    ImmediateFlush = true,
                    AppendToFile = true,
                    RollingStyle = RollingFileAppender.RollingMode.Date,
                    DatePattern = "_yyyyMMdd",
                    StaticLogFileName = false,
                    PreserveLogFileNameExtension = true,
                    Layout = layout
                };
                var cAppender = new ConsoleAppender
                {
                    Target = ConsoleAppender.ConsoleOut,
                    Layout = layout
                };
                BasicConfigurator.Configure(repo, rfAppender, cAppender);
                rfAppender.ActivateOptions();
                cAppender.ActivateOptions();
            }

            Main = LogManager.GetLogger("Pos_Main", "Main");
            Net = LogManager.GetLogger("Pos_Net", "Net");
            Device = LogManager.GetLogger("Pos_Device", "Device");
        }

        public static ILog Main { get; }
        public static ILog Net { get; }
        public static ILog Device { get; }
    }
}