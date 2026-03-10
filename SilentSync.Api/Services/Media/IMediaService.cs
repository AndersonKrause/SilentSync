namespace SilentSync.Api.Services.Media;

public interface IMediaService
{
    Task<object> UploadAsync(IFormFile file);
}