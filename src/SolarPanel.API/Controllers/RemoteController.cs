using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarPanel.Application.Interfaces;
using SolarPanel.Core.Entities;

namespace SolarPanel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RemoteController : ControllerBase
{
    private readonly IScriptService _scriptService;

    public RemoteController(IScriptService scriptService)
    {
        _scriptService = scriptService ?? throw new ArgumentNullException(nameof(scriptService));
    }

    [HttpGet("{id}/script")]
    public async Task<IActionResult> GetScript(string id, CancellationToken cancellationToken = default)
    {
        var metadata = await _scriptService.GetScriptMetadataAsync(id, cancellationToken);
        
        if (metadata == null)
            return NotFound();

        var conditionalRequest = new ConditionalRequest
        {
            IfNoneMatch = Request.Headers.IfNoneMatch.ToString(),
            IfModifiedSince = Request.Headers.IfModifiedSince.ToString()
        };

        var isModified = await _scriptService.IsScriptModifiedAsync(metadata, conditionalRequest);
        
        SetResponseHeaders(metadata);

        if (!isModified)
            return StatusCode(304);

        return PhysicalFile(metadata.FilePath, "application/javascript");
    }

    private void SetResponseHeaders(ScriptMetadata metadata)
    {
        Response.Headers.ETag = metadata.ETag;
        Response.Headers.LastModified = metadata.LastWriteUtc.ToString("R");
        Response.Headers["X-Signature"] = metadata.Signature;
        Response.Headers["X-Signature-256"] = metadata.Signature;
        Response.Headers.CacheControl = "no-cache, must-revalidate";
    }
}