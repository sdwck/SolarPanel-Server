using System.Security.Cryptography;
using System.Text;
using SolarPanel.Application.Interfaces;

namespace SolarPanel.Infrastructure.Services;

public class FileHashService : IFileHashService
{
    private const int BufferSize = 81920;

    public async Task<(byte[] Sha256Hash, byte[] HmacHash)> ComputeHashesAsync(
        string filePath, 
        string secret, 
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentNullException.ThrowIfNull(secret);

        var secretBytes = Encoding.UTF8.GetBytes(secret);

        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var sha256 = SHA256.Create();
        using var hmac = new HMACSHA256(secretBytes);

        var buffer = new byte[BufferSize];
        int bytesRead;

        while ((bytesRead = await fileStream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
            hmac.TransformBlock(buffer, 0, bytesRead, null, 0);
        }

        sha256.TransformFinalBlock([], 0, 0);
        hmac.TransformFinalBlock([], 0, 0);

        return (sha256.Hash ?? [], hmac.Hash ?? []);
    }
}