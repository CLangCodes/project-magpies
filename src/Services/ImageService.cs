namespace ProjectMagpie.Services;

public interface IImageService
{
    /// <summary>
    /// Saves the given file content under wwwroot/ThingImages.
    /// </summary>
    /// <param name="fileStream">The file content to save.</param>
    /// <param name="fileName">The original file name (used to determine the extension).</param>
    /// <param name="thingId">The owning Thing's Id, used to build a unique file name.</param>
    /// <returns>The web-relative path (e.g. "ThingImages/xxx.png") to store on Thing.ImageUrl.</returns>
    Task<string> UploadImage(Stream fileStream, string fileName, int thingId);
    Task DeleteImage(string FilePath);
}

public class ImageService(IWebHostEnvironment Env) : IImageService
{
    async Task<string> IImageService.UploadImage(Stream fileStream, string fileName, int thingId)
    {
        var ext = Path.GetExtension(fileName);
        var newFileName = $"{thingId}_{DateTime.UtcNow.Ticks}{ext}";
        var folder = Path.Combine(Env.WebRootPath, "ThingImages");
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, newFileName);

        await using var stream = File.Create(path);
        await fileStream.CopyToAsync(stream);

        return $"ThingImages/{newFileName}";
    }

    async Task IImageService.DeleteImage(string FileName)
    {
        if (string.IsNullOrEmpty(FileName))
            return;

        var filePath = Path.Combine(Env.WebRootPath, FileName);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }
}