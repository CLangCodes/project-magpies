namespace ProjectMagpie.Services;

public abstract class ImageValidationException(string message) : Exception(message);
public sealed class UnsupportedImageTypeException(string message) : ImageValidationException(message);
public sealed class InvalidImageContentException(string message) : ImageValidationException(message);
public sealed class ImageTooLargeException(string message) : ImageValidationException(message);

public interface IImageService
{
    /// <summary>
    /// Saves the given file content under wwwroot/ThingImages.
    /// </summary>
    /// <param name="fileStream">The file content to save.</param>
    /// <param name="fileName">The original file name (used to determine the extension).</param>
    /// <param name="thingId">The owning Thing's Id, used to build a unique file name.</param>
    /// <returns>The web-relative path (e.g. "ThingImages/xxx.png") to store on Thing.ImageUrl.</returns>
    /// <exception cref="ImageValidationException">The file's extension/content/size failed validation.</exception>
    Task<string> UploadImage(Stream fileStream, string fileName, int thingId);
    Task DeleteImage(string FilePath);
}

public class ImageService(IWebHostEnvironment Env) : IImageService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = [".png", ".jpg", ".jpeg", ".gif"];

    private static readonly (byte[] Signature, string Extension)[] KnownSignatures =
    [
        ([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], "png"),
        ([0xFF, 0xD8, 0xFF], "jpg"),
        ([0x47, 0x49, 0x46, 0x38, 0x37, 0x61], "gif"),
        ([0x47, 0x49, 0x46, 0x38, 0x39, 0x61], "gif"),
    ];

    async Task<string> IImageService.UploadImage(Stream fileStream, string fileName, int thingId)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new UnsupportedImageTypeException($"'{ext}' is not a supported image type. Allowed types: {string.Join(", ", AllowedExtensions)}.");

        var header = new byte[8];
        var headerBytesRead = await ReadFullyAsync(fileStream, header, 0, header.Length);
        if (!KnownSignatures.Any(known => headerBytesRead >= known.Signature.Length && header.AsSpan(0, known.Signature.Length).SequenceEqual(known.Signature)))
            throw new InvalidImageContentException("The uploaded file's content does not match a supported image format.");

        var newFileName = $"{thingId}_{DateTime.UtcNow.Ticks}{ext}";
        var folder = Path.Combine(Env.WebRootPath, "ThingImages");
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, newFileName);

        try
        {
            await using (var destination = File.Create(path))
            {
                await destination.WriteAsync(header.AsMemory(0, headerBytesRead));

                var buffer = new byte[81920];
                long totalBytes = headerBytesRead;
                int bytesRead;
                while ((bytesRead = await fileStream.ReadAsync(buffer)) > 0)
                {
                    totalBytes += bytesRead;
                    if (totalBytes > MaxFileSizeBytes)
                        throw new ImageTooLargeException($"Image exceeds the maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB.");

                    await destination.WriteAsync(buffer.AsMemory(0, bytesRead));
                }
            }
        }
        catch
        {
            if (File.Exists(path))
                File.Delete(path);
            throw;
        }

        return $"ThingImages/{newFileName}";
    }

    private static async Task<int> ReadFullyAsync(Stream stream, byte[] buffer, int offset, int count)
    {
        var totalRead = 0;
        while (totalRead < count)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset + totalRead, count - totalRead));
            if (read == 0)
                break;
            totalRead += read;
        }
        return totalRead;
    }

    async Task IImageService.DeleteImage(string FileName)
    {
        if (string.IsNullOrEmpty(FileName) || FileName.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            return;

        string imagesRoot, candidatePath;
        try
        {
            imagesRoot = Path.GetFullPath(Path.Combine(Env.WebRootPath, "ThingImages") + Path.DirectorySeparatorChar);
            candidatePath = Path.GetFullPath(Path.Combine(Env.WebRootPath, FileName));
        }
        catch (Exception ex) when (ex is ArgumentException or PathTooLongException or NotSupportedException)
        {
            return;
        }

        if (!candidatePath.StartsWith(imagesRoot, StringComparison.OrdinalIgnoreCase))
            return;

        if (File.Exists(candidatePath))
            File.Delete(candidatePath);
    }
}
