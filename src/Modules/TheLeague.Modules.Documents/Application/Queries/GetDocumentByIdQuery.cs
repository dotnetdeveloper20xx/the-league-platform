using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Documents.Application.Dtos;
using TheLeague.Modules.Documents.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Documents.Application.Queries;

public record GetDocumentByIdQuery(Guid DocumentId) : IRequest<Result<DocumentDto>>;

public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, Result<DocumentDto>>
{
    private readonly DocumentsDbContext _db;

    public GetDocumentByIdQueryHandler(DocumentsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<DocumentDto>> Handle(GetDocumentByIdQuery request, CancellationToken ct)
    {
        var document = await _db.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId, ct);

        if (document is null)
            return Result.Failure<DocumentDto>("Document not found.");

        var dto = new DocumentDto(
            document.Id,
            document.ClubId,
            document.MemberId,
            document.FileName,
            document.ContentType,
            document.FileSize,
            document.DocumentType,
            document.BlobKey,
            document.UploadedByUserId,
            document.UploadedAt,
            document.ThumbnailBlobKey);

        return Result.Success(dto);
    }
}
