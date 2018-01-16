using System.Windows.Controls;
using YuanTu.Consts.Models;

namespace YuanTu.Consts.FrameworkBase
{
    public interface IShell : IDependency
    {
        /// <summary>
        ///     用于提供全局遮罩
        /// </summary>
        Grid Mask { get; }

        bool IsTransitioning { get; }
    }

    public interface IShellViewModel : IDependency
    {
        BusyModel Busy { get; set; }
        AlertModel Alert { get; set; }
        ConfirmModel Confirm { get; set; }
        MaskModel Mask { get; set; }
        bool ShowNavigating { get; set; }
        int TimeOutSeconds { get; set; }
        bool TimeOutStop { get; set; }

        void OnViewInit();
    }
}