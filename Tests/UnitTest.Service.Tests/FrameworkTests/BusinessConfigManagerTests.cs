using Microsoft.Practices.ServiceLocation;
using Xunit;
using YuanTu.Consts.Services;

namespace UnitTest.Service.Tests.FrameworkTests
{
    public class BusinessConfigManagerTests : TestBase
    {
        [Trait("分组", "系统")]
        [Fact]
        public void TestGetFlowId()
        {
            var config = ServiceLocator.Current.GetInstance<IBusinessConfigManager>();
            var flow1 = config.GetFlowId("单元测试");
            var flow2 = config.GetFlowId("单元测试");
            var trim1 = flow1.Substring(flow1.Length - 6, 6);
            var trim2 = flow2.Substring(flow2.Length - 6, 6);
            Assert.True(int.Parse(trim2) > int.Parse(trim1), $"第一次申请:{flow1} 第二次申请:{flow1}");
        }
    }
}