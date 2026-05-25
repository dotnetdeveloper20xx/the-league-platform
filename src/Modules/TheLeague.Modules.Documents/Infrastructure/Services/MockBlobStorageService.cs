namespace TheLeague.Modules.Documents.Infrastructure.Services;

/// <summary>
/// Mock blob storage service that stores files locally in a temp directory for development.
/// </summary>
public class MockBlobStorageService : IBlobStorageService
{
    private readonly string _basePath;

    public MockBlobStorageService()
    {
        _basePath = Path.Combine(Path.GetTempPath(), "TheLeague", "BlobStorage");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadAsync(string containerPath, Stream content, string contentType, CancellationToken ct)
    {
        var blobKey = $"{containerPath}/{Guid.NewGuid()}";
        var filePath = GetFilePath(blobKey);

        var directory = Path.GetDirectoryName(filePath)!;
        Directory.CreateDirectory(directory);

        await using var fileStream = File.Create(filePath);
        await content.CopyToAsync(fileStream, ct);

        return blobKey;
    }

    public Task<string> GenerateDownloadUrlAsync(string blobKey, TimeSpan expiry, CancellationToken ct)
    {
        var filePath = GetFilePath(blobKey);
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Blob not found: {blobKey}");

        // In development, return a local file path as a mock URL
        var mockUrl = $"https://localhost/mock-blob/{blobKey}?expiry={DateTime.UtcNow.Add(expiry):O}";
        return Task.FromResult(mockUrl);
    }

    public Task DeleteAsync(string blobKey, CancellationToken ct)
    {
        var filePath = GetFilePath(blobKey);
        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    public Task<long> GetClubStorageUsageAsync(Guid clubId, CancellationToken ct)
    {
        var clubPath = Path.Combine(_basePath, clubId.ToString());
        if (!Directory.Exists(clubPath))
            return Task.FromResult(0L);

        var totalSize = Directory.GetFiles(clubPath, "*", SearchOption.AllDirectories)
            .Sum(f => new FileInfo(f).Length);

        return Task.FromResult(totalSize);
    }

    private string GetFilePath(string blobKey)
    {
        return Path.Combine(_basePath, blobKey.Replace('/', Path.DirectorySeparatorChar));
    }
}
