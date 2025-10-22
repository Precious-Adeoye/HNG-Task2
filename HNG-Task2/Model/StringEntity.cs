using System.ComponentModel.DataAnnotations;

namespace HNG_Task2.Model
{
    public class StringEntity
    {
        [Key]
        public string Id { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int Length { get; set; }
        public bool IsPalindrome { get; set; }
        public int UniqueCharacters { get; set; }
        public int WordCount { get; set; }
        public string Sha256Hash { get; set; } = string.Empty;
        public string CharacterFrequencyJson { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
