using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hcc
{
    public static class HccUtil
    {
        public static string url_join(string part1, string part2)
        {
            string usePart1 = part1.Replace("\\","/");
            string usePart2 = part2.Replace("\\", "/");

            if ( !usePart1.EndsWith("/",StringComparison.CurrentCulture))
                usePart1 += "/";

            if (usePart2.StartsWith("/", StringComparison.CurrentCulture))
                usePart2 = usePart2.Substring(1);

            return usePart1 + usePart2;
        }
    }
}
