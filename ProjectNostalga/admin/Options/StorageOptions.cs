namespace admin.Options;

public class StorageOptions
{
    public string Provider { get; set; } = "Local";

    public string LocalPath { get; set; } = "/etc/music";

    public SpacesOptions Spaces { get; set; } = new();
}

public class SpacesOptions
{
    public string AccessKey { get; set; } = "";

    public string SecretKey { get; set; } = "";

    public string Bucket { get; set; } = "";

    public string Region { get; set; } = "nyc3";
}
