namespace TheLeague.Modules.Documents.Infrastructure.Services;

public interface IImageOptimizer
{
    Task<Stream> ResizeProfilePhotoAsync(Stream input, CancellationToken ct);
    Task<Stream> GenerateThumbnailAsync(Stream input, CancellationToken ct);
}

/// <summary>
/// Placeholder image optimizer for profile photo resize (300x300) and thumbnail (150x150).
/// In production, this would use a library like SkiaSharp or ImageSharp.
/// </summary>
public class ImageOptimizer : IImageOptimizer
{
    public const int ProfilePhotoSize = 300;
    public const int ThumbnailSize = 150;
    public const int QualityPercent = 80;

    public Task<Stream> ResizeProfilePhotoAsync(Stream input, CancellationToken ct)
    {
        // Placeholder: In production, resize to 300x300 at 80% quality
        // using SkiaSharp or SixLabors.ImageSharp
        var output = new MemoryStream();
        input.CopyTo(output);
        output.Position = 0;
        return Task.FromResult<Stream>(output);
    }

    public Task<Stream> GenerateThumbnailAsync(Stream input, CancellationToken ct)
    {
        // Placeholder: In production, resize to 150x150 at 80% quality
        // using SkiaSharp or SixLabors.ImageSharp
        var output = new MemoryStream();
        input.CopyTo(output);
        output.Position = 0;
        return Task.FromResult<Stream>(output);
    }
}
