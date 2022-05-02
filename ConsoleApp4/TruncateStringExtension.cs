using System;

namespace ConsoleApp4
{
    public static class TruncateStringExtension
    {
        public static string Truncate(this string s, int length)
        {
            // Cannot use .Substring(), because it expects full length :(
            return s.Substring(0, Math.Min(s.Length, length));
        }
    }
}