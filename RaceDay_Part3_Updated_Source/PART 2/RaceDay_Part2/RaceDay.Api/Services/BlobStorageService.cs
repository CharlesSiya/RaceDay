using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace RaceDay.Api.Services;

public interface IBlobStorageService
{
    Task<string> UploadAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
}

public sealed class BlobStorageService(IConfiguration configuration) : IBlobStorageService
{
    private readonly string? _connectionString = configuration["AzureBlobStorage:ConnectionString"];
    private readonly string _containerName = configuration["AzureBlobStorage:ContainerName"] ?? "raceday-images";

    public async Task<string> UploadAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new InvalidOperationException("Azure Blob Storage is not configured. Set AzureBlobStorage:ConnectionString.");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(extension)) throw new ArgumentException("Only JPG, PNG and WEBP images are allowed.");
        if (file.Length == 0 || file.Length > 5 * 1024 * 1024)
            throw new ArgumentException("Image must be between 1 byte and 5 MB.");

        var service = new BlobServiceClient(_connectionString);
        var container = service.GetBlobContainerClient(_containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        var blobName = $"{folder}/{Guid.NewGuid():N}{extension}";
        var blob = container.GetBlobClient(blobName);
        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType }, cancellationToken: cancellationToken);

        // Return a time-limited read SAS where the connection string contains an account key.
        if (blob.CanGenerateSasUri)
        {
            var sas = new BlobSasBuilder { BlobContainerName = _containerName, BlobName = blobName, Resource = "b", ExpiresOn = DateTimeOffset.UtcNow.AddDays(30) };
            sas.SetPermissions(BlobSasPermissions.Read);
            return blob.GenerateSasUri(sas).ToString();
        }
        return blob.Uri.ToString();
    }
}
