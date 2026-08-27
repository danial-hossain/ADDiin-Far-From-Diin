using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace AdDiin.Services
{
    public interface IPhotoService
    {
        Task<ImageUploadResult?> AddPhotoAsync(IFormFile file, string folder = "ad-diin");
        Task<DeletionResult?> DeletePhotoAsync(string publicId);
    }

    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;

        public PhotoService(IConfiguration config)
        {
            var cloudName = config["CloudinarySettings:CloudName"];
            var apiKey = config["CloudinarySettings:ApiKey"];
            var apiSecret = config["CloudinarySettings:ApiSecret"];

            var acc = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(acc);
            _cloudinary.Api.Secure = true;
        }

        public async Task<ImageUploadResult?> AddPhotoAsync(IFormFile file, string folder = "ad-diin")
        {
            if (file == null || file.Length == 0) return null;

            var uploadResult = new ImageUploadResult();

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                Transformation = new Transformation().Crop("limit").Width(1200).Height(800).Quality("auto")
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult;
        }

        public async Task<DeletionResult?> DeletePhotoAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId)) return null;

            var deleteParams = new DeletionParams(publicId);
            return await _cloudinary.DestroyAsync(deleteParams);
        }
    }
}
