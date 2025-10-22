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
            if (request == null || string.IsNullOrWhiteSpace(request.Value))
            {
                return UnprocessableEntity(new { error = "Value is required and must be a non-empty string" });
            }

            if (!Regex.IsMatch(request.Value, @"^[\p{L}\p{N}\s]+$"))
            {
                return UnprocessableEntity(new { error = "Value must contain only letters, numbers, or spaces" });
            }

            if (await _storage.ExistsAsync(request.Value))
            {
                return Conflict(new { error = "String already exists" });
            }

            var entity = new StringEntity
            {
                Id = HashHelper.ComputeSha256(request.Value),
                Value = request.Value,
                Length = request.Value.Length,
                IsPalindrome = StringHelper.IsPalindrome(request.Value),
                UniqueCharacters = StringHelper.GetUniqueCharacterCount(request.Value),
                WordCount = StringHelper.GetWordCount(request.Value),
                Sha256Hash = HashHelper.ComputeSha256(request.Value),
                CharacterFrequencyMap = StringHelper.GetCharacterFrequency(request.Value),
                CreatedAt = DateTime.UtcNow
            };

            await _storage.AddAsync(entity);
            return CreatedAtAction(nameof(GetByValue), new { value = request.Value }, entity);
        }

        // GET: api/strings/{value}
        [HttpGet("{value}")]
        public async Task<IActionResult> GetByValue(string value)
        {
            var entity = await _storage.GetByValueAsync(value);
            if (entity == null)
            {
                return NotFound(new { error = "String not found" });
            }
            return Ok(entity);
        }

        // GET: api/strings
        [HttpGet]
        public async Task<IActionResult> GetAllStrings(
            [FromQuery] bool? isPalindrome,
            [FromQuery] int? minLength,
            [FromQuery] int? maxLength,
            [FromQuery] int? wordCount,
            [FromQuery] string? containsChar)
        {
           var entities = await _storage.GetFilteredAsync(isPalindrome, minLength, maxLength, wordCount, containsChar);
            return Ok(new
            {
                data = entities,
                count = entities.Count,
                filters_applied = new
                {
                    is_palindrome = isPalindrome,
                    min_length = minLength,
                    max_length = maxLength,
                    word_count = wordCount,
                    contains_character = containsChar
                }
            });
        }

        // GET: api/strings/filter-by-natural-language
        [HttpGet("filter-by-natural-language")]
        public async Task<IActionResult> FilterByNaturalLanguage([FromQuery]string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { error = "Query parameter is required" });
            }

            var parsedFilters = ParseNaturalLanguageQuery(query);
            var entities = await _storage.GetFilteredAsync(
                parsedFilters.is_palindrome,
                parsedFilters.min_length,
                null, // max_length not parsed
                parsedFilters.word_count,
                null // contains_character not parsed
            );

            return Ok(new
            {
                data = entities,
                count = entities.Count,
                interpreted_query = new
                {
                    original = query,
                    parsed_filters = parsedFilters
                }
            });
        }

        // DELETE: api/strings/{value}
        [HttpDelete("{value}")]
        public async Task<IActionResult> DeleteString(string value)
        {
            if (await _storage.ExistsAsync(value))
            {
                await _storage.DeleteAsync(value);
                return NoContent();
            }
            return NotFound(new { error = "String not found" });
        }

        private (bool? is_palindrome, int? word_count, int? min_length) ParseNaturalLanguageQuery(string query)
        {
            query = query.ToLowerInvariant();
            bool? isPalindrome = query.Contains("palindromic") ? true : null;
            int? wordCount = query.Contains("single word") ? 1 : query.Contains("two words") ? 2 : null;
            int? minLength = query.Contains("at least") && Regex.Match(query, @"at least (\d+)").Success
                ? int.Parse(Regex.Match(query, @"at least (\d+)").Groups[1].Value)
                : null;
            return (isPalindrome, wordCount, minLength);
        }
    }
}