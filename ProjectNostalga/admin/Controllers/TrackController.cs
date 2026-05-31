

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

[ApiController]
[Route("api/[controller]")]
public class TrackController : ControllerBase
{
    private readonly IKvpStore _kvpStore;

    public TrackController( IKvpStore kvpStore )
    {
        _kvpStore = kvpStore;
    }
    
    [HttpGet("get/{trackId}")]
    public Task<Track> GetTrackMetadata(string trackId)
    {
        var metadataJson = _kvpStore.GetValue(trackId);
        var track = JsonSerializer.Deserialize<Track>(metadataJson ?? string.Empty);
        return Task.FromResult(track ?? new Track());
    }
}