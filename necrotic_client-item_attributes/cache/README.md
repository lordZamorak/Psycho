# Client Cache

The full Psycho client cache is intentionally not committed to regular git because several cache files exceed GitHub's per-file limits.

For local development, keep the existing cache files in this directory or sync them into `C:\Users\xzero\PsychoCache` with:

```powershell
..\scripts\run-client.ps1 -SyncCache
```

For hosting, publish the cache as a release artifact, external download, or Git LFS payload.
