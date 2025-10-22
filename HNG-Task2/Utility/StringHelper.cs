using System.Text.RegularExpressions;

namespace HNG_Task2.Utility
{
    public static class StringHelper
    {
        public static bool IsPalindrome(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            var cleaned = Regex.Replace(value.ToLowerInvariant(), "[^a-z0-9]", "");
            return cleaned.SequenceEqual(cleaned.Reverse());
        }

        public static int GetUniqueCharacterCount(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            return value.ToLowerInvariant().Distinct().Count(c => !char.IsWhiteSpace(c));
        }

        public static int GetWordCount(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            return Regex.Split(value.Trim(), @"\s+").Length;
        }

        public static Dictionary<string, int> GetCharacterFrequency(string value)
        {
            if (string.IsNullOrEmpty(value)) return new Dictionary<string, int>();
            return value.ToLowerInvariant()
                .GroupBy(c => c)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());
        }
    }
}