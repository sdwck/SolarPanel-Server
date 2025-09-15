using SolarPanel.Core.Entities;

namespace SolarPanel.Application.Interfaces;

public interface IScriptService
{
    Task<ScriptMetadata?> GetScriptMetadataAsync(string scriptId, CancellationToken cancellationToken = default);
    Task<bool> IsScriptModifiedAsync(ScriptMetadata metadata, ConditionalRequest conditionalRequest);
}