using AdDiin.Models.Entities;
using System.Text.Json;

namespace AdDiin.Services
{
    public interface IAboutService
    {
        Task<AboutContentModel> GetContentAsync();
        Task<AboutContentModel> UpdateContentAsync(AboutContentModel content);
    }

    public class AboutService : IAboutService
    {
        private readonly string _filePath;
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public AboutService(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "App_Data", "about-content.json");
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        public async Task<AboutContentModel> GetContentAsync()
        {
            if (!File.Exists(_filePath))
            {
                var def = new AboutContentModel();
                await SaveToFileAsync(def);
                return def;
            }

            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                var model = JsonSerializer.Deserialize<AboutContentModel>(json, JsonOptions);
                return model ?? new AboutContentModel();
            }
            catch
            {
                return new AboutContentModel();
            }
        }

        public async Task<AboutContentModel> UpdateContentAsync(AboutContentModel content)
        {
            await SaveToFileAsync(content);
            return content;
        }

        private async Task SaveToFileAsync(AboutContentModel model)
        {
            var json = JsonSerializer.Serialize(model, JsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
