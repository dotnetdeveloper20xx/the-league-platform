using MediatR;
using TheLeague.Modules.Communications.Application.Dtos;
using TheLeague.Modules.Communications.Domain;
using TheLeague.Modules.Communications.Infrastructure.Persistence;
using TheLeague.Modules.Communications.Infrastructure.Providers;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Communications.Application.Commands;

public record SendSmsCommand(
    string RecipientPhone,
    Guid? RecipientMemberId,
    string Message
) : IRequest<Result<SmsLogDto>>;

public class SendSmsCommandHandler : IRequestHandler<SendSmsCommand, Result<SmsLogDto>>
{
    private readonly CommunicationsDbContext _db;
    private readonly ISmsProvider _smsProvider;
    private readonly ITenantService _tenantService;

    public SendSmsCommandHandler(CommunicationsDbContext db, ISmsProvider smsProvider, ITenantService tenantService)
    {
        _db = db;
        _smsProvider = smsProvider;
        _tenantService = tenantService;
    }

    public async Task<Result<SmsLogDto>> Handle(SendSmsCommand request, CancellationToken cancellationToken)
    {
        if (_tenantService.CurrentTenantId is null)
            return Result.Failure<SmsLogDto>("Tenant context is required.");

        if (request.Message.Length > SmsLog.MaxMessageLength)
            return Result.Failure<SmsLogDto>($"SMS message must not exceed {SmsLog.MaxMessageLength} characters.");

        var result = await _smsProvider.SendAsync(request.RecipientPhone, request.Message, cancellationToken);

        var status = result.Success ? "Sent" : "Failed";
        var log = SmsLog.Create(
            _tenantService.CurrentTenantId.Value,
            request.RecipientMemberId,
            request.RecipientPhone,
            request.Message,
            status
        );

        if (!result.Success)
            log.MarkFailed(result.ErrorMessage ?? "Unknown error");

        _db.SmsLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new SmsLogDto(
            log.Id, log.RecipientMemberId, log.RecipientPhone, log.Message,
            log.Status, log.SentAt, log.FailureReason
        ));
    }
}
