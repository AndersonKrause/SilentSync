# 🎬 SilentSync

SilentSync is a real-time synchronized media platform that enables multiple mobile devices to play perfectly synchronized audio while a central screen displays video content.

The platform is designed for low-latency, scalable playback synchronization across many connected users.

## 🌐 Live Demo

https://silentsync.uk

---

## 🚀 Features

✅ Room-based synchronization

✅ Real-time communication using SignalR

✅ JWT authentication

✅ Email verification and login codes

✅ Video upload and processing

✅ Automatic audio extraction with FFmpeg

✅ HLS media streaming

✅ Multi-user support

✅ Admin dashboard

✅ Drift correction and playback synchronization

---

## 🏗 Architecture

### Backend

* ASP.NET Core 10
* SignalR
* Entity Framework Core
* PostgreSQL (Neon)
* FFmpeg
* JWT Authentication
* Resend Email Service
* Swagger

### Frontend

* HTML
* JavaScript
* SignalR Client
* HTML5 Audio/Video
* hls.js

### Infrastructure

* Render
* Neon PostgreSQL
* Cloudflare DNS
* Resend
* Docker

---

## 🛠 Technology Stack

* .NET 10
* ASP.NET Core
* SignalR
* Entity Framework Core
* PostgreSQL
* Docker
* FFmpeg
* JavaScript
* Cloudflare
* Render
* Resend

---

## 🎯 Use Cases

SilentSync can be used for:

* Events
* Silent cinema experiences
* Museums and exhibitions
* Conferences
* Educational environments
* Multi-language audio experiences

---

## ⚙️ Local Development

### 1. Start PostgreSQL

```powershell
docker compose up -d
docker ps
```

### 2. Configure appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=silentsync;Username=your_username;Password=your_password"
  },

  "Tools": {
    "FFmpegPath": "C:\\Path\\To\\ffmpeg\\bin\\ffmpeg.exe"
  },

  "PublicBaseUrl": "http://YOUR_IP:5031",

  "Jwt": {
    "Issuer": "SilentSync",
    "Audience": "SilentSync",
    "Key": "YOUR_SECRET_KEY"
  }
}
```

### 3. Apply Migrations

```powershell
dotnet ef database update
```

### 4. Run the API

```powershell
dotnet run
```

---

## 📚 Documentation

Detailed technical documentation can be found in the `/docs` folder:

* DEPLOYMENT.md
* DATABASE.md
* DOMAIN_SETUP.md
* EMAIL_CONFIGURATION.md
* MEDIA_STORAGE.md
* TROUBLESHOOTING.md

---

## 🔒 Production Environment

Production deployment includes:

* Render
* Neon PostgreSQL
* Cloudflare
* Resend
* HTTPS
* Custom Domain

---

## 👨‍💻 Author

Developed by Anderson Davi Krause
