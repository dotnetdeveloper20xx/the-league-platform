using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Members.Application.Dtos;
using TheLeague.Modules.Members.Infrastructure.Persistence;

namespace TheLeague.Modules.Members.Application.Queries;

public record GetCustomFieldsQuery() : IRequest<List<CustomFieldDefinitionDto>>;

public class GetCustomFieldsQueryHandler : IRequestHandler<GetCustomFieldsQuery, List<CustomFieldDefinitionDto>>
{
    private readonly MembersDbContext _db;

    public GetCustomFieldsQueryHandler(MembersDbContext db)
    {
        _db = db;
    }

    public async Task<List<CustomFieldDefinitionDto>> Handle(GetCustomFieldsQuery request, CancellationToken cancellationToken)
    {
        return await _db.CustomFieldDefinitions
            .AsNoTracking()
            .OrderBy(f => f.DisplayOrder)
            .Select(f => new CustomFieldDefinitionDto(
                f.Id, f.Name, f.FieldType, f.IsRequired, f.Options, f.DisplayOrder
            ))
            .ToListAsync(cancellationToken);
    }
}
