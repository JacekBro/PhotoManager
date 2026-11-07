using Microsoft.EntityFrameworkCore;

public class PhotoCatalogContext : DbContext
{
    public DbSet<FileEntry> Files => Set<FileEntry>();
    public DbSet<FolderEntry> Folders => Set<FolderEntry>();
    public DbSet<DuplicateGroup> DuplicateGroups => Set<DuplicateGroup>();
    public DbSet<DuplicateMember> DuplicateMembers => Set<DuplicateMember>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<FileTag> FileTags => Set<FileTag>();
    public DbSet<ScanJob> ScanJobs => Set<ScanJob>();
    public DbSet<ScanError> ScanErrors => Set<ScanError>();

    public PhotoCatalogContext(DbContextOptions<PhotoCatalogContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FolderEntry>()
            .HasIndex(x => x.FullPath)
            .IsUnique();

        modelBuilder.Entity<FileEntry>()
            .HasIndex(x => x.FullPath)
            .IsUnique();

        modelBuilder.Entity<FileEntry>()
            .HasIndex(x => x.HashSha256);

        modelBuilder.Entity<FileEntry>()
            .HasIndex(x => x.TakenDateUtc);

        modelBuilder.Entity<Tag>()
            .HasIndex(x => x.Name)
            .IsUnique();

        modelBuilder.Entity<FileTag>()
            .HasKey(x => new { x.FileEntryId, x.TagId });

        modelBuilder.Entity<DuplicateMember>()
            .HasOne<FileEntry>()
            .WithMany()
            .HasForeignKey(x => x.FileEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DuplicateMember>()
            .HasOne<DuplicateGroup>()
            .WithMany()
            .HasForeignKey(x => x.DuplicateGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FileEntry
{
    public long Id { get; set; }
    public string FullPath { get; set; } = "";
    public string FileName { get; set; } = "";
    public string Extension { get; set; } = "";
    public long SizeBytes { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public DateTime? ModifiedUtc { get; set; }
    public DateTime? AccessedUtc { get; set; }
    public DateTime? TakenDateUtc { get; set; }
    public string? HashSha256 { get; set; }
    public string? PerceptualHash { get; set; }
    public int? FolderId { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public bool IsImage { get; set; }
    public bool IsDeletedOnDisk { get; set; }
    public DateTime ScannedUtc { get; set; }
}

public class FolderEntry
{
    public int Id { get; set; }
    public string FullPath { get; set; } = "";
    public string FolderName { get; set; } = "";
    public int? ParentFolderId { get; set; }
    public DateTime ScannedUtc { get; set; }
}

public class DuplicateGroup
{
    public long Id { get; set; }
    public string GroupKey { get; set; } = "";
    public DateTime CreatedUtc { get; set; }
}

public class DuplicateMember
{
    public long Id { get; set; }
    public long DuplicateGroupId { get; set; }
    public long FileEntryId { get; set; }
    public bool IsPrimary { get; set; }
    public double? MatchScore { get; set; }
}

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? ColorHex { get; set; }
}

public class FileTag
{
    public long FileEntryId { get; set; }
    public int TagId { get; set; }
    public DateTime TaggedUtc { get; set; }
}

public class ScanJob
{
    public long Id { get; set; }
    public string RootPath { get; set; } = "";
    public string Status { get; set; } = "";
    public int FilesScanned { get; set; }
    public int ErrorsCount { get; set; }
    public DateTime StartedUtc { get; set; }
    public DateTime? FinishedUtc { get; set; }
}

public class ScanError
{
    public long Id { get; set; }
    public long ScanJobId { get; set; }
    public string Path { get; set; } = "";
    public string ErrorMessage { get; set; } = "";
    public DateTime OccurredUtc { get; set; }
}
