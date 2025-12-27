using System.Threading.Tasks;

namespace DailyPlannerMauiBlazor.Services
{
    public interface IStorageService
    {
        Task SetAsync<T>(string key, T value);
        Task<T?> GetAsync<T>(string key);
        Task RemoveAsync(string key);
        Task ClearAllAsync();
    }
}
