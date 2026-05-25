using MediatR;
using TheLeague.Modules.Communications.Application.Dtos;
using TheLeague.Modules.Communications.Domain;
using TheLeague.Modules.Communications.Infrastructure.Persistence;
using TheLeague.Modules.Communications.Infrastructure.Providers;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Communications.Application.Commands;

public record BulkRecipient(string Email, Guid? MemberId, bool OptedOut);

public record SendBulkCampaignCommand(
    string Name,
    string Subject,
    string Body,
    string? TargetSegment,
    List<BulkRecipient> Recipients
) : IRequest<Result<BulkEmailCampaignDto>>;

public class SendBulkCampaignCommandHandler : IRequestHandler<SendBulkCampaignCommand, Result<BulkEmailCampaignDto>>
{
    private readonly CommunicationsDbContext _db;
    private readonly IEmailProvider _emailProvider;
    private readonly ITenantService _tenantService;

    public SendBulkCampaignCommandHandler(CommunicationsDbContext db, IEmailProvider emailProvider, ITenantService tenantService)
    {
        _db = db;
        _emailProvider = emailProvider;
        _tenantService = tenantService;
    }

    public async Task<Result<BulkEmailCampaignDto>> Handle(SendBulkCampaignCommand request, CancellationToken cancellationToken)
    {
        if (_tenantService.CurrentTenantId is null)
            return Result.Failure<BulkEmailCampaignDto>("Tenant context is required.");

        if (request.Recipients.Count > BulkEmailCampaign.MaxRecipients)
            return Result.Failure<BulkEmailCampaignDto>($"Maximum {BulkEmailCampaign.MaxRecipients} recipients allowed per campaign.");

        var campaign = BulkEmailCampaign.Create(
            _tenantService.CurrentTenantId.Value,
            request.Name,
            request.Subject,
            request.Body,
            request.TargetSegment
        );

        // Separate opted-out recipients
        var eligibleRecipients = request.Recipients.Where(r => !r.OptedOut).ToList();
        var excludedCount = request.Recipients.Count - eligibleRecipients.Count;

        campaign.StartSending(request.Recipients.Count, excludedCount);

        _db.BulkEmailCampaigns.Add(campaign);
        await _db.SaveChangesAsync(cancellationToken);

        // Send emails to eligible recipients
        foreach (var recipient in eligibleRecipients)
        {
            var result = await _emailProvider.SendAsync(recipient.Email, request.Subject, request.Body, cancellationToken);

            if (result.Success)
            {
                campaign.IncrementSent();
                var log = EmailLog.Create(
                    _tenantService.CurrentTenantId.Value,
                    recipient.MemberId,
                    recipient.Email,
                    "BulkCampaign",
                    request.Subject,
                    "Sent"
                );
                _db.EmailLogs.Add(log);
            }
            else
            {
                campaign.IncrementFailed();
                var log = EmailLog.Create(
                    _tenantService.CurrentTenantId.Value,
                    recipient.MemberId,
                    recipient.Email,
                    "BulkCampaign",
                    request.Subject,
                    "Failed"
                );
                log.MarkFailed(result.ErrorMessage ?? "Unknown error");
                _db.EmailLogs.Add(log);
            }
        }

        campaign.MarkCompleted();
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new BulkEmailCampaignDto(
            campaign.Id, campaign.Name, campaign.Subject, campaign.Body,
            campaign.TargetSegment, campaign.TotalRecipients, campaign.SentCount,
            campaign.FailedCount, campaign.ExcludedCount, campaign.Status,
            campaign.CreatedAt, campaign.CompletedAt
        ));
    }
}
