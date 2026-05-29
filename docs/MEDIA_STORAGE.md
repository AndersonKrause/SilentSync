# Media Storage

## Current Implementation

Processed media is stored locally inside the application container.

```text
/app/App_Data/processed
```

## Media Pipeline

1. Upload video
2. Process with FFmpeg
3. Generate HLS output
4. Serve through HTTP

## Limitation

Render containers are ephemeral.

Uploaded files may be lost after redeployment.