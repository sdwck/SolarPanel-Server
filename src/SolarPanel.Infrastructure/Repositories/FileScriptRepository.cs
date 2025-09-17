using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SolarPanel.Core.Interfaces;
using SolarPanel.Infrastructure.Options;

namespace SolarPanel.Infrastructure.Repositories;

public class FileScriptRepository : IScriptRepository
{
    private readonly IHostEnvironment _environment;
    private readonly RemoteScriptOptions _options;

    public FileScriptRepository(IHostEnvironment environment, IOptions<RemoteScriptOptions> options)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public Task<string?> GetScriptPathAsync(string scriptId)
    {
        var scriptPath = Path.Combine(_environment.ContentRootPath, _options.ScriptRelativePath ?? "script.js");
        return Task.FromResult<string?>(scriptPath);
    }

    public Task<bool> ScriptExistsAsync(string scriptPath)
    {
        return Task.FromResult(File.Exists(scriptPath));
    }

    public Task<DateTime> GetLastWriteTimeUtcAsync(string scriptPath)
    {
        return Task.FromResult(File.GetLastWriteTimeUtc(scriptPath));
    }
}