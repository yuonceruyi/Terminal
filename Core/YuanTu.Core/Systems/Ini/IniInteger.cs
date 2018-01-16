using System;

namespace YuanTu.Core.Systems.Ini
{
    public class IniInteger
    {
        private readonly IniFile _iniFile;
        private readonly string _key;
        private readonly string _section;

        public IniInteger(IniFile file, string section, string key)
        {
            _iniFile = file;
            _section = section;
            _key = key;
        }

        public int Value
        {
            get
            {
                var s = _iniFile.IniReadValue(_section, _key);
                if (string.IsNullOrEmpty(s))
                {
                    Value = 0;
                    return 0;
                }
                return Convert.ToInt32(s);
            }
            set
            {
                _iniFile.IniWriteValue(_section, _key, value.ToString());
                OnSet?.Invoke(this, new IntegerSetEventArgs(value));
            }
        }

        public event EventHandler<IntegerSetEventArgs> OnSet;

        public class IntegerSetEventArgs : EventArgs
        {
            public IntegerSetEventArgs(int value)
            {
                Value = value;
            }

            public int Value { get; private set; }
        }
    }
}