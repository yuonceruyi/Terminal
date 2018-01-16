using System.Windows.Input;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Component.ViewModels;

namespace YuanTu.Default.Component.Views
{
    /// <summary>
    ///     InfoMatrixTest.xaml 的交互逻辑
    /// </summary>
    public partial class InfoMatrixTestView : ViewsBase
    {
        public InfoMatrixTestView()
        {
            InitializeComponent();
        }

        private async void ViewsBase_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var context = this.DataContext as InfoMatrixTestViewModel;
            if(context == null)return;
            switch (e.Key)
            {
                case Key.D1: await context.Info.Execute(); break;
                case Key.D2: await context.InfoIcon.Execute(); break;
                case Key.D3: await context.InfoMore.Execute(); break;
                case Key.D4: await context.InfoType.Execute(); break;
                case Key.Q: await context.ColumnPlus.Execute();break;
                case Key.W: await context.ColumnMinus.Execute(); break;
                case Key.E: await context.RowPlus.Execute(); break;
                case Key.R: await context.RowMinus.Execute(); break;
            }
        }
    }
}