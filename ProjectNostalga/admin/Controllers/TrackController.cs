

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

[ApiController]
[Route("api/[controller]")]
public class TrackController : ControllerBase
{
    private readonly IKvpStore _kvpStore;
    private readonly IHubContext<TrackHub> _trackHubContext;

    public TrackController( IKvpStore kvpStore, IHubContext<TrackHub> trackHubContext)
    {
        _kvpStore = kvpStore;
        _trackHubContext = trackHubContext;
    }
    
    [HttpGet("get/{trackId}")]
    public Task<Track> GetTrackMetadata(string trackId)
    {
        var metadataJson = _kvpStore.GetValue(trackId);
        var track = JsonSerializer.Deserialize<Track>(metadataJson ?? string.Empty);
        return Task.FromResult(track ?? new Track());
    }

    [HttpPost("set/nowplaying")]
    public async Task SendTrackInfo([FromQuery] string title)
    {
        if ( string.IsNullOrEmpty(title) )
        {
            Console.WriteLine("No title provided");
            return;
        }
        
        Console.WriteLine("Received track change notification: " + title);
        var parts = title.Split(" - ");
        title = parts.FirstOrDefault(p => Guid.TryParse(p, out _)) ?? title;
        if ( string.IsNullOrEmpty(title) )
        {
            Console.WriteLine("No title provided");
            return;
        }
        
        var trackJson = _kvpStore.GetValue(title) ?? string.Empty;
        var track = JsonSerializer.Deserialize<Track>(trackJson);
        var nowPlayingInfo = new NowPlayingInfo
        {
            Track = track ?? new Track { Title = title },
            PlayedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        _kvpStore.SetValue("nowplaying", JsonSerializer.Serialize(nowPlayingInfo));
        await _trackHubContext.Clients.All.SendAsync("ReceiveTrackInfo", nowPlayingInfo );
    }
    
    [HttpGet("get/nowplaying")]
    public async Task<NowPlayingInfo> GetNowPlaying()
    {
        Console.WriteLine("Received request for now playing info");
        var nowPlayingJson = _kvpStore.GetValue("nowplaying");
        if (string.IsNullOrEmpty(nowPlayingJson))
        {
            Console.WriteLine("No now playing info found");
            return new NowPlayingInfo();
        }
        var nowPlayingInfo = JsonSerializer.Deserialize<NowPlayingInfo>(nowPlayingJson);
        if (nowPlayingInfo == null)
        {
            Console.WriteLine("Failed to deserialize now playing info");
            return new NowPlayingInfo();
        }
        return nowPlayingInfo;
    }
}