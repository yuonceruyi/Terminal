using Microsoft.Practices.ServiceLocation;
using System.IO;
using Xunit;
using YuanTu.Consts.Services;

namespace UnitTest.Service.Tests.FrameworkTests
{
    public class ResourceManagerTests : TestBase
    {
        [Trait("分组", "系统")]
        [Fact]
        public void GetRes()
        {
            var config = ServiceLocator.Current.GetInstance<IResourceEngine>();
            var fl = config.GetResourceFullPath("Main");
            Assert.True(File.Exists(fl));
        }
    }
}