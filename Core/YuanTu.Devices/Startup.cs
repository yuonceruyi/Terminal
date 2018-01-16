using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Devices
{
    public class Startup:DefaultStartup
    {
        public override void AfterStartup()
        {
            base.AfterStartup();
            var container = ServiceLocator.Current.GetInstance<IUnityContainer>();

            var deviceInterface = typeof (IDevice);
            var alltypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(p => p.GetName().Name.Contains("YuanTu.") || p.GetName().Name.Contains("Terminal"))
                .SelectMany(p => p.GetTypes()).Where(p=>deviceInterface.IsAssignableFrom(p)&&p!=deviceInterface).ToArray();
            var interfaces = alltypes.Where(p => p.IsInterface);
            foreach (var @interface in interfaces)
            {
                var objs = alltypes.Where(p => @interface.IsAssignableFrom(p) & !p.IsAbstract).ToArray();
                container.RegisterTypes(objs, WithMappings.FromAllInterfaces, WithName.TypeName,WithLifetime.ContainerControlled);
            }

        }
    }
}
