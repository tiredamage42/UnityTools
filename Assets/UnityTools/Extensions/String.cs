
using System.Globalization;
namespace UnityTools {
    public static class StringExtensions
    {
        const char lineBreak = '\n';
        const string lineBreakAdd = "-\n";
        
        public static string AdjustToMaxLineLength (this string input, int maxCharacters, out int lines) {
            lines = 1;

            int length = input.Length;
            string adjusted = "";
            int l = 0;
            for (int i = 0; i < length; i++) {                
                adjusted += input[i];
                l++;
                if (input[i] == lineBreak) {
                    l = 0;
                    lines++;
                }
                else {
                    if ( l == maxCharacters ) {
                        adjusted += lineBreakAdd;
                        l = 0;
                        lines++;
                    }
                }
            }
            return adjusted;
        }
        static int LineCount (this string input) {
            int lines = 1;
            int length = input.Length;
            for (int i = 0; i < length; i++) {                
                if (input[i] == lineBreak) lines++;
            }
            return lines;
        }

        public static string LargeNumberToString(this int num)
        {
            if (num > 999999999 || num < -999999999 )
                return num.ToString("0,,,.###B", CultureInfo.InvariantCulture);
            else if (num > 999999 || num < -999999 )
                return num.ToString("0,,.##M", CultureInfo.InvariantCulture);
            else if (num > 999 || num < -999)
                return num.ToString("0,.#K", CultureInfo.InvariantCulture);
            else
                return num.ToString(CultureInfo.InvariantCulture);
        }
    }
}
