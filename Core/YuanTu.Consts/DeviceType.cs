using System.Collections.Generic;

namespace YuanTu.Consts
{
    public class DeviceType
    {
        public const string Default = "";
        public const string Clinic = "Clinic";
        public const string HAtm = "HAtm";
        public const string Tablet = "Tablet";
        public const string House = "House";

        public static readonly string[] DeviceTypes = {Clinic, HAtm, House, Tablet };
        public static readonly string[] DeviceTypeStrings = { $".{Clinic}.", $".{HAtm}.",$".{House}", $".{Tablet}." };

        public static Dictionary<string, string[]> FallBackToDefaultStrategy => new Dictionary<string, string[]>
        {
            ["YT-550"] = new[] { Default },
            ["YT-560"] = new[] { Default },
            ["YT-540"] = new[] { Default },
            ["ZJ-350"] = new[] { Clinic, Default },
            ["YT-JK630"] = new[] { House, Default },
            ["YT-JK750"] = new[] { House, Default },
            ["YT-740"] = new[] { HAtm, Default },
            ["YT-CK350"] = new[] { Tablet, Default },
        };

        public static Dictionary<string, string[]> NoFallBackStrategy => new Dictionary<string, string[]>
        {
            ["YT-550"] = new[] { Default },
            ["YT-560"] = new[] { Default },
            ["YT-540"] = new[] { Default },
            ["ZJ-350"] = new[] { Clinic },
            ["YT-JK630"] = new[] { House },
            ["YT-JK750"] = new[] { House },
            ["YT-740"] = new[] { HAtm },
            ["YT-CK350"] = new[] { Tablet },
        };

        public static string GetDeviceFromTypeFullName(string fullName)
        {
            for (int i = 0; i < DeviceTypes.Length; i++)
            {
                if (fullName.Contains(DeviceTypeStrings[i]))
                    return DeviceTypes[i];
            }
            return Default;
        }
    }
}