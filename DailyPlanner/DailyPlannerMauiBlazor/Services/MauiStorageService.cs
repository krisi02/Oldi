using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace DailyPlannerMauiBlazor.Services
{
    public class MauiStorageService : IStorageService
    {
        public async Task SetAsync<T>(string key, T value)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                await SecureStorage.SetAsync(key, json);
            }
            catch (Exception)
            {
                // Handle exceptions appropriately
            }
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var json = await SecureStorage.GetAsync(key);
                if (string.IsNullOrEmpty(json))
                    return default;
                
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception)
            {
                return default;
            }
        }

        public Task RemoveAsync(string key)
        {
            SecureStorage.Remove(key);
            return Task.CompletedTask;
        }

        public Task ClearAllAsync()
        {
            SecureStorage.RemoveAll();
            return Task.CompletedTask;
        }
    }
}
