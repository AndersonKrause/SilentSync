using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace SilentSync.Api.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController(IWebHostEnvironment env, IConfiguration config) : ControllerBase
{
    [HttpPost("upload")]
    [RequestSizeLimit(2_000_000_000)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Empty file.");

        var mediaId = Guid.NewGuid().ToString("N");

        var uploadsDir = Path.Combine(env.ContentRootPath, "App_Data", "uploads");
        var outDir = Path.Combine(env.ContentRootPath, "App_Data", "processed", mediaId);

        Directory.CreateDirectory(uploadsDir);
        Directory.CreateDirectory(outDir);

        var safeName = Path.GetFileName(file.FileName);
        var inputPath = Path.Combine(uploadsDir, $"{mediaId}_{safeName}");

        await using (var fs = System.IO.File.Create(inputPath))
            await file.CopyToAsync(fs);

        // ✅ Gera MP3 (funciona no Android/Chrome)
        var mp3Path = Path.Combine(outDir, "audio.mp3");

        var args =
            $"-y -i \"{inputPath}\" -vn -c:a libmp3lame -b:a 128k -ac 2 -ar 44100 \"{mp3Path}\"";

        var ffmpegPath = config["Tools:FFmpegPath"];
        if (string.IsNullOrWhiteSpace(ffmpegPath))
            ffmpegPath = "ffmpeg";

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

        if (p.ExitCode != 0)
            return Problem($"ffmpeg failed: {stderr}");

        // ✅ devolve path relativo (o front monta URL com window.location.origin)
        var audioPath = $"/media/{mediaId}/audio.mp3";
        return Ok(new { mediaId, audioPath });
    }
}
