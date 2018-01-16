using System;
using System.Reflection;

namespace YuanTu.Core.Infrastructure
{
    internal class AssemblyInfo
    {
        public Assembly Assembly { get; set; }

        public Type[] Types { get; set; }

        public override string ToString()
        {
            return $"{Assembly.FullName} #{Types.Length}";
        }
    }
}