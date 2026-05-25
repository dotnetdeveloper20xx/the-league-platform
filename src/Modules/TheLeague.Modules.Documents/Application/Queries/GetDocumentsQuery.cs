using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Documents.Application.Dtos;
using TheLeague.Modules.Documents.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Documents.Application.Queries;

public record GetDocumentsQuery(
    Guid? MemberId = null,
    string? DocumentType = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<List<DocumentDto>>>;

public class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, Result<List<DocumentDto>>>
{
    private readonly DocumentsDbContext _db;

    public GetDocumentsQueryHandler(DocumentsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<DocumentDto>>> Handle(GetDocumentsQuery request, CancellationToken ct)
    {
        var query = _db.Documents.AsNoTracking().AsQueryable();

        if (request.MemberId.HasValue)
            query = query.Where(d => d.MemberId == request.MemberId.Value);

        if (!string.IsNullOrEmpty(request.DocumentType))
            query = query.Where(d => d.DocumentType == request.DocumentType);

        var documents = await query
            .OrderByDescending(d => d.UploadedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new DocumentDto(
                d.Id,
                d.ClubId,
                d.MemberId,
                d.FileName,
                d.ContentType,
                d.FileSize,
                d.DocumentType,
                d.BlobKey,
                d.UploadedByUserId,
                d.UploadedAt,
                d.ThumbnailBlobKey))
            .ToListAsync(ct);

        return Result.Success(documents);
    }
}
