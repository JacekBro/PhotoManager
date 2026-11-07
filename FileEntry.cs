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

    public DateTime? TakenDateUtc { get; set; }   // from EXIF if available
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
    public string GroupKey { get; set; } = "";   // hash or grouping key
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
    public string Status { get; set; } = ""; // Pending, Running, Done, Failed
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
