using System.Runtime.InteropServices;
using System.Text.Json;
using admin.Options;
using Microsoft.Extensions.Options;

public class TrackStore : ITrackStore
{
    private readonly IKvpStore _kvpStore;
    private readonly string _uploadsDirectory;

    public TrackStore( IKvpStore kvpStore, IOptions<StorageOptions> options )
    {
        _kvpStore = kvpStore;
        _uploadsDirectory = options.Value.LocalPath;
    }

    public void UploadTrack(Track track, FileInfo audioFile)
    {
        Console.WriteLine($"Uploading track: {track.Title} by {track.Artist} with file {audioFile.FullName}");
        
        // Fix extension if needed
        var fixedPath = audioFile.FullName;

        if (audioFile.Extension.Equals(".tmp", StringComparison.OrdinalIgnoreCase))
        {
            fixedPath = Path.ChangeExtension(audioFile.FullName, ".mp3");
            File.Move(audioFile.FullName, fixedPath, overwrite: true);
            audioFile = new FileInfo(fixedPath);
        }

        // Update audio file metadata using TagLibSharp
        using var tFile = TagLib.File.Create(new FileAbstraction(audioFile.FullName));
        tFile.Tag.Title = $"{track.TrackId} - {track.Title}";
        tFile.Tag.Performers = [track.Artist ?? "Unknown Artist"];
        tFile.Tag.Album = track.Album ?? "Unknown Album";
        tFile.Save();

        Console.WriteLine($"{tFile.Tag.Title} - {tFile.Tag.Performers.FirstOrDefault()} - {tFile.Tag.Album}");

        Console.WriteLine($"Saved metadata for {audioFile.FullName}");

        // update duration
        track.Duration = tFile.Properties.Duration.TotalSeconds;

        Console.WriteLine($"Updated duration for {audioFile.FullName}");

        // save track info
       _kvpStore.SetValue(track.TrackId.ToString(), JsonSerializer.Serialize(track));

        Console.WriteLine($"Saved track info for {audioFile.FullName}");

        // move file to tracks directory to trigger liquidsoap processing
        if (!Directory.Exists(_uploadsDirectory))
        {
            Console.WriteLine("Creating upload directory");
            Directory.CreateDirectory(_uploadsDirectory);
        }
        
        var filePath = Path.Combine(_uploadsDirectory, audioFile.Name);

        try
        {
            File.Copy(audioFile.FullName, filePath, overwrite: true);

            Console.WriteLine($"Copy Succeded. Exists after copy: {File.Exists(filePath)}");

            if ( RuntimeInformation.IsOSPlatform(OSPlatform.Linux) )
            {
                File.SetUnixFileMode(filePath,
                    UnixFileMode.UserRead |
                    UnixFileMode.UserWrite |
                    UnixFileMode.GroupRead |
                    UnixFileMode.GroupWrite |
                    UnixFileMode.OtherRead);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}