namespace SolarPanel.Core.Interfaces;

public interface IScriptRepository
{
    Task<string?> GetScriptPathAsync(string scriptId);
    Task<bool> ScriptExistsAsync(string scriptPath);
    Task<DateTime> GetLastWriteTimeUtcAsync(string scriptPath);
}