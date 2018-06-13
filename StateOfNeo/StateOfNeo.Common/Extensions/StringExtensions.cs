using System.Text.RegularExpressions;

namespace StateOfNeo.Common
{
    public static class StringExtensions
    {
        public static string ToMatchedIp(this string ipString)
        {
            Match match = Regex.Match(ipString, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            if (match.Success)
            {
                return match.Value;
            }
            return ipString;
        }
    }
}
