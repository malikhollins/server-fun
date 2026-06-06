using FluentStorage;
using FluentStorage.Blobs;

public class MusicStore : IMusicStore
{
    public async Task UploadFileAsync( FileInfo file )
    {
        IBlobStorage storage = StorageFactory.Blobs.DigitalOceanSpaces( "hackme", "hackme", "hackme", "hackme" );
        await storage.WriteFileAsync( file.FullName, file.FullName );
    }
}