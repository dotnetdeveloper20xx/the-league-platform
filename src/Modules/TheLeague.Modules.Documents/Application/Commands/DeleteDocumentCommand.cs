using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Documents.Infrastructure.Persistence;
using TheLeague.Modules.Documents.Infrastructure.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Documents.Application.Commands;

public record DeleteDocumentCommand(Guid DocumentId) : IRequest<Result>;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Result>
{
    private readonly DocumentsDbContext _db;
    private readonly IBlobStorageService _blobStorage;

    public DeleteDocumentCommandHandler(DocumentsDbContext db, IBlobStorageService blobStorage)
    {
        _db = db;
        _blobStorage = blobStorage;
    }

    public async Task<Result> Handle(DeleteDocumentCommand request, CancellationToken ct)
    {
        var document = await _db.Documents
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId, ct);

        if (document is null)
            return Result.Failure("Document not found.");

        // Delete blob
        await _blobStorage.DeleteAsync(document.BlobKey, ct);

        // Delete thumbnail if exists
        if (!string.IsNullOrEmpty(document.ThumbnailBlobKey))
            await _blobStorage.DeleteAsync(document.ThumbnailBlobKey, ct);

        // Remove metadata
        _db.Documents.Remove(document);
        await _db.SaveChangesAsync(ct);

        return Result.Success("Document deleted successfully.");
    }
}
