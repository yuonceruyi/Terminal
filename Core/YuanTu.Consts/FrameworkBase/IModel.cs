using System.ComponentModel;
using Prism.Mvvm;

namespace YuanTu.Consts.FrameworkBase
{
    public interface IModel : INotifyPropertyChanged, IDependency
    {
    }

    public abstract class ModelBase : BindableBase, IModel
    {
    }
}
