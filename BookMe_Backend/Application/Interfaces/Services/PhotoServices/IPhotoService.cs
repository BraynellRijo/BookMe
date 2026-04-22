using Application.DTOs.PhotoDTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services.PhotoServices
{
    public interface IPhotoService
    {
        Task<PhotoUploadResultDTO> AddPhotoAsync(IFormFile file, string folder);
        Task<IEnumerable<PhotoUploadResultDTO>> AddPhotosAsync(IEnumerable<IFormFile> files, string folder);
        Task<bool> DeletePhotoAsync(string publicId);
    }
}
