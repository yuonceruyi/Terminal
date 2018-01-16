using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace YuanTu.Consts.UserControls
{
   public static  class PictureBoxUtility
    {
        public static readonly DependencyProperty BindableSourceProperty =
       DependencyProperty.RegisterAttached("BindableSource", typeof(string), typeof(PictureBoxUtility), new UIPropertyMetadata(null, BindableSourcePropertyChanged));

        public static string GetBindableSource(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableSourceProperty);
        }

        public static void SetBindableSource(DependencyObject obj, string value)
        {
            obj.SetValue(BindableSourceProperty, value);
        }

        public static void BindableSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var windowsFormsHost = o as WindowsFormsHost;
                    var pictureBox = windowsFormsHost?.Child as PictureBox;
                    if (pictureBox == null) continue;
                    var uri = e.NewValue as string;
                    pictureBox.ImageLocation = !string.IsNullOrEmpty(uri) ? uri : null;
                    break;
                }
            });
           
          
        }

    }
}
