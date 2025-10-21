namespace HNG_Task2.Utility
{
    public class StringHelper
    {
        public static bool IsPalindrome(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return false;

            var cleaned = new string(s.ToLower().Where(char.IsLetterOrDigit).ToArray());
            return cleaned.SequenceEqual(cleaned.Reverse());
        }

        public static int CountWords(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return 0;

            return s.Split((char[])null!, StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }
}
