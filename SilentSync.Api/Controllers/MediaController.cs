using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace SilentSync.Api.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    public MediaController(IWebHostEnvironment env, IConfiguration config)
    {
        _env = env ?? throw new ArgumentNullException(nameof(env));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }
    
    [HttpPost("upload")]
    [RequestSizeLimit(2_000_000_000)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Empty file.");

        var mediaId = Guid.NewGuid().ToString("N");

        var uploadsDir = Path.Combine(_env.ContentRootPath, "App_Data", "uploads");
        var outDir = Path.Combine(_env.ContentRootPath, "App_Data", "processed", mediaId);

        Directory.CreateDirectory(uploadsDir);
        Directory.CreateDirectory(outDir);

        var safeName = Path.GetFileName(file.FileName);
        var inputPath = Path.Combine(uploadsDir, $"{mediaId}_{safeName}");

        await using (var fs = System.IO.File.Create(inputPath))
            await file.CopyToAsync(fs);

        var ffmpegPath = _config["Tools:FFmpegPath"];
        if (string.IsNullOrWhiteSpace(ffmpegPath))
            ffmpegPath = "ffmpeg";

        // 1) gera/normaliza vídeo para um caminho servido pelo /media
        // Tentamos remux (rápido). Se falhar, fazemos reencode básico.
        var videoOnDisk = Path.Combine(outDir, "video.mp4");

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

        // remux (sem reencode)
        var remuxArgs = $"-y -i \"{inputPath}\" -c copy \"{videoOnDisk}\"";
        var (remuxCode, remuxErr) = await RunFfmpeg(remuxArgs);

        if (remuxCode != 0)
        {
            // fallback reencode
            var reencodeArgs = $"-y -i \"{inputPath}\" -c:v libx264 -preset veryfast -crf 23 -c:a aac -b:a 128k \"{videoOnDisk}\"";
            var (reCode, reErr) = await RunFfmpeg(reencodeArgs);
            if (reCode != 0)
                return Problem($"ffmpeg video failed: {remuxErr}\n---fallback---\n{reErr}");
        }

        // 2) Gera MP3 (igual você já fazia, mas a partir do video.mp4)
        var mp3Path = Path.Combine(outDir, "audio.mp3");
        var audioArgs = $"-y -i \"{videoOnDisk}\" -vn -c:a libmp3lame -b:a 128k -ac 2 -ar 44100 \"{mp3Path}\"";
        var (audioCode, audioErr) = await RunFfmpeg(audioArgs);

        if (audioCode != 0)
            return Problem($"ffmpeg audio failed: {audioErr}");

        // devolve paths relativos servidos por /media (Program.cs já mapeia!)
        var audioPath = $"/media/{mediaId}/audio.mp3";
        var videoPath = $"/media/{mediaId}/video.mp4";

        return Ok(new { mediaId, audioPath, videoPath });
    }
}
