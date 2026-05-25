using MediatR;
using TheLeague.Modules.Communications.Application.Dtos;
using TheLeague.Modules.Communications.Domain;
using TheLeague.Modules.Communications.Infrastructure.Persistence;
using TheLeague.Modules.Communications.Infrastructure.Providers;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Communications.Application.Commands;

public record SendEmailCommand(
    string RecipientEmail,
    Guid? RecipientMemberId,
    string TemplateType,
    string Subject,
    string Body
) : IRequest<Result<EmailLogDto>>;

public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand, Result<EmailLogDto>>
{
    private readonly CommunicationsDbContext _db;
    private readonly IEmailProvider _emailProvider;
    private readonly ITenantService _tenantService;

    public SendEmailCommandHandler(CommunicationsDbContext db, IEmailProvider emailProvider, ITenantService tenantService)
    {
        _db = db;
        _emailProvider = emailProvider;
        _tenantService = tenantService;
    }

    public async Task<Result<EmailLogDto>> Handle(SendEmailCommand request, CancellationToken cancellationToken)
    {
        if (_tenantService.CurrentTenantId is null)
            return Result.Failure<EmailLogDto>("Tenant context is required.");

        var result = await _emailProvider.SendAsync(request.RecipientEmail, request.Subject, request.Body, cancellationToken);

        var status = result.Success ? "Sent" : "Failed";
        var log = EmailLog.Create(
            _tenantService.CurrentTenantId.Value,
            request.RecipientMemberId,
            request.RecipientEmail,
            request.TemplateType,
            request.Subject,
            status
        );

        if (!result.Success)
            log.MarkFailed(result.ErrorMessage ?? "Unknown error");

        _db.EmailLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new EmailLogDto(
            log.Id, log.RecipientMemberId, log.RecipientEmail, log.TemplateType,
            log.Subject, log.Status, log.SentAt, log.DeliveredAt, log.FailureReason
        ));
    }
}
