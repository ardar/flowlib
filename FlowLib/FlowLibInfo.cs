using System.Globalization;
using Flowertwig.Utils;

namespace FlowLib
{
    public class FlowLibInfo
    {
        protected static CultureInfo _cultureInfo = CultureInfo.GetCultureInfo("en-GB");
        protected static string _name = "FlowLib";
        protected static double _version;

        public static string GetName()
        {
            return _name;
        }

        public  static string GetRunningVersionString()
        {
            return string.Format("{0} v{1}", _name, GetRunningVersionNumber().ToString(_cultureInfo.NumberFormat));
        }

        public  static double GetRunningVersionNumber()
        {
            if (_version <= 0)
            {
                var info = new FlowLibInfo();
                _version = AssemblyOperations.GetRunningVersionNumber(info);
            }
            return _version;
        }
    }
}
