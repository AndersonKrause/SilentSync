using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
using SilentSync.Api.Data;
using SilentSync.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// (Opcional) se estiver testando via celular no HTTP, pode comentar o redirect em DEV
// app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

var mediaRoot = Path.Combine(app.Environment.ContentRootPath, "App_Data", "processed");
Directory.CreateDirectory(mediaRoot);

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".m3u8"] = "application/vnd.apple.mpegurl";
provider.Mappings[".aac"]  = "audio/aac";
provider.Mappings[".ts"]   = "video/mp2t";

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaRoot),
    RequestPath = "/media",
    ContentTypeProvider = provider
});

app.MapControllers();

app.MapHub<RoomHub>("/hubs/rooms");

app.Run();
