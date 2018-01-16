namespace YuanTu.AutoUpdater
{
    public interface IAutoUpdater
    {
        bool Check();
        void Update();
        void RollBack();
    }
}