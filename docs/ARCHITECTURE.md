# Architecture

## Overview

SilentSync is a real-time synchronized media platform that allows a central screen to control media playback while connected mobile devices receive synchronized audio streams.

## High-Level Architecture

```text
Browser (Admin)
        │
        ▼
 ASP.NET Core API
        │
        ├── SignalR Hub
        ├── Authentication
        ├── Media Processing
        ├── Room Management
        │
        ▼
 PostgreSQL (Neon)

        ▼
 FFmpeg Processing

        ▼
 HLS Media Output

        ▼
 Mobile Clients
```

## Components

### Authentication

* Email-based authentication
* One-time login codes
* JWT tokens
* Role-based authorization

### Room Management

* Room creation
* Room membership
* Playback synchronization
* Real-time state updates

### Media Processing

* Video upload
* Audio extraction using FFmpeg
* HLS generation
* Media distribution

### Real-Time Communication

SignalR is used for:

* Playback events
* Room updates
* Synchronization messages
* Drift correction

## Infrastructure

* Render
* Neon PostgreSQL
* Cloudflare
* Resend
