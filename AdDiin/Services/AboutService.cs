using AdDiin.Models.Entities;
using System.Text.Json;

namespace AdDiin.Services
{
    /// <summary>
    /// Defines access to the file-backed About page content.
    /// </summary>
    public interface IAboutService
    {
        Task<AboutContentModel> GetContentAsync();
        Task<AboutContentModel> UpdateContentAsync(AboutContentModel content);
    }

    /// <summary>
    /// Reads and writes editable About content under the application data folder.
    /// </summary>
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

        /// <summary>
        /// Returns saved content or creates a default document when none exists.
        /// </summary>
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

        /// <summary>
        /// Persists the supplied About page model as formatted JSON.
        /// </summary>
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
