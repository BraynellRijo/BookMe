using Application.DTOs.PhotoDTOs;
using Application.Interfaces.Services.PhotoServices;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.PhotoServices
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;

        public PhotoService(IOptions<CloudinarySettings> options)
        {
            var account = new Account(
                options.Value.CloudName,
                options.Value.ApiKey,
                options.Value.ApiSecret
            );

            _cloudinary = new Cloudinary( account );
        }

        public async Task<PhotoUploadResultDTO> AddPhotoAsync(IFormFile file, string folder)
        {
            var uploadResult = new ImageUploadResult();

            if(file.Length > 0)
            {
                var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto"),
                    Folder = folder
                };

                uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if(uploadResult.Error != null)
                    throw new Exception($"Cloudinary Error: {uploadResult.Error.Message}");
            
            }

            return new PhotoUploadResultDTO 
            {
                PublicId = uploadResult.PublicId,
                Url = uploadResult.SecureUrl.ToString()
            };
        }

        public async Task<IEnumerable<PhotoUploadResultDTO>> AddPhotosAsync(IEnumerable<IFormFile> files, string folder)
        {
            var uploadTasks = new List<Task<PhotoUploadResultDTO>>();

            foreach(var file in files)
            {
                uploadTasks.Add(AddPhotoAsync(file, folder));
            }

            var uploadResults = await Task.WhenAll(uploadTasks);
            return uploadResults;
        }

        public async Task<bool> DeletePhotoAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);
            return result.Result == "ok";
        }

    }
}
