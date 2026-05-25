namespace TheLeague.Modules.Documents.Infrastructure.Services;

public interface IFileValidator
{
    bool IsValidFileType(string contentType, string fileName);
    bool IsValidFileSize(long fileSize);
    string[] AllowedExtensions { get; }
    string[] AllowedContentTypes { get; }
    long MaxFileSizeBytes { get; }
}

public class FileValidator : IFileValidator
{
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    private static readonly string[] _allowedExtensions =
    {
        ".jpg", ".jpeg", ".png", ".webp",
        ".pdf", ".docx",
        ".csv", ".xlsx"
    };

    private static readonly string[] _allowedContentTypes =
    {
        "image/jpeg", "image/png", "image/webp",
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/csv",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    };

    public string[] AllowedExtensions => _allowedExtensions;
    public string[] AllowedContentTypes => _allowedContentTypes;
    public long MaxFileSizeBytes => MaxFileSize;

    public bool IsValidFileType(string contentType, string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension))
            return false;

        return _allowedExtensions.Contains(extension) &&
               _allowedContentTypes.Contains(contentType.ToLowerInvariant());
    }

    public bool IsValidFileSize(long fileSize)
    {
        return fileSize > 0 && fileSize <= MaxFileSize;
    }
}
