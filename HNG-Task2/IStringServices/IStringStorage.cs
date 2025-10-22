using HNG_Task2.Model;

namespace HNG_Task2.IStringServices
{
    public interface IStringStorage
    {
        Task AddAsync(StringEntity entity);
        Task<bool> ExistsAsync(string value);
        Task<StringEntity?> GetByValueAsync(string value);
        Task<List<StringEntity>> GetFilteredAsync(bool? isPalindrome, int? minLength, int? maxLength, int? wordCount, string? containsChar);
        Task DeleteAsync(string value);
    }
}
