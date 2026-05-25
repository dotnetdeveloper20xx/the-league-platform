using MediatR;
using TheLeague.Modules.Communications.Application.Dtos;
using TheLeague.Modules.Communications.Domain;
using TheLeague.Modules.Communications.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Communications.Application.Commands;

public record CreateTemplateCommand(
    string Name,
    string TemplateType,
    string Subject,
    string Body
) : IRequest<Result<TemplateDto>>;

public class CreateTemplateCommandHandler : IRequestHandler<CreateTemplateCommand, Result<TemplateDto>>
{
    private readonly CommunicationsDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateTemplateCommandHandler(CommunicationsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<TemplateDto>> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        if (_tenantService.CurrentTenantId is null)
            return Result.Failure<TemplateDto>("Tenant context is required.");

        if (!CommunicationTemplate.ValidTemplateTypes.Contains(request.TemplateType))
            return Result.Failure<TemplateDto>($"Invalid template type. Valid types: {string.Join(", ", CommunicationTemplate.ValidTemplateTypes)}");

        var template = CommunicationTemplate.Create(
            _tenantService.CurrentTenantId.Value,
            request.Name,
            request.TemplateType,
            request.Subject,
            request.Body
        );

        _db.Templates.Add(template);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(template));
    }

    private static TemplateDto MapToDto(CommunicationTemplate t) => new(
        t.Id, t.Name, t.TemplateType, t.Subject, t.Body, t.IsActive, t.CreatedAt, t.UpdatedAt
    );
}
