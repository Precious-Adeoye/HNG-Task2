using System.ComponentModel.DataAnnotations;

namespace HNG_Task2.Model
{
    public class StringEntity
    {
        [Key]
        public string Id { get; set; }
        public string Value { get; set; }
        public int Length { get; set; }
        public bool IsPalindrome { get; set; }
        public int UniqueCharacters { get; set; }
        public int WordCount { get; set; }
        public string Sha256Hash { get; set; }
        public string CharacterFrequencyJson { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
