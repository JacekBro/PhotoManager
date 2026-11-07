using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

public class PhotoScanner
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff", ".webp", ".heic"
    };

    private readonly PhotoCatalogContext _db;

    public PhotoScanner(PhotoCatalogContext db)
    {
        _db = db;
    }

    public async Task<long> ScanDriveAsync(string rootPath, CancellationToken ct = default)
    {
        var job = new ScanJob
        {
            RootPath = rootPath,
            Status = "Running",
            StartedUtc = DateTime.UtcNow
        };

        _db.ScanJobs.Add(job);
        await _db.SaveChangesAsync(ct);

        try
        {
            foreach (var file in Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories))
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    await UpsertFileAsync(file, ct);
                    job.FilesScanned++;
                    if (job.FilesScanned % 100 == 0)
                        await _db.SaveChangesAsync(ct);
                }
                catch (Exception ex)
                {
                    _db.ScanErrors.Add(new ScanError
                    {
                        ScanJobId = job.Id,
                        Path = file,
                        ErrorMessage = ex.Message,
                        OccurredUtc = DateTime.UtcNow
                    });
                    job.ErrorsCount++;
                }
            }

            job.Status = "Done";
            job.FinishedUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return job.Id;
        }
        catch
        {
            job.Status = "Failed";
            job.FinishedUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            throw;
        }
    }

    private async Task UpsertFileAsync(string path, CancellationToken ct)
    {
        var info = new FileInfo(path);

        var folderPath = info.Directory?.FullName ?? "";
        var folder = await GetOrCreateFolderAsync(folderPath, ct);

        var existing = await _db.Files.FirstOrDefaultAsync(x => x.FullPath == path, ct);

        var isImage = ImageExtensions.Contains(info.Extension);

        string? sha256 = null;
        if (isImage && info.Length > 0)
            sha256 = await ComputeSha256Async(path, ct);

        DateTime? takenUtc = null; // fill from EXIF later

        if (existing == null)
        {
            _db.Files.Add(new FileEntry
            {
                FullPath = path,
                FileName = info.Name,
                Extension = info.Extension,
                SizeBytes = info.Length,
                CreatedUtc = info.CreationTimeUtc,
                ModifiedUtc = info.LastWriteTimeUtc,
                AccessedUtc = info.LastAccessTimeUtc,
                TakenDateUtc = takenUtc,
                HashSha256 = sha256,
                FolderId = folder.Id,
                IsImage = isImage,
                IsDeletedOnDisk = false,
                ScannedUtc = DateTime.UtcNow
            });
        }
        else
        {
            existing.FileName = info.Name;
            existing.Extension = info.Extension;
            existing.SizeBytes = info.Length;
            existing.CreatedUtc = info.CreationTimeUtc;
            existing.ModifiedUtc = info.LastWriteTimeUtc;
            existing.AccessedUtc = info.LastAccessTimeUtc;
            existing.TakenDateUtc = takenUtc;
            existing.HashSha256 = sha256;
            existing.FolderId = folder.Id;
            existing.IsImage = isImage;
            existing.IsDeletedOnDisk = false;
            existing.ScannedUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task<FolderEntry> GetOrCreateFolderAsync(string path, CancellationToken ct)
    {
        var existing = await _db.Folders.FirstOrDefaultAsync(x => x.FullPath == path, ct);
        if (existing != null) return existing;

        var folder = new FolderEntry
        {
            FullPath = path,
            FolderName = new DirectoryInfo(path).Name,
            ScannedUtc = DateTime.UtcNow
        };

        _db.Folders.Add(folder);
        await _db.SaveChangesAsync(ct);
        return folder;
    }

    private static async Task<string> ComputeSha256Async(string path, CancellationToken ct)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA256.HashDataAsync(stream, ct);
        return Convert.ToHexString(hash);
    }
}
