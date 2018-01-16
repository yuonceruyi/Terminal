using Autofac;
using YuanTu.ISO8583.Interfaces;

namespace YuanTu.ISO8583
{
    internal class ContainerWrapper : IContainerWrapper
    {
        public IContainer Container { get; set; }

        public IEncoder CreateEncoder()
        {
            return Container.Resolve<IEncoder>();
        }

        public IDecoder CreateDecoder()
        {
            return Container.Resolve<IDecoder>();
        }
    }
}