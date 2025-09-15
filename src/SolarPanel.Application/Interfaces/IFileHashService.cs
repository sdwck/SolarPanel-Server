namespace SolarPanel.Application.Interfaces;

public interface IFileHashService
{
    Task<(byte[] Sha256Hash, byte[] HmacHash)> ComputeHashesAsync(string filePath, string secret, CancellationToken cancellationToken = default);
}