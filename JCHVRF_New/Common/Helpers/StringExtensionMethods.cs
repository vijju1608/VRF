using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JCHVRF_New.Common.Helpers
{
    public static class StringExtensionMethods
    {
        public static bool HasSpecialCharacters(this String str)
        {
            return !Regex.IsMatch(str, "^[ \\w\\.\\#\\-]*$");
        }

        public static bool HasOnlyZero(this string str)
        {
            return !Regex.IsMatch(str,"^[0]*$");
        }               
    }
}
