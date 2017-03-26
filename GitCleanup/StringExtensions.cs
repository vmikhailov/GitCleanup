using System;

namespace GitCleanup
{
    public static class StringExtensions
    {
        public static string PadRightOrLimit(this string str, int size)
        {
            return str.Substring(0, Math.Min(size, str.Length)).PadRight(size);
        }
    }
}