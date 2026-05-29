# Deployment Guide

## Production Stack

* Render
* Neon PostgreSQL
* Cloudflare
* Resend

## Environment Variables

```text
ConnectionStrings__Default

Jwt__Issuer
Jwt__Audience
Jwt__Key

PublicBaseUrl

Tools__FFmpegPath

Resend__ApiKey
```

## Deployment Process

### 1. Push Changes

```bash
git add .
git commit -m "Description"
git push
```

### 2. Automatic Deployment

Render automatically deploys new commits pushed to the main branch.

### 3. Verify Deployment

Check:

* Build logs
* Application logs
* HTTPS status
* Domain status

## Production URL

https://silentsync.uk
