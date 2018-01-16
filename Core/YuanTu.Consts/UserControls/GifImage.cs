using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace YuanTu.Consts.UserControls
{
    public class GifImage : Image
    {
        #region Memmbers

        private GifBitmapDecoder _gifDecoder;
        private Int32Animation _animation;
        private bool _isInitialized;

        #endregion Memmbers

        #region Properties

        private int FrameIndex
        {
            get { return (int)GetValue(FrameIndexProperty); }
            set { SetValue(FrameIndexProperty, value); }
        }

        private static readonly DependencyProperty FrameIndexProperty =
         DependencyProperty.Register("FrameIndex", typeof(int), typeof(GifImage), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(ChangingFrameIndex)));

        private static void ChangingFrameIndex(DependencyObject obj, DependencyPropertyChangedEventArgs ev)
        {
            GifImage image = obj as GifImage;
            image.Source = image._gifDecoder.Frames[(int)ev.NewValue];
        }

        /// <summary>
        /// Defines whether the animation starts on it's own
        /// </summary>
        public bool AutoStart
        {
            get { return (bool)GetValue(AutoStartProperty); }
            set { SetValue(AutoStartProperty, value); }
        }

        public static readonly DependencyProperty AutoStartProperty =
         DependencyProperty.Register("AutoStart", typeof(bool), typeof(GifImage), new UIPropertyMetadata(false, AutoStartPropertyChanged));

        private static void AutoStartPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                (sender as GifImage).StartAnimation();
        }

        public string GifSource
        {
            get { return (string)GetValue(GifSourceProperty); }
            set { SetValue(GifSourceProperty, value); }
        }

        public static readonly DependencyProperty GifSourceProperty =
         DependencyProperty.Register("GifSource", typeof(string), typeof(GifImage), new UIPropertyMetadata(string.Empty, GifSourcePropertyChanged));

        private static void GifSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            // CARLO 20100622: Reinitialize animation everytime image is changed
            (sender as GifImage).Initialize();
            (sender as GifImage).StartAnimation();
        }

        #endregion Properties

        #region Private Instance Methods

        private void Initialize()
        {

            //_gifDecoder = new GifBitmapDecoder(new Uri(String.Format("pack://application:,,,{0}", this.GifSource)), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            _gifDecoder = new GifBitmapDecoder(new Uri(this.GifSource), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            _animation = new Int32Animation(0, _gifDecoder.Frames.Count - 1, new Duration(new TimeSpan(0, 0, 0, _gifDecoder.Frames.Count / 5, (int)((_gifDecoder.Frames.Count / 5.0 - _gifDecoder.Frames.Count / 5) * 1000))));
            _animation.RepeatBehavior = RepeatBehavior.Forever;
            this.Source = _gifDecoder.Frames[0];

            _isInitialized = true;
        }

        #endregion Private Instance Methods

        #region Public Instance Methods

        /// <summary>
        /// Shows and starts the gif animation
        /// </summary>
        public void Show()
        {
            this.Visibility = Visibility.Visible;
            this.StartAnimation();
        }

        /// <summary>
        /// Hides and stops the gif animation
        /// </summary>
        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
            this.StopAnimation();
        }

        /// <summary>
        /// Starts the animation
        /// </summary>
        public void StartAnimation()
        {
            if (!_isInitialized)
                this.Initialize();

            BeginAnimation(FrameIndexProperty, _animation);
        }

        /// <summary>
        /// Stops the animation
        /// </summary>
        public void StopAnimation()
        {
            BeginAnimation(FrameIndexProperty, null);
        }

        #endregion Public Instance Methods
    }
}
