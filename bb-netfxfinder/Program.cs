using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

/// <summary>
/// see https://msdn.microsoft.com/en-us/library/hh925568.aspx#net_c
/// it is completely bonkers that this even *needs* to exist in 2017
/// </summary>
namespace bb_netfxfinder
{
    class Program
    {
        static void Main(string[] args)
        {
            //since we can assume they'll have 4.0, just a quick check for 4.5+
            var net45 = NetFxVersion.Get45PlusFromRegistry();
            if(net45.Major >=4 && net45.Minor >= 5)
            {
                Console.WriteLine("Using .net 4.5 version...");
            }
            Console.WriteLine(net45);
            Console.ReadLine();
        }

    }

    public class DotNetVersionHelper
    {
        //public static bool Is45Available()
        //{

        //}
    }

    public class GetDotNetVersion
    {
        public static void GetVersionFromRegistry()
        {
            var v = new List<Version>();
            using (var ndpKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
            {
                foreach (var versionKeyName in ndpKey.GetSubKeyNames().Where(x => x.StartsWith("v")))
                {
                    var versionKey = ndpKey.OpenSubKey(versionKeyName);
                    var name = (string)versionKey.GetValue("Version", "");
                    var sp = versionKey.GetValue("SP", "").ToString();
                    var install = versionKey.GetValue("Install", "").ToString();

                    if (sp != "" && install == "1")
                    {
                        Console.WriteLine(versionKeyName + "  " + name + "  SP" + sp);
                    }
                    if (name != "")
                    {
                        continue;
                    }

                    foreach (string subKeyName in versionKey.GetSubKeyNames())
                    {
                        var subKey = versionKey.OpenSubKey(subKeyName);
                        name = (string)subKey.GetValue("Version", "");
                        if (name != "")
                            sp = subKey.GetValue("SP", "").ToString();
                        install = subKey.GetValue("Install", "").ToString();
                        if (sp != "" && install == "1")
                        {
                            Console.WriteLine(subKeyName + "  " + name + "  SP" + sp);
                        }
                        else if (install == "1")
                        {
                            Console.WriteLine(subKeyName + "  " + name);
                        }
                    }
                }
            }
        }

        

        //private static string CheckFor45PlusVersion(int releaseKey)
        //{
        //    if (releaseKey >= 394802) return "4.6.2 or later";
        //    if (releaseKey >= 394254) return "4.6.1";
        //    if (releaseKey >= 393295) return "4.6";
        //    if (releaseKey >= 379893) return "4.5.2";
        //    if (releaseKey >= 378675) return "4.5.1";
        //    if (releaseKey >= 378389) return "4.5";
        //    return "No 4.5 or later version detected";
        //}

        
    }


}