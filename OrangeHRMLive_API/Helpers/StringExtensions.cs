using System.Text.RegularExpressions;

namespace OrangeHRMLive_API.Helpers
{
    public static class StringExtensions
    {
        /// <summary>
        /// Tham khảo https://www.c-sharpcorner.com/article/c-sharp-regex-examples/
        /// </summary>
        /// <param name="content"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        public static string GetStringByRegex(this string content, string pattern)
        {
            // Create a Regex
            Regex rg = new Regex(pattern);
            var match = rg.Match(content);
            if (match.Success)
            {
                return match.Groups[1].ToString();
            }

            return null;
        }
    }
}
