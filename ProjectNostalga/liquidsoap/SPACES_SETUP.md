# Reading the Liquidsoap Playlist from DigitalOcean Spaces

This documents how to have Liquidsoap stream music stored in a DigitalOcean
Space (S3-compatible object storage) instead of a local `./music` folder.

## TL;DR

Liquidsoap 2.4+ has a built-in `s3://` protocol that fetches files via the AWS
CLI, and it supports custom S3 endpoints — so it works with DigitalOcean Spaces
directly.

When Liquidsoap needs a track, it shells out to the AWS CLI, downloads the file
to a temp location, plays it, then cleans it up. Nothing persists a full local
copy of the library.

References:
- [Protocols reference (`s3`)](https://www.liquidsoap.info/doc-2.4.3/protocols.html#s3)
- [Settings reference (AWS protocol)](https://www.liquidsoap.info/doc-2.4.3/settings.html)

## The playlist file

The `playlist` operator "reads a playlist or a directory and play all files" —
where "directory" means a *local* filesystem directory. It has no S3 bucket
listing capability, so to pull from Spaces you provide an actual playlist file
(e.g. `.m3u`) that enumerates the tracks.

Provide an `.m3u` whose lines are absolute `s3://` URIs:

```
s3://my-music-bucket/tracks/song1.mp3
s3://my-music-bucket/tracks/song2.mp3
```

A **remote** playlist file is officially supported (the docs give an explicit
remote-m3u-over-HTTP example), and since `s3://` resolves like any other
request, the `.m3u` can live in the Space itself — or be a local file.

References:
- [`playlist` operator reference](https://www.liquidsoap.info/doc-2.4.3/reference.html) — "Read a playlist or a directory and play all files."
- [Playlist parsers (remote m3u example)](https://www.liquidsoap.info/doc-2.4.3/playlist_parsers.html)
