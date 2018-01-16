using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Consts.Services
{
    public interface IAudioPlayer : IService
    {
        /// <summary>
        ///     开始播放
        /// </summary>
        /// <param name="fileName"></param>
        void StartPlayAsync(string fileName);

        /// <summary>
        ///     结束播放
        /// </summary>
        void StopPlayerAsync();
    }
}