using Autofac;

namespace YuanTu.ISO8583.Interfaces
{
    public interface IContainerWrapper
    {
        IContainer Container { get; set; }
        IEncoder CreateEncoder();
        IDecoder CreateDecoder();
    }
}