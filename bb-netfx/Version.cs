using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bb_netfx
{
    public struct NetFxVersion
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Build { get; set; }
        public string Revision { get; set; }

        public NetFxVersion(string dotNotated)
        {
            var pieces = dotNotated.Split('.');
            Major = pieces.Length > 0 ? int.Parse(pieces[0]) : 0;
            Minor = pieces.Length > 1 ? int.Parse(pieces[1]) : 0;
            Build = pieces.Length > 2 ? int.Parse(pieces[2]) : 0;
            Revision = pieces.Length > 3 ? pieces[3] : "";
        }

        public NetFxVersion(int major = 0, int minor = 0, int build = 0, string rev = "")
        {
            Major = major;
            Minor = minor;
            Build = build;
            Revision = rev;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Revision))
            {
                return $"{Major}.{Minor}.{Build}";
            }
            return $"{Major}.{Minor}.{Build}.{Revision}";
        }

        private static NetFxVersion CheckFor45PlusVersion(int releaseKey)
        {
            if (releaseKey >= 394802) return new NetFxVersion(4, 6, 2);
            if (releaseKey >= 394254) return new NetFxVersion(4, 6, 1);
            if (releaseKey >= 393295) return new NetFxVersion(4, 6);
            if (releaseKey >= 379893) return new NetFxVersion(4, 5, 2);
            if (releaseKey >= 378675) return new NetFxVersion(4, 5, 1);
            if (releaseKey >= 378389) return new NetFxVersion(4, 5);
            return new NetFxVersion();
        }

        public static NetFxVersion Get45PlusFromRegistry()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    return CheckFor45PlusVersion((int)ndpKey.GetValue("Release"));
                }
                return new NetFxVersion();
            }
        }
    }
}
