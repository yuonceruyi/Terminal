using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace YuanTu.YuHangArea.ISO8583.Commons
{
    public class IniFile
    {
        public string path; //INI文件名

        /// 类的构造函数，传递INI文件的路径和文件名
        public IniFile(string INIPath)
        {
            path = INIPath;
        }

        public IniFile(string INIPath, bool local)
        {
            //path = local ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, INIPath) : INIPath;
            path = local ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, INIPath) : INIPath;
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal,
            int size, string filePath);

        //写INI文件
        public bool IniWriteValue(string Section, string Key, string Value)
        {
            return WritePrivateProfileString(Section, Key, Value, path) == 0;
        }

        //读取INI文件
        public string IniReadValue(string Section, string Key)
        {
            var temp = new StringBuilder(255);
            var i = GetPrivateProfileString(Section, Key, "", temp, 255, path);
            return temp.ToString();
        }
    }
}