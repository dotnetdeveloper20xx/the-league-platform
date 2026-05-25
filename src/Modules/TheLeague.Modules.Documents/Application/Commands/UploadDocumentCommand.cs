using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Documents.Application.Dtos;
using TheLeague.Modules.Documents.Application.Events;
using TheLeague.Modules.Documents.Domain;
using TheLeague.Modules.Documents.Infrastructure.Persistence;
using TheLeague.Modules.Documents.Infrastructure.Services;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Documents.Application.Commands;

public record UploadDocumentCommand(
    Guid? MemberId,
    string FileName,
    string ContentType,
    long FileSize,
    string DocumentType,
    Stream FileStream,
    Guid UploadedByUserId
) : IRequest<Result<DocumentDto>>;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Result<DocumentDto>>
{
    private readonly DocumentsDbContext _db;
    private readonly IBlobStorageService _blobStorage;
    private readonly IFileValidator _fileValidator;
    private readonly IImageOptimizer _imageOptimizer;
    private readonly IIntegrationEventBus _eventBus;
    private readonly ITenantService _tenantService;

    private static readonly Dictionary<string, long> StorageLimitsPerTier = new()
    {
        { "Free", 1L * 1024 * 1024 * 1024 },         // 1 GB
        { "Starter", 5L * 1024 * 1024 * 1024 },      // 5 GB
        { "Pro", 25L * 1024 * 1024 * 1024 },         // 25 GB
        { "Enterprise", 100L * 1024 * 1024 * 1024 }  // 100 GB
    };

    public UploadDocumentCommandHandler(
        DocumentsDbContext db,
        IBlobStorageService blobStorage,
        IFileValidator fileValidator,
        IImageOptimizer imageOptimizer,
        IIntegrationEventBus eventBus,
        ITenantService tenantService)
    {
        _db = db;
        _blobStorage = blobStorage;
        _fileValidator = fileValidator;
        _imageOptimizer = imageOptimizer;
        _eventBus = eventBus;
        _tenantService = tenantService;
    }

    public async Task<Result<DocumentDto>> Handle(UploadDocumentCommand request, CancellationToken ct)
    {
        var clubId = _tenantService.CurrentTenantId;
        if (clubId is null)
            return Result.Failure<DocumentDto>("Tenant context is required.");

        // Validate file type
        if (!_fileValidator.IsValidFileType(request.ContentType, request.FileName))
            return Result.Failure<DocumentDto>(
                $"Invalid file type. Allowed types: {string.Join(", ", _fileValidator.AllowedExtensions)}");

        // Validate file size
        if (!_fileValidator.IsValidFileSize(request.FileSize))
            return Result.Failure<DocumentDto>(
                $"File size exceeds the maximum allowed size of {_fileValidator.MaxFileSizeBytes / (1024 * 1024)}MB.");

        // Validate document type
        var validDocumentTypes = new[] { "ProfilePhoto", "MedicalForm", "ConsentForm", "DBSCertificate", "ClubDocument", "EventMedia" };
        if (!validDocumentTypes.Contains(request.DocumentType))
            return Result.Failure<DocumentDto>(
                $"Invalid document type. Allowed types: {string.Join(", ", validDocumentTypes)}");

        // Mock malware scan (in production, integrate with a real scanner)
        var isMalwareFree = await ScanForMalwareAsync(request.FileStream, ct);
        if (!isMalwareFree)
            return Result.Failure<DocumentDto>("File rejected: potential malware detected.");

        // Reset stream position after scan
        request.FileStream.Position = 0;

        // Upload to blob storage
        var containerPath = $"{clubId}";
        var blobKey = await _blobStorage.UploadAsync(containerPath, request.FileStream, request.ContentType, ct);

        // Generate thumbnail for images (profile photos)
        string? thumbnailBlobKey = null;
        if (request.DocumentType == "ProfilePhoto" && IsImageContentType(request.ContentType))
        {
            request.FileStream.Position = 0;
            using var thumbnailStream = await _imageOptimizer.GenerateThumbnailAsync(request.FileStream, ct);
            thumbnailBlobKey = await _blobStorage.UploadAsync($"{clubId}/thumbnails", thumbnailStream, request.ContentType, ct);
        }

        // Create document metadata
        var document = Document.Create(
            clubId.Value,
            request.MemberId,
            request.FileName,
            request.ContentType,
            request.FileSize,
            request.DocumentType,
            blobKey,
            request.UploadedByUserId,
            thumbnailBlobKey);

        _db.Documents.Add(document);
        await _db.SaveChangesAsync(ct);

        // Publish integration event
        await _eventBus.PublishAsync(new DocumentUploadedEvent(
            document.Id,
            clubId.Value,
            request.MemberId,
            request.FileName,
            request.DocumentType,
            request.FileSize), ct);

        return Result.Success(ToDto(document));
    }

    private static Task<bool> ScanForMalwareAsync(Stream fileStream, CancellationToken ct)
    {
        // Mock malware scan — always passes in development
        // In production, integrate with Windows Defender, ClamAV, or a cloud scanning service
        return Task.FromResult(true);
    }

    private static bool IsImageContentType(string contentType)
    {
        return contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }

    private static DocumentDto ToDto(Document doc) => new(
        doc.Id,
        doc.ClubId,
        doc.MemberId,
        doc.FileName,
        doc.ContentType,
        doc.FileSize,
        doc.DocumentType,
        doc.BlobKey,
        doc.UploadedByUserId,
        doc.UploadedAt,
        doc.ThumbnailBlobKey);
}
