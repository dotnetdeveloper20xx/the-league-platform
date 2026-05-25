using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Communications.Application.Dtos;
using TheLeague.Modules.Communications.Domain;
using TheLeague.Modules.Communications.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Communications.Application.Commands;

public record UpdateTemplateCommand(
    Guid Id,
    string Name,
    string TemplateType,
    string Subject,
    string Body,
    bool IsActive
) : IRequest<Result<TemplateDto>>;

public class UpdateTemplateCommandHandler : IRequestHandler<UpdateTemplateCommand, Result<TemplateDto>>
{
    private readonly CommunicationsDbContext _db;

    public UpdateTemplateCommandHandler(CommunicationsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<TemplateDto>> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        if (!CommunicationTemplate.ValidTemplateTypes.Contains(request.TemplateType))
            return Result.Failure<TemplateDto>($"Invalid template type. Valid types: {string.Join(", ", CommunicationTemplate.ValidTemplateTypes)}");

        var template = await _db.Templates.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (template is null)
            return Result.Failure<TemplateDto>("Template not found.");

        template.Update(request.Name, request.TemplateType, request.Subject, request.Body, request.IsActive);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new TemplateDto(
            template.Id, template.Name, template.TemplateType, template.Subject,
            template.Body, template.IsActive, template.CreatedAt, template.UpdatedAt
        ));
    }
}
