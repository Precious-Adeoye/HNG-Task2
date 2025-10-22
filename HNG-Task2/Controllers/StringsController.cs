using System.Text.Json;
using System.Text.RegularExpressions;
using HNG_Task2.IStringServices;
using HNG_Task2.Model;
using HNG_Task2.Utility;
using Microsoft.AspNetCore.Mvc;

namespace HNG_Task2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StringsController : ControllerBase
    {
        private readonly IStringStorage _storage;

        public StringsController(IStringStorage storage)
        {
            _storage = storage;
        }

        // POST: api/strings
        [HttpPost]
        public async Task<IActionResult> AnalyzeString([FromBody] StringRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Value))
                return BadRequest(new { error = "Invalid request body or missing 'value' field" });

            if (!request.Value.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))
                return UnprocessableEntity(new { error = "Invalid data type for 'value' (must be string)" });

            string value = request.Value;
            string hash = HashHelper.ComputeSha256(value);

            if (await _storage.ExistsAsync(value))
                return Conflict(new { error = "String already exists in the system" });

            var freq = value.ToLower().GroupBy(c => c).ToDictionary(g => g.Key.ToString(), g => g.Count());
            var entity = new StringEntity
            {
                Id = hash,
                Value = value,
                Sha256Hash = hash,
                Length = value.Length,
                IsPalindrome = StringHelper.IsPalindrome(value),
                UniqueCharacters = value.ToLower().Distinct().Count(c => char.IsLetterOrDigit(c) || c == ' '),
                WordCount = StringHelper.CountWords(value),
                CharacterFrequencyJson = JsonSerializer.Serialize(freq, new JsonSerializerOptions { WriteIndented = true }),
                CreatedAt = DateTime.UtcNow
            };

            await _storage.AddAsync(entity);
            return CreatedAtAction(nameof(GetString), new { value }, new
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
                created_at = entity.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            });
        }

        // GET: api/strings/{value}
        [HttpGet("{value}")]
        public async Task<IActionResult> GetString(string value)
        {
            var entity = await _storage.GetByValueAsync(value);
            if (entity == null)
                return NotFound(new { error = "String does not exist in the system" });

            var freq = JsonSerializer.Deserialize<Dictionary<string, int>>(entity.CharacterFrequencyJson);
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
                created_at = entity.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            });
        }

        // GET: api/strings
        [HttpGet]
        public async Task<IActionResult> GetAllStrings(
            [FromQuery] bool? is_palindrome,
            [FromQuery] int? min_length,
            [FromQuery] int? max_length,
            [FromQuery] int? word_count,
            [FromQuery] string? contains_character)
        {
            try
            {
                var entities = await _storage.GetFilteredAsync(is_palindrome, min_length, max_length, word_count, contains_character);
                var data = entities.Select(e => new
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
                        character_frequency_map = JsonSerializer.Deserialize<Dictionary<string, int>>(e.CharacterFrequencyJson)
                    },
                    created_at = e.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
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
            catch
            {
                return BadRequest(new { error = "Invalid query parameter values or types" });
            }
        }

        // GET: api/strings/filter-by-natural-language
        [HttpGet("filter-by-natural-language")]
        public async Task<IActionResult> FilterByNaturalLanguage(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { error = "Query cannot be empty" });

            query = query.ToLower();
            bool? isPalindrome = null;
            int? wordCount = null;
            int? minLength = null;
            string? containsChar = null;

            try
            {
                if (query.Contains("single word") && query.Contains("palindromic"))
                {
                    wordCount = 1;
                    isPalindrome = true;
                }
                else if (query.Contains("palindromic") && query.Contains("first vowel"))
                {
                    isPalindrome = true;
                    containsChar = "a";
                }
                else
                {
                    if (query.Contains("palindromic")) isPalindrome = true;
                    if (query.Contains("single word")) wordCount = 1;

                    var matchLength = Regex.Match(query, @"longer than (\d+)");
                    if (matchLength.Success) minLength = int.Parse(matchLength.Groups[1].Value) + 1;

                    var matchChar = Regex.Match(query, @"letter (\w)");
                    if (matchChar.Success) containsChar = matchChar.Groups[1].Value;
                }

                if (isPalindrome == null && wordCount == null && minLength == null && containsChar == null)
                    return BadRequest(new { error = "Unable to parse natural language query" });

                var entities = await _storage.GetFilteredAsync(isPalindrome, minLength, null, wordCount, containsChar);
                var data = entities.Select(s => new
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
                        character_frequency_map = JsonSerializer.Deserialize<Dictionary<string, int>>(s.CharacterFrequencyJson)
                    },
                    created_at = s.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
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
            catch
            {
                return UnprocessableEntity(new { error = "Query parsed but resulted in conflicting filters" });
            }
        }

        // DELETE: api/strings/{value}
        [HttpDelete("{value}")]
        public async Task<IActionResult> DeleteString(string value)
        {
            var entity = await _storage.GetByValueAsync(value);
            if (entity == null)
                return NotFound(new { error = "String does not exist in the system" });

            await _storage.DeleteAsync(value);
            return NoContent();
        }
    }
}