using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace YuanTu.Test
{
    internal class PackHelper
    {
        private const string xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";
        private const string rar = @"C:\Program Files\WinRAR\rar";

        private static long _count;
        private static long Count => _count++;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">Configuration=Debug PublishRoot=</param>
        public static void Run(string[] args)
        {
            var id = Count;
            Log(id, "Begin");
            var sw = Stopwatch.StartNew();
            try
            {
                var configuration = ParseCmd(args, "Configuration", "Debug");
                var index = Application.StartupPath.LastIndexOf("Tests", StringComparison.Ordinal);
                var rootPath = Application.StartupPath.Substring(0, index);

                var publishRoot = ParseCmd(args, "PublishRoot", Path.Combine(rootPath, "Publish"));
                if (!Directory.Exists(publishRoot))
                    Directory.CreateDirectory(publishRoot);
                var publishPath = Path.Combine(publishRoot, configuration);
                if (Directory.Exists(publishPath))
                    Directory.Delete(publishPath, true);
                Directory.CreateDirectory(publishPath);

                Log(id, $"Parse T:{sw.ElapsedMilliseconds}ms");
                var projectFilePaths = Directory.EnumerateFiles(rootPath, "*.csproj", SearchOption.AllDirectories).ToList();
                var packInfos = new List<PackInfo>();
                foreach (var projectFilePath in projectFilePaths
                    .Where(s => s.Contains(@"\Functions\")
                                && !s.Contains("YuanTu.Default")))
                {
                    var outputPath = GetOutputPath(configuration, projectFilePath);
                    if (string.IsNullOrEmpty(outputPath))
                    {
                        Log(id, $"Can't find OutputPath for {configuration}: {projectFilePath}");
                        continue;
                    }
                    if (!Directory.Exists(outputPath))
                    {
                        Log(id, $"Can't find OutputPathDirectory for {configuration}: {projectFilePath}");
                        continue;
                    }
                    var packInfo = new PackInfo
                    {
                        OutputPath = outputPath,
                        PackPath = Path.Combine(publishPath, Path.GetFileNameWithoutExtension(projectFilePath))
                    };
                    packInfos.Add(packInfo);
                }
                Log(id, $"Parse Done T:{sw.ElapsedMilliseconds}ms");
                var common = new List<string>
                {
                    "Microsoft.*.dll",
                    "System.*.dll",
                    "Vlc.*.dll",
                    "ID003ProtocolManager.dll",
                    "log4net.dll",
                    "Newtonsoft.Json.dll",
                    "Prism.dll",
                    "Prism.Unity.Wpf.dll",
                    "Prism.Wpf.dll",
                    "zxing.dll",
                    "CameraLib.dll",
                };
                var share = new List<string>
                {
                    "Config.xml",
                    "Terminal.exe",
                    "Terminal.exe.config",
                    "YuanTu.AutoUpdater.dll",
                    "YuanTu.Consts.dll",
                    "YuanTu.Core.dll",
                    "YuanTu.Default.dll",
                    "YuanTu.Default.Theme.dll",
                    "YuanTu.Devices.dll",
                };
                var excludes = Enumerable.Empty<string>()
                    .Concat(common)
                    .Concat(share)
                    .ToList();

                var commonPackPath = Path.Combine(publishPath, "Common");
                Pack(Path.Combine(rootPath, "Common"), new List<string>
                {
                    "a",
                    "-r",
                    $"\"{commonPackPath}.rar\"",
                    "External\\*",
                    "Resource\\*"
                });
                var terminalProjectFilePath = projectFilePaths.First(s => s.EndsWith("Terminal.csproj"));
                var terminalOutputPath = GetOutputPath(configuration, terminalProjectFilePath);
                Pack(terminalOutputPath, new List<string>
                {
                    "a",
                    "-r",
                    $"\"{commonPackPath}.rar\""
                }.Concat(common));
                Log(id, $"Common Done T:{sw.ElapsedMilliseconds}ms");

                foreach (var packInfo in packInfos)
                {
                    Pack(packInfo.OutputPath, new List<string>
                    {
                        "a",
                        "-r",
                        $"\"{packInfo.PackPath}.rar\"",
                        "External\\*",
                        "Resource\\*",
                        "CurrentResource\\*",
                        "*.dll",
                        "*.exe"
                    }.Concat(excludes.Select(e => $"-x{e}")));

                    Pack(terminalOutputPath, new List<string>
                    {
                        "a",
                        $"\"{packInfo.PackPath}.rar\"",
                    }.Concat(share));
                }
                Log(id, $"Finish T:{sw.ElapsedMilliseconds}ms");
            }
            catch (Exception e)
            {
                Log(id, $"Exception T:{sw.ElapsedMilliseconds}ms {e}");
                throw;
            }
        }

        private static void Log(long id, string message, [CallerMemberName] string method = null)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}][{id}][{method}] {message}");
        }

        private static void Pack(string workingDirectory, IEnumerable<string> arguments)
        {
            var sw = Stopwatch.StartNew();
            var id = Count;
            var argumentsString = string.Join(" ", arguments);
            Log(id, $"WD:{workingDirectory} A:{argumentsString}");
            var mre = new ManualResetEvent(false);
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = rar,
                Arguments = argumentsString,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            Task.Run(() =>
            {
                try
                {
                    var reader = p.StandardOutput;
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        Console.WriteLine(line);
                    }
                }
                finally
                {
                    mre.Set();
                }
            });
            p.WaitForExit();
            sw.Stop();
            mre.WaitOne();
            Log(id, $"R:{p.ExitCode} T:{sw.ElapsedMilliseconds}ms");
        }

        private static string GetOutputPath(string configuration, string projectFilePath)
        {
            XDocument doc;
            using (var s = File.OpenRead(projectFilePath))
            {
                doc = XDocument.Load(s);
            }
            string outputPath = null;
            foreach (var element in doc.Descendants(XName.Get("PropertyGroup", xmlns)))
            {
                var attribute = element.Attribute("Condition");
                if (attribute == null)
                    continue;
                if (attribute.Value != $" '$(Configuration)|$(Platform)' == '{configuration}|AnyCPU' ")
                    continue;
                var node = element.Descendants(XName.Get("OutputPath", xmlns)).FirstOrDefault();
                outputPath = Path.Combine(Path.GetDirectoryName(projectFilePath), node.Value);
            }

            return outputPath;
        }

        public static void ParseCmd(string[] args, string name, ref string value)
        {
            var prefix = $"{name}=";
            var argName = args.FirstOrDefault(one => one.StartsWith(prefix));
            if (!string.IsNullOrEmpty(argName))
                value = argName.Substring(prefix.Length);
        }

        public static string ParseCmd(string[] args, string name, string defaultValue)
        {
            ParseCmd(args, name, ref defaultValue);
            return defaultValue;
        }

        private class PackInfo
        {
            public string OutputPath { get; set; }
            public string PackPath { get; set; }
        }
    }
}