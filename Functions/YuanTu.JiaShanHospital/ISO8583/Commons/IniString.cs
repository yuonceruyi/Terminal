using System;

namespace YuanTu.JiaShanHospital.ISO8583.Commons
{
    public class IniString
    {
        public IniString(IniFile file, string section, string key)
        {
            _iniFile = file;
            _section = section;
            _key = key;
        }

        private readonly IniFile _iniFile;
        private readonly string _section;
        private readonly string _key;

        public string Value
        {
            get
            {
                return _iniFile.IniReadValue(_section, _key);
            }
            set
            {
                _iniFile.IniWriteValue(_section, _key, value);
                OnSet?.Invoke(this, new StringSetEventArgs(value));
            }
        }

        public event EventHandler<StringSetEventArgs> OnSet;

        public class StringSetEventArgs : EventArgs
        {
            public StringSetEventArgs(string value)
            {
                Value = value;
            }

            public string Value { get; private set; }
        }
    }
}
