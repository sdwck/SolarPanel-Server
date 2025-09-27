using System.Globalization;
using Microsoft.Extensions.Options;
using SolarPanel.Application.Interfaces;
using SolarPanel.Core.Entities;
using SolarPanel.Core.Interfaces;
using SolarPanel.Infrastructure.Options;

namespace SolarPanel.Infrastructure.Services;

public class ScriptService : IScriptService
{
    private readonly IScriptRepository _scriptRepository;
    private readonly IFileHashService _fileHashService;
    private readonly string _secret;

    public ScriptService(
        IScriptRepository scriptRepository,
        IFileHashService fileHashService,
        IOptions<RemoteScriptOptions> options)
    {
        _scriptRepository = scriptRepository ?? throw new ArgumentNullException(nameof(scriptRepository));
        _fileHashService = fileHashService ?? throw new ArgumentNullException(nameof(fileHashService));
        _secret = options?.Value?.Secret ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<ScriptMetadata?> GetScriptMetadataAsync(string scriptId,
        CancellationToken cancellationToken = default)
    {
        var scriptPath = await _scriptRepository.GetScriptPathAsync(scriptId);

        if (string.IsNullOrEmpty(scriptPath) || !await _scriptRepository.ScriptExistsAsync(scriptPath))
            return null;

        var lastWriteUtc = await _scriptRepository.GetLastWriteTimeUtcAsync(scriptPath);
        var (shaHash, hmacHash) = await _fileHashService.ComputeHashesAsync(scriptPath, _secret, cancellationToken);

        return new ScriptMetadata
        {
            ETag = $"\"{Convert.ToHexString(shaHash).ToLowerInvariant()}\"",
            LastWriteUtc = lastWriteUtc,
            Signature = Convert.ToHexString(hmacHash).ToLowerInvariant(),
            FilePath = scriptPath
        };
    }

    public Task<bool> IsScriptModifiedAsync(ScriptMetadata metadata, ConditionalRequest conditionalRequest)
    {
        if (!string.IsNullOrWhiteSpace(conditionalRequest.IfNoneMatch))
        {
            var normalizedETag = NormalizeETag(metadata.ETag);
            var tokens = conditionalRequest.IfNoneMatch.Split(',').Select(t => t.Trim());

            foreach (var token in tokens)
            {
                if (token == "*" || NormalizeETag(token) == normalizedETag)
                    return Task.FromResult(false);
            }
        }
        else if (!string.IsNullOrWhiteSpace(conditionalRequest.IfModifiedSince))
        {
            if (DateTimeOffset.TryParse(conditionalRequest.IfModifiedSince, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var since))
            {
                var lastWrite = new DateTimeOffset(metadata.LastWriteUtc, TimeSpan.Zero);
                if (lastWrite <= since)
                    return Task.FromResult(false);
            }
        }

        return Task.FromResult(true);
    }

    private static string NormalizeETag(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return raw;

        var normalized = raw.Trim();

        if (normalized.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
            normalized = normalized[2..].Trim();

        if (normalized is ['"', _, ..] && normalized[^1] == '"')
            normalized = normalized.Substring(1, normalized.Length - 2);

        return normalized;
    }
}