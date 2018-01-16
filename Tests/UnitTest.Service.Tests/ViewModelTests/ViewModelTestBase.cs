using Xunit;
using YuanTu.Consts.FrameworkBase;

namespace UnitTest.Service.Tests.ViewModelTests
{
    public class ViewModelTestBase : TestBase
    {
        [Trait("分组", "挂号")]
        [Fact]
        public void LaunchMain()
        {
            var main = GetInstance<IShellViewModel>();
        }
    }
}