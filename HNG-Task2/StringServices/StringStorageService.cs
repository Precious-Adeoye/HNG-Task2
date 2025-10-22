using System.Text.Json;
using HNG_Task2.Model;
using HNG_Task2.Utility;
using HNG_Task2.IStringServices;

namespace HNG_Task2.StringServices
{
    public class StringStorageService : IStringStorage
    {
        private readonly Dictionary<string, StringEntity> _inMemoryStore = new();
        private readonly string _jsonFilePath;
        private readonly object _fileLock = new();

        public StringStorageService(IConfiguration configuration)
        {
            // Get JsonFilePath from config or environment variable, fallback to platform-appropriate default
            _jsonFilePath = configuration.GetValue<string>("Storage:JsonFilePath")
                ?? (Environment.OSVersion.Platform == PlatformID.Win32NT
                    ? Path.Combine(Directory.GetCurrentDirectory(), "data", "strings.json")
                    : "/tmp/strings.json");

            // Ensure parent directory exists
            var directory = Path.GetDirectoryName(_jsonFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            LoadFromJson();
        }

        public async Task AddAsync(StringEntity entity)
        {
            _inMemoryStore[entity.Id] = entity;
            await SaveToJsonAsync();
        }

        public async Task<bool> ExistsAsync(string value)
        {
            string hash = HashHelper.ComputeSha256(value);
            return await Task.FromResult(_inMemoryStore.ContainsKey(hash));
        }

        public async Task<StringEntity?> GetByValueAsync(string value)
        {
            string hash = HashHelper.ComputeSha256(value);
            return await Task.FromResult(_inMemoryStore.TryGetValue(hash, out var entity) ? entity : null);
        }

        public async Task<List<StringEntity>> GetFilteredAsync(bool? isPalindrome, int? minLength, int? maxLength, int? wordCount, string? containsChar)
        {
            var query = _inMemoryStore.Values.AsEnumerable();
            if (isPalindrome.HasValue)
                query = query.Where(s => s.IsPalindrome == isPalindrome.Value);
            if (minLength.HasValue)
                query = query.Where(s => s.Length >= minLength.Value);
            if (maxLength.HasValue)
                query = query.Where(s => s.Length <= maxLength.Value);
            if (wordCount.HasValue)
                query = query.Where(s => s.WordCount == wordCount.Value);
            if (!string.IsNullOrEmpty(containsChar))
                query = query.Where(s => s.Value.Contains(containsChar, StringComparison.OrdinalIgnoreCase));

            return await Task.FromResult(query.ToList());
        }

        public async Task DeleteAsync(string value)
        {
            string hash = HashHelper.ComputeSha256(value);
            _inMemoryStore.Remove(hash);
            await SaveToJsonAsync();
        }

        private void LoadFromJson()
        {
            lock (_fileLock)
            {
                if (File.Exists(_jsonFilePath))
                {
                    try
                    {
                        var json = File.ReadAllText(_jsonFilePath);
                        var entities = JsonSerializer.Deserialize<List<StringEntity>>(json);
                        if (entities != null)
                        {
                            _inMemoryStore.Clear();
                            foreach (var entity in entities)
                            {
                                _inMemoryStore[entity.Id] = entity;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to load JSON: {ex.Message}");
                    }
                }
            }
        }

        private async Task SaveToJsonAsync()
        {
            lock (_fileLock)
            {
                try
                {
                    var directory = Path.GetDirectoryName(_jsonFilePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var json = JsonSerializer.Serialize(_inMemoryStore.Values, options);
                    File.WriteAllText(_jsonFilePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to save JSON: {ex.Message}");
                }
            }
            await Task.CompletedTask;
        }
    }
}
