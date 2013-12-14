using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoreLib.Modules.Helpers
{
    public static class Extensions
    {
        public static string Limit(this string text)
        {
            return Limit(text, 100);
        }

        public static string Limit(this string text, int newLength)
        {
            if (text.IsNullOrEmpty())
                return string.Empty;
            StringBuilder sb = new StringBuilder(text.Trim());
            sb.Replace("  ", " ");
            if (sb.Length > newLength)
            {
                sb.Remove(newLength, sb.Length - newLength);
                sb.Append(" ...");
            }
            return sb.ToString();
        }

        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }
    }
}
