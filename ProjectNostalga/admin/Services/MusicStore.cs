using admin.Options;
using FluentStorage;
using FluentStorage.Blobs;
using Microsoft.Extensions.Options;

public class MusicStore : IMusicStore
{
    private readonly StorageOptions _options;

    public MusicStore( IOptions<StorageOptions> options )
    {
        _options = options.Value;
    }

    public async Task UploadFileAsync( FileInfo file )
    {
        SpacesOptions spaces = _options.Spaces;
        IBlobStorage storage = StorageFactory.Blobs.DigitalOceanSpaces(
            spaces.AccessKey, spaces.SecretKey, spaces.Bucket, spaces.Region );

        await storage.WriteFileAsync( $"tracks/{file.Name}", file.FullName );
    }
}
