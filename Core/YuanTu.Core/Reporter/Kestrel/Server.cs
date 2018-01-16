using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace YuanTu.Core.Reporter.Kestrel
{
    //class Server
    //{
    //    protected string rootPath;
    //    protected string exeName = "FileServer";
    //    protected string zipName = "FileServer.zip";
    //    protected string logName = "FileServer.TimeStamp";

    //    public Server()
    //    {
    //        rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "External", "FileServer");
    //    }

    //    public void Start()
    //    {
    //        try
    //        {
    //            if (Process.GetProcesses().Any(p => p.ProcessName.Equals(exeName, StringComparison.OrdinalIgnoreCase)))
    //                return;
    //            EnsureExe();
    //            EnsureProcess();
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.Main.Warn($"文件服务器启动失败:{ex.Message}\n{ex.StackTrace}");
    //        }
    //    }

    //    protected DateTime GetTimeStamp()
    //    {
    //        var path = Path.Combine(rootPath, logName);
    //        if (!File.Exists(path))
    //            return new DateTime();
    //        var text = File.ReadAllText(path);
    //        DateTime timeStamp;
    //        if(!DateTime.TryParse(text,out timeStamp))
    //            return new DateTime();
    //        return timeStamp;
    //    }

    //    protected void SetTimeStamp(DateTime timeStamp)
    //    {
    //        var path = Path.Combine(rootPath, logName);
    //        File.WriteAllText(path, timeStamp.ToString("yyyy-MM-dd HH:mm:ss"));
    //    }

    //    protected void EnsureDirectory(string path)
    //    {
    //        var directory = Path.GetDirectoryName(path);
    //        if (string.IsNullOrEmpty(directory))
    //            return;
    //        if (!Directory.Exists(directory))
    //            Directory.CreateDirectory(directory);
    //    }

    //    protected void EnsureExe()
    //    {
    //        var zipFile = new FileInfo(Path.Combine(rootPath, zipName));
    //        if (!zipFile.Exists)
    //            return;
    //        var time = zipFile.LastWriteTime;
    //        if (time == GetTimeStamp())
    //            return;
    //        using (var fs = zipFile.OpenRead())
    //        using (var za = new ZipArchive(fs, ZipArchiveMode.Read))
    //            foreach (var entry in za.Entries)
    //            {
    //                var fullPath = Path.Combine(rootPath, entry.FullName);
    //                EnsureDirectory(fullPath);
    //                using (var f = File.OpenWrite(fullPath))
    //                using (var zf = entry.Open())
    //                    zf.CopyTo(f);
    //            }
    //        SetTimeStamp(time);
    //    }

    //    protected void EnsureProcess()
    //    {
    //        var exePath = Path.Combine(rootPath, $"{exeName}.exe");
    //        if (!File.Exists(exePath))
    //            return;
    //        try
    //        {
    //            Process.Start(new ProcessStartInfo()
    //            {
    //                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
    //                FileName = exePath,
    //                WindowStyle = ProcessWindowStyle.Hidden
    //            });
    //        }
    //        catch (Exception)
    //        {
    //            //
    //        }
    //    }
    //}
    public class Server : IDisposable
    {
        private static readonly List<Type> Middlewares = new List<Type>();
        private readonly IWebHost _host;

        public Server()
        {
            CoreFileCopy();
            var host = new WebHostBuilder()
                .UseWebRoot(AppDomain.CurrentDomain.BaseDirectory)
                .UseStartup<Startup>()
                .UseKestrel()
                .UseUrls("http://*:9090")
                .Build();
            _host = host;
        }

        public void Dispose()
        {
            _host.Dispose();
        }

        public void Start()
        {
            _host.Start();
        }

        private void CoreFileCopy()
        {
            var in64Bit = IntPtr.Size == 8;
            
            var srcPath = $"External\\{(in64Bit ? "libuv_x64.dll" : "libuv_x86.dll")}";
            var srcInfo = new FileInfo(srcPath);
            var destInfo = new FileInfo("libuv.dll");
            if (!destInfo.Exists || destInfo.Length != srcInfo.Length)
                File.Copy(srcInfo.FullName, destInfo.FullName, true);
        }

        public static void AddMiddlewares(params Type[] types)
        {
            Middlewares.AddRange(types);
        }

        internal class Startup : IStartup
        {
            //public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            //{
            //    app.UseStaticFiles(new StaticFileOptions
            //    {
            //        ServeUnknownFileTypes = true
            //    });
            //}

            public IServiceProvider ConfigureServices(IServiceCollection services)
            {
                services.AddDirectoryBrowser();
                return services.BuildServiceProvider();
            }

            public void Configure(IApplicationBuilder app)
            {
                foreach (var middleware in Middlewares)
                    app.UseMiddleware(middleware);

                app.UseFileServer(new FileServerOptions
                {
                    FileProvider = new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory),
                    EnableDirectoryBrowsing = true,
                    StaticFileOptions =
                    {
                        DefaultContentType = "application/x-msdownload",
                        ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
                        {
                            {".config", "text/plain"},
                            {".jpg", "image/jpeg"},
                            {".png", "image/png"},
                            {".mp4", "video/mp4"}
                        })
                    }
                });
            }
        }
    }
}