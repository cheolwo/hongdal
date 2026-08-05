using System;
using System.Text.RegularExpressions;

namespace Ssalddel.Unity.Data
{
    public static class StableDataId
    {
        private static readonly Regex Pattern = new Regex(
            "^[a-z0-9]+(?:[.-][a-z0-9]+)*(?::[a-z0-9]+(?:[.-][a-z0-9]+)*)+$",
            RegexOptions.CultureInvariant);

        public static bool IsValid(string? value)
        {
            return !string.IsNullOrWhiteSpace(value) && Pattern.IsMatch(value);
        }

        public static void EnsureValid(string value, string parameterName)
        {
            if (!IsValid(value))
            {
                throw new ArgumentException(
                    "Stable ID는 소문자 영숫자 segment를 colon으로 연결해야 합니다.",
                    parameterName);
            }
        }
    }
}
