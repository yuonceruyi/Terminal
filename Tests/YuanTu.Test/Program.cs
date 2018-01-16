using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Terminal;

namespace YuanTu.Test
{
    internal static class Program
    {
        /// <summary>
        ///     应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Any(p => p == "Pack"))
            {
                PackHelper.Run(args);
                return;
            }
            if (args.Any(p => p == "Main"))
            {
                var destPath = Application.StartupPath;
                var index = destPath.IndexOf("\\Tests\\", StringComparison.Ordinal);
                var srcPath = Path.Combine(destPath.Substring(0, index), "Common");

                var info = new ProcessStartInfo()
                {
                    FileName = "xcopy",
                    Arguments = $"/y /s /d \"{srcPath}\" \"{destPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                };
                var p = Process.Start(info);
                Task.Run(() =>
                {
                    try
                    {
                        var sr = p.StandardOutput;
                        while (!sr.EndOfStream)
                        {
                            Console.WriteLine(sr.ReadLine());
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
                p.WaitForExit();

                App.Main();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TestForm());
        }
    }
}