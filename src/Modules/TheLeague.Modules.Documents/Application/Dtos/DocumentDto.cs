namespace TheLeague.Modules.Documents.Application.Dtos;

public record DocumentDto(
    Guid Id,
    Guid ClubId,
    Guid? MemberId,
    string FileName,
    string ContentType,
    long FileSize,
    string DocumentType,
    string BlobKey,
    Guid UploadedByUserId,
    DateTime UploadedAt,
    string? ThumbnailBlobKey
);
