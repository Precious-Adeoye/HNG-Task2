using System.Text.Json;
using System.Text.RegularExpressions;
using HNG_Task2.Data;
using HNG_Task2.Model;
using HNG_Task2.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HNG_Task2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StringsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StringsController(AppDbContext context)
        {
            _context = context;
        }

        // ----------------- 1️⃣ CREATE / ANALYZE STRING ----------------
        [HttpPost]
        public async Task<IActionResult> AnalyzeString([FromBody] StringRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Value))
                return BadRequest(new { error = "Missing 'value' field" });

            string value = request.Value;

            if (_context.Strings.Any(s => s.Value == value))
                return Conflict(new { error = "String already exist" });

            var hash = HashHelper.ComputeSha256(value);
            var freq = value.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
            var isPalindrome = StringHelper.IsPalindrome(value);
            var wordCount = StringHelper.CountWords(value);

            var entity = new StringEntity
            {
                Id = hash,
                Value = value,
                Sha256Hash = hash,
                Length = value.Length,
                IsPalindrome = isPalindrome,
                UniqueCharacters = freq.Count,
                WordCount = wordCount,
                CharacterFrequencyJson = JsonSerializer.Serialize(freq),
                CreatedAt = DateTime.UtcNow
            };

            _context.Strings.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetString), new { value = value }, new
            {
                id = entity.Sha256Hash,
                value = entity.Value,
                properties = new
                {
                    length = entity.Length,
                    is_palindrome = entity.IsPalindrome,
                    unique_characters = entity.UniqueCharacters,
                    word_count = entity.WordCount,
                    sha256_hash = entity.Sha256Hash,
                    character_frequency_map = freq
                },
                created_at = entity.CreatedAt
            });
        }

        // ----------------- 2️⃣ GET SPECIFIC STRING -----------------
        [HttpGet("{value}")]
        public IActionResult GetString(string value)
        {
            var entity = _context.Strings.FirstOrDefault(s => s.Value == value);
            if (entity == null) return NotFound(new { error = "String does not exist in the system" });

            var freq = JsonSerializer.Deserialize<Dictionary<char, int>>(entity.CharacterFrequencyJson);

            return Ok(new
            {
                id = entity.Sha256Hash,
                value = entity.Value,
                properties = new
                {
                    length = entity.Length,
                    is_palindrome = entity.IsPalindrome,
                    unique_characters = entity.UniqueCharacters,
                    word_count = entity.WordCount,
                    sha256_hash = entity.Sha256Hash,
                    character_frequency_map = freq
                },
                created_at = entity.CreatedAt
            });
        }

        // ----------------- 3️⃣ GET ALL STRINGS WITH FILTERS -----------------
        [HttpGet]
        public IActionResult GetAllStrings(
        bool? is_palindrome,
        int? min_length,
        int? max_length,
        int? word_count,
        string? contains_character
            )
        {
            var query = _context.Strings.AsQueryable();

            if (is_palindrome.HasValue)
                query = query.Where(s => s.IsPalindrome == is_palindrome.Value);
            if (min_length.HasValue)
                query = query.Where(s => s.Length >= min_length.Value);
            if (max_length.HasValue)
                query = query.Where(s => s.Length <= max_length.Value);
            if (word_count.HasValue)
                query = query.Where(s => s.WordCount == word_count.Value);
            if (!string.IsNullOrEmpty(contains_character))
                query = query.Where(s => s.Value.Contains(contains_character));

            var results = query.ToList();

            var data = results.Select(e => new
            {
                id = e.Sha256Hash,
                value = e.Value,
                properties = new
                {
                    length = e.Length,
                    is_palindrome = e.IsPalindrome,
                    unique_characters = e.UniqueCharacters,
                    word_count = e.WordCount,
                    sha256_hash = e.Sha256Hash,
                    character_frequency_map = JsonSerializer.Deserialize<Dictionary<char, int>>(e.CharacterFrequencyJson)
                },
                created_at = e.CreatedAt
            });

            return Ok(new
            {
                data,
                count = data.Count(),
                filters_applied = new
                {
                    is_palindrome,
                    min_length,
                    max_length,
                    word_count,
                    contains_character
                }
            });
        }

        // ----------------- 4️⃣ NATURAL LANGUAGE FILTER -----------------

        [HttpGet("filter-by-natural-language")]
        public IActionResult FilterByNaturalLanguage(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { error = "Query cannot be empty" });

            query = query.ToLower();
            bool? isPalindrome = null;
            int? wordCount = null;
            int? minLength = null;
            string? containsChar = null;

            if (query.Contains("palindromic")) isPalindrome = true;
            if (query.Contains("single word")) wordCount = 1;

            var matchLength = Regex.Match(query, @"longer than (\d+)");
            if (matchLength.Success) minLength = int.Parse(matchLength.Groups[1].Value);

            var matchChar = Regex.Match(query, @"letter (\w)");
            if (matchChar.Success) containsChar = matchChar.Groups[1].Value;

            var result = _context.Strings.AsQueryable();
            if (isPalindrome.HasValue)
                result = result.Where(s => s.IsPalindrome == isPalindrome.Value);
            if (wordCount.HasValue)
                result = result.Where(s => s.WordCount == wordCount.Value);
            if (minLength.HasValue)
                result = result.Where(s => s.Length > minLength.Value);
            if (!string.IsNullOrEmpty(containsChar))
                result = result.Where(s => s.Value.Contains(containsChar));

            var data = result.ToList().Select(s => new
            {
                id = s.Sha256Hash,
                value = s.Value,
                properties = new
                {
                    length = s.Length,
                    is_palindrome = s.IsPalindrome,
                    unique_characters = s.UniqueCharacters,
                    word_count = s.WordCount,
                    sha256_hash = s.Sha256Hash,
                    character_frequency_map = JsonSerializer.Deserialize<Dictionary<char, int>>(s.CharacterFrequencyJson)
                },
                created_at = s.CreatedAt
            });

            return Ok(new
            {
                data,
                count = data.Count(),
                interpreted_query = new
                {
                    original = query,
                    parsed_filters = new
                    {
                        word_count = wordCount,
                        is_palindrome = isPalindrome,
                        min_length = minLength,
                        contains_character = containsChar
                    }
                }
            });
        }

        // ----------------- 5️⃣ DELETE STRING -----------------
        [HttpDelete("{value}")]
        public async Task<IActionResult> DeleteString(string value)
        {
            var entity = _context.Strings.FirstOrDefault(s => s.Value == value);
            if (entity == null)
                return NotFound(new { error = "String does not exist in the system" });

            _context.Strings.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
