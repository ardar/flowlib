namespace FlowLib.Connections.Protocols.Nmdc
{
    public static class Dummy
    {
        public static string ConvertToNmdc(string str)
        {
            str = str.Replace("&", "&amp;");
            str = str.Replace("&#124;", "&#36;");
            str = str.Replace("$", "&#124;");
            return str;
        }

        public static string ConvertFromNmdc(string str)
        {
            str = str.Replace("&#124;", "$");
            str = str.Replace("&#36;", "&#124;");
            str = str.Replace("&amp;", "&");
            return str;
        }
    }
}