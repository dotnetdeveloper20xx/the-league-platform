namespace TheLeague.Modules.Documents.Infrastructure.Services;

public interface IBlobStorageService
{
    Task<string> UploadAsync(string containerPath, Stream content, string contentType, CancellationToken ct);
    Task<string> GenerateDownloadUrlAsync(string blobKey, TimeSpan expiry, CancellationToken ct);
    Task DeleteAsync(string blobKey, CancellationToken ct);
    Task<long> GetClubStorageUsageAsync(Guid clubId, CancellationToken ct);
}
