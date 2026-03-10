using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace SilentSync.Api.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<MediaController> _logger;

    private const long MaxUploadBytes = 250L * 1024 * 1024;

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

    public MediaController(
        IWebHostEnvironment env,
        IConfiguration config,
        ILogger<MediaController> logger)
    {
        _env = env ?? throw new ArgumentNullException(nameof(env));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("upload")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Empty file.");

        if (file.Length > MaxUploadBytes)
            return BadRequest($"File too large. Max size is {MaxUploadBytes / (1024 * 1024)} MB.");

        if (!IsAllowedUpload(file))
            return BadRequest("Invalid file type. Allowed: .mp4, .mov, .m4v, .webm, .mkv");

        var mediaId = Guid.NewGuid().ToString("N");
        var outDir = Path.Combine(_env.ContentRootPath, "App_Data", "processed", mediaId);

        Directory.CreateDirectory(outDir);

        var ext = Path.GetExtension(file.FileName ?? string.Empty).ToLowerInvariant();
        var inputPath = Path.Combine(outDir, $"input{ext}");
        var videoOnDisk = Path.Combine(outDir, "video.mp4");
        var mp3Path = Path.Combine(outDir, "audio.mp3");

        try
        {
            _logger.LogInformation(
                "Processing media upload {MediaId} ({FileName}, {Length} bytes)",
                mediaId, file.FileName, file.Length);

            await using (var fs = System.IO.File.Create(inputPath))
            {
                await file.CopyToAsync(fs);
            }

            var ffmpegPath = _config["Tools:FFmpegPath"];
            if (string.IsNullOrWhiteSpace(ffmpegPath))
                ffmpegPath = "ffmpeg";

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
            var (remuxCode, remuxErr) = await RunFfmpeg(remuxArgs);

            if (remuxCode != 0)
            {
                _logger.LogWarning("FFmpeg remux failed for {MediaId}: {Error}", mediaId, remuxErr);

                var reencodeArgs =
                    $"-y -i \"{inputPath}\" -c:v libx264 -preset veryfast -crf 23 -c:a aac -b:a 128k \"{videoOnDisk}\"";

                var (reCode, reErr) = await RunFfmpeg(reencodeArgs);

                if (reCode != 0)
                {
                    _logger.LogError("FFmpeg video processing failed for {MediaId}: {Error}", mediaId, reErr);
                    TryDeleteDirectory(outDir);
                    return Problem("Video processing failed.");
                }
            }

            var audioArgs =
                $"-y -i \"{videoOnDisk}\" -vn -c:a libmp3lame -b:a 128k -ac 2 -ar 44100 \"{mp3Path}\"";

            var (audioCode, audioErr) = await RunFfmpeg(audioArgs);

            if (audioCode != 0)
            {
                _logger.LogError("FFmpeg audio extraction failed for {MediaId}: {Error}", mediaId, audioErr);
                TryDeleteDirectory(outDir);
                return Problem("Audio extraction failed.");
            }

            var audioPath = $"/media/{mediaId}/audio.mp3";
            var videoPath = $"/media/{mediaId}/video.mp4";

            _logger.LogInformation("Media upload processed successfully for {MediaId}", mediaId);

            return Ok(new { mediaId, audioPath, videoPath });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected media upload failure for {MediaId}", mediaId);
            TryDeleteDirectory(outDir);
            return Problem("Unexpected media processing failure.");
        }
    }

    private static bool IsAllowedUpload(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
            return false;

        if (string.IsNullOrWhiteSpace(file.ContentType))
            return false;

        return AllowedContentTypes.Contains(file.ContentType);
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch
        {
        }
    }
}