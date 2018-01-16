using Xunit;
using YuanTu.Consts;
using YuanTu.Consts.Services;

namespace UnitTest.Service.Tests.FrameworkTests
{
    public class ConfigurationTests : TestBase
    {
        [Trait("分组", "系统")]
        [Fact]
        public void TestGetValue()
        {
            var config = GetInstance<IConfigurationManager>();
            Assert.True(config.GetValue("HospitalId") == FrameworkConst.HospitalId);
        }
    }
}