# Troubleshooting

## Neon Authentication Failed

Error:

```text
28P01: password authentication failed
```

Solution:

* Verify credentials
* Reset Neon password
* Update connection string

---

## Render Dockerfile Not Found

Error:

```text
failed to read dockerfile
```

Solution:

* Verify Dockerfile location
* Verify Render root directory

---

## App_Data Permission Denied

Error:

```text
UnauthorizedAccessException
```

Solution:

* Use writable directories
* Avoid protected container paths

---

## Gmail SMTP Timeout

Error:

```text
SmtpException: The operation has timed out
```

Solution:

* Use Resend instead of Gmail SMTP

---

## Cloudflare Error 1000

Error:

```text
DNS points to prohibited IP
```

Solution:

* Use DNS Only mode
* Disable Cloudflare proxy for Render records
