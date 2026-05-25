using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Documents.Domain;

public class Document : TenantEntity
{
    public Guid? MemberId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string DocumentType { get; private set; } = string.Empty;
    public string BlobKey { get; private set; } = string.Empty;
    public Guid UploadedByUserId { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public string? ThumbnailBlobKey { get; private set; }

    private Document() { }

    public static Document Create(
        Guid clubId,
        Guid? memberId,
        string fileName,
        string contentType,
        long fileSize,
        string documentType,
        string blobKey,
        Guid uploadedByUserId,
        string? thumbnailBlobKey = null)
    {
        return new Document
        {
            ClubId = clubId,
            MemberId = memberId,
            FileName = fileName,
            ContentType = contentType,
            FileSize = fileSize,
            DocumentType = documentType,
            BlobKey = blobKey,
            UploadedByUserId = uploadedByUserId,
            UploadedAt = DateTime.UtcNow,
            ThumbnailBlobKey = thumbnailBlobKey
        };
    }
}
