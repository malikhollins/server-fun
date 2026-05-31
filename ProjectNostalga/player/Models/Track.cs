
public class Track
{
    public Guid TrackId { get; set; } = Guid.NewGuid();
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public double Duration { get; set; }
}