# 🎬 SilentSync

SilentSync is a real-time audio synchronization platform that allows multiple mobile devices to play perfectly synchronized audio alongside a centrally displayed video (e.g., on a projector or large screen).

The goal is to enable scalable, low-latency synchronized audio playback across many users using their own smartphones.

## 🚀 Current Features

✅ Room-based synchronization (6-digit room codes)

✅ Central controller (screen operator/controller)

✅ Real-time synchronization via SignalR

✅ Automatic audio extraction via FFmpeg

✅ Audio streaming over HTTP/HTTPS

✅ Drift correction mechanism

✅ Multi-user support

## 🏗 Architecture Overview

### Backend
- ASP.NET Core 9
- SignalR (real-time communication)
- Entity Framework Core
- PostgreSQL (via Docker)
- FFmpeg (audio extraction)
- Swagger (API documentation)

### Frontend (MVP)
- HTML + JavaScript
- SignalR client
- `<audio>` HTML5
- hls.js (HLS)

 ## 🛠 Technologies
- .NET 9
- ASP.NET Core
- SignalR
- Entity Framework Core
- PostgreSQL
- Docker
- FFmpeg
- JavaScript
 

## 🎯 Project Vision

SilentSync aims to provide a scalable and lightweight solution for synchronized mobile audio experiences in:
- Events
- Independent cinemas
- Silent exhibitions
- Conferences
- Educational environments

## 👨‍💻 Author
Developed by Anderson Davi Krause
