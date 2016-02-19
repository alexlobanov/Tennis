using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Tennis_Betfair.DBO
{
    public class SetIEFunc
    {
        public static void SetIE9Feature()
        {
            try
            {
                UInt32 FeatureCode = 9000; // for IE9, use 8000 for IE8 and 7000 for IE7 etc.   
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var rootKey = Registry.LocalMachine;
                var sk = rootKey.OpenSubKey(@"SOFTWARE", true);
                var mk = sk.OpenSubKey(@"Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BEHAVIORS", true);
                if (mk != null)
                {
                    var p = System.AppDomain.CurrentDomain.FriendlyName;
                    mk.SetValue(p, FeatureCode, RegistryValueKind.DWord);
                }

                var mk2 = sk.OpenSubKey(@"Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION",
                                        true);
                if (mk2 != null)
                {
                    var p = System.AppDomain.CurrentDomain.FriendlyName;
                    mk2.SetValue(p, FeatureCode, RegistryValueKind.DWord);
                }

                var m = String.Format(@"Great! Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BEHAVIORS and " +
                    "FEATURE_BROWSER_EMULATION modes went OK to mode {0}. Please restart the " +
                    " application for this to take effect.",
                    FeatureCode);
                Debug.WriteLine("Ok");

            }
            catch (Exception ex)
            {
                var m = String.Format(@"Sorry, it's a NO GO for setting Microsoft\Internet " +
                "Explorer Main FeatureControl FEATURE_BEHAVIORS and FEATURE_BROWSER_EMULATION modes. " +
                "Did you run as Administrator? {0}",
                    ex.ToString());
                Debug.WriteLine("none");
            }
        }
    }
}
