using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Documents.Infrastructure.Persistence;
using TheLeague.Modules.Documents.Infrastructure.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Documents.Application.Commands;

public record GenerateDownloadUrlCommand(Guid DocumentId) : IRequest<Result<string>>;

public class GenerateDownloadUrlCommandHandler : IRequestHandler<GenerateDownloadUrlCommand, Result<string>>
{
    private readonly DocumentsDbContext _db;
    private readonly IBlobStorageService _blobStorage;

    public GenerateDownloadUrlCommandHandler(DocumentsDbContext db, IBlobStorageService blobStorage)
    {
        _db = db;
        _blobStorage = blobStorage;
    }

    public async Task<Result<string>> Handle(GenerateDownloadUrlCommand request, CancellationToken ct)
    {
        var document = await _db.Documents
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId, ct);

        if (document is null)
            return Result.Failure<string>("Document not found.");

        // Generate a 1-hour SAS token URL
        var downloadUrl = await _blobStorage.GenerateDownloadUrlAsync(
            document.BlobKey,
            TimeSpan.FromHours(1),
            ct);

        return Result.Success<string>(downloadUrl);
    }
}
