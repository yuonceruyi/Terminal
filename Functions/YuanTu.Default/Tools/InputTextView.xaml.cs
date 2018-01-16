using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Systems.Ini;

namespace YuanTu.Default.Tools
{
    /// <summary>
    /// InputTextView.xaml 的交互逻辑
    /// </summary>
    
    public partial class InputTextView : Window
    {
        private InputTextView()
        {
            InitializeComponent();
        }

        private string _content;

        public static Result<string> ShowDialogView(string title)
        {
            var sm = ServiceLocator.Current.GetInstance<IShellViewModel>();
            var k = LoadFromCash();
            var dialog = new InputTextView
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = ServiceLocator.Current.GetInstance<IShell>() as Window
            };
            int val = 0;
            while (val++<3)
            {
                try
                {
                    dialog.Content.Clear();
                    dialog.Content.Text = k.Item2;
                    dialog.Title.Text = title;

                    sm.TimeOutStop = true;
                    if (dialog.ShowDialog() ?? false)
                    {
                        return Result<string>.Success(dialog._content);
                    }

                    return Result<string>.Fail("");
                }
                catch (Exception ex)
                {
                    continue;
                   // 
                }
                finally
                {
                    UpdateCash(k.Item1, dialog._content);
                    sm.TimeOutSeconds = sm.TimeOutSeconds;
                }
            }
            return Result<string>.Fail("");

        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            _content =Content.Text;
            DialogResult = true;
        }

        private void ButtonCalcel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private static Tuple<string,string> LoadFromCash()
        {
            var fr = new StackFrame(2, true);
            var mb = fr.GetMethod();
            var fullName = mb.DeclaringType.FullName + "." + mb.Name;
            var tmp = IniFile.IniReadValue("Test", fullName);
            if (tmp.IsNullOrWhiteSpace())
            {
                return new Tuple<string, string>(fullName,null);
            }
            foreach (var kv in _special)
            {
                tmp = tmp.Replace(kv.Value, kv.Key);
            }
            return new Tuple<string, string>(fullName, tmp);
        }

        private static void UpdateCash(string key, string val)
        {
            if (string.IsNullOrWhiteSpace(key)||string.IsNullOrWhiteSpace(val))
            {
                return;
            }
            var tmp = val;
            foreach (var kv in _special)
            {
                tmp = tmp.Replace(kv.Key, kv.Value);
            }
            IniFile.IniWriteValue("Test", key, tmp);
        }

        private static readonly IniFile IniFile=new IniFile("TestData.ini",true);
        private static Dictionary<string, string>_special=new Dictionary<string, string>
        {
            ["\r"]="&newline",
            ["\n"]="&enter",
            ["="]="&equal"
        }; 
    }
}
