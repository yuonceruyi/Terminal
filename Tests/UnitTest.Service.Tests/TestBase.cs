using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Core.Infrastructure;

namespace UnitTest.Service.Tests
{
    public abstract class TestBase
    {
        public static IUnityContainer UnityContainer;

        static TestBase()
        {
            FrameworkConst.UnitTest = true;
            var destPath = AppDomain.CurrentDomain.BaseDirectory;
            var index = destPath.IndexOf("\\Tests\\", StringComparison.Ordinal);
            var srcPath = Path.Combine(destPath.Substring(0, index), "Common");

            var p = Process.Start("xcopy", $"/y /s /d \"{srcPath}\" \"{destPath}\"");
            p.WaitForExit();

            var bootstrapper = new Terminal.Bootstrapper();
            UnityContainer = new UnityContainer();
            var locator = new UnityServiceLocator(UnityContainer);
            ServiceLocator.SetLocatorProvider(() => locator);
            SystemStartup.Initialize(UnityContainer, "");
        }

        public T GetInstance<T>()
        {
            return ServiceLocator.Current.GetInstance<T>();
        }
    }
}