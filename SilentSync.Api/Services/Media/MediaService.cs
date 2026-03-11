using System.Diagnostics;

namespace SilentSync.Api.Services.Media;

public class MediaService : IMediaService
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<MediaService> _logger;

    private const long MaxUploadBytes = 1024L * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mov", ".m4v", ".webm", ".mkv"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "video/mp4",
        "video/quicktime",
        "video/x-m4v",
        "video/webm",
        "video/x-matroska",
        "application/octet-stream"
    };

    public MediaService(
        IWebHostEnvironment env,
        IConfiguration config,
        ILogger<MediaService> logger)
    {
        _env = env;
        _config = config;
        _logger = logger;
    }

    public async Task<object> UploadAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("Empty file.");

        if (file.Length > MaxUploadBytes)
            throw new ArgumentException("File too large.");

        if (!IsAllowedUpload(file))
            throw new ArgumentException("Invalid file type.");

        var mediaId = Guid.NewGuid().ToString("N");
        var outDir = Path.Combine(_env.ContentRootPath, "App_Data", "processed", mediaId);

        Directory.CreateDirectory(outDir);

        var ext = Path.GetExtension(file.FileName ?? "").ToLowerInvariant();
        var inputPath = Path.Combine(outDir, $"input{ext}");
        var videoOnDisk = Path.Combine(outDir, "video.mp4");
        var mp3Path = Path.Combine(outDir, "audio.mp3");

        try
        {
            await using (var fs = File.Create(inputPath))
                await file.CopyToAsync(fs);

            var ffmpegPath = _config["Tools:FFmpegPath"] ?? "ffmpeg";

            async Task<(int code, string stderr)> RunFfmpeg(string args)
            {
                var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = args,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                p.Start();
                var stderr = await p.StandardError.ReadToEndAsync();
                await p.WaitForExitAsync();

                return (p.ExitCode, stderr);
            }

            var remuxArgs = $"-y -i \"{inputPath}\" -c copy \"{videoOnDisk}\"";
            var (remuxCode, _) = await RunFfmpeg(remuxArgs);

            if (remuxCode != 0)
            {
                var reencodeArgs =
                    $"-y -i \"{inputPath}\" -c:v libx264 -preset veryfast -crf 23 -c:a aac -b:a 128k \"{videoOnDisk}\"";

                var (reCode, _) = await RunFfmpeg(reencodeArgs);

                if (reCode != 0)
                    throw new Exception("Video processing failed.");
            }

            var audioArgs =
                $"-y -i \"{videoOnDisk}\" -vn -c:a libmp3lame -b:a 128k -ac 2 -ar 44100 \"{mp3Path}\"";

            var (audioCode, _) = await RunFfmpeg(audioArgs);

            if (audioCode != 0)
                throw new Exception("Audio extraction failed.");

            return new
            {
                mediaId,
                audioPath = $"/media/{mediaId}/audio.mp3",
                videoPath = $"/media/{mediaId}/video.mp4"
            };
        }
        catch
        {
            TryDeleteDirectory(outDir);
            throw;
        }
    }

    private static bool IsAllowedUpload(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName ?? "");
        if (!AllowedExtensions.Contains(ext))
            return false;

        return AllowedContentTypes.Contains(file.ContentType);
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
        catch { }
    }
}