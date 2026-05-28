

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

[ApiController]
[Route("api/[controller]")]
public class LiquidSoapController : ControllerBase
{
    private readonly IHubContext<TrackChangeHub> _hubContext;

    public LiquidSoapController(IHubContext<TrackChangeHub> hubContext)
    {
        _hubContext = hubContext;
    }

    [HttpPost("notifytrackchanged")]
    public async Task<IActionResult> NotifyTrackChange([FromBody] JsonElement metadataJson)
    {
        string jsonString = metadataJson.GetRawText();
        await _hubContext.Clients.All.SendAsync("TrackChanged", jsonString);
        return Ok();
    }
}