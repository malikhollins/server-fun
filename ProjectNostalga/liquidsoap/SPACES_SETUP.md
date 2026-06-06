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

## Relevant Liquidsoap settings

```
settings.protocol.aws.endpoint := <value>   # Alternative endpoint URL (for non-AWS S3 like DO Spaces)
settings.protocol.aws.region   := <value>
settings.protocol.aws.profile  := <value>
settings.protocol.aws.path     := "aws"      # path to the aws CLI binary
```

The `endpoint` setting is the key that points the protocol at DigitalOcean
Spaces instead of AWS.

## Setup

### 1. `liquidsoap/script.liq`

Add the AWS/S3 configuration near the top, and point the playlist at an `.m3u`
file of `s3://` URIs:

```liquidsoap
settings.protocol.aws.endpoint := environment.get("S3_ENDPOINT", default="https://nyc3.digitaloceanspaces.com")
settings.protocol.aws.region   := environment.get("S3_REGION", default="nyc3")

playlistSource = playlist("s3://my-music-bucket/playlist.m3u", reload=300, reload_mode="seconds")
```

> Note: `reload_mode="watch"` only works on local files. For a remote playlist
> use `reload_mode="seconds"` with a `reload` interval (300s above).

The AWS CLI automatically reads credentials from the `AWS_ACCESS_KEY_ID` and
`AWS_SECRET_ACCESS_KEY` environment variables.

### 2. `liquidsoap/Dockerfile`

Install the AWS CLI:

```dockerfile
FROM savonet/liquidsoap:v2.4.x-latest

USER root
RUN apt-get update && apt-get install -y awscli && rm -rf /var/lib/apt/lists/*

COPY script.liq /etc/liquidsoap/script.liq
CMD ["/etc/liquidsoap/script.liq"]
```

### 3. `compose.yaml`

Remove the local bind mount (`./music:/etc/music`) and add the Spaces
credentials + endpoint. No FUSE-related options needed:

```yaml
  liquidsoap:
    build:
      context: ./liquidsoap
    container_name: liquidsoap
    restart: always
    environment:
      - ICECAST_HOST=icecast
      - ICECAST_PORT=8000
      - ICECAST_SOURCE_PASSWORD=${ICECAST_SOURCE_PASSWORD:-hackme}
      - ICECAST_MOUNT=/stream
      - ADMIN_BASE_URL=http://admin:5179
      # --- DigitalOcean Spaces ---
      - AWS_ACCESS_KEY_ID=${SPACES_KEY}
      - AWS_SECRET_ACCESS_KEY=${SPACES_SECRET}
      - S3_ENDPOINT=https://nyc3.digitaloceanspaces.com
      - S3_REGION=nyc3
    networks:
      - radionetwork
```

### 4. `.env`

```bash
SPACES_KEY=DO00xxxxxxxx
SPACES_SECRET=xxxxxxxx
```

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

### Using `prefix` for bare keys

The `playlist` operator has a `prefix` parameter, documented as useful for
"resolution through a particular protocol." That lets your `.m3u` contain bare
object keys while Liquidsoap prepends the protocol + bucket:

```liquidsoap
playlistSource = playlist(prefix="s3://my-music-bucket/", "playlist.m3u", reload=300, reload_mode="seconds")
```

with `playlist.m3u` containing:

```
tracks/song1.mp3
tracks/song2.mp3
```

### Generating it from the admin app

The admin app already talks to Spaces via FluentStorage
(`admin/Services/MusicStore.cs`), so it's the natural place to (re)write
`playlist.m3u` whenever a track is uploaded or removed. That keeps the playlist
in sync with the bucket contents automatically.

## Local files for development testing

You don't have to hit Spaces while developing — keep the original local
`./music` bind mount and drive the source location from an env var so the same
image works in both environments.

### `script.liq`

The script already reads `PLAYLIST_URL` (defaulting to `/etc/music`). Point it
at a local directory in dev or an `s3://` playlist in prod. Use
`reload_mode="seconds"` so it works for both (a remote `s3://` playlist can't be
`watch`ed):

```liquidsoap
settings.protocol.aws.endpoint := environment.get("S3_ENDPOINT", default="https://nyc3.digitaloceanspaces.com")
settings.protocol.aws.region   := environment.get("S3_REGION", default="nyc3")

playlist_url = environment.get("PLAYLIST_URL", default="/etc/music")
playlistSource = playlist(playlist_url, reload=300, reload_mode="seconds")
```

The `settings.protocol.aws.*` lines are harmless when `PLAYLIST_URL` is a local
path — they only matter once an `s3://` URI is resolved.

### Development (`compose.yaml`)

Local folder, no credentials needed:

```yaml
    volumes:
      - ./music:/etc/music:rw
    environment:
      - PLAYLIST_URL=/etc/music
```

### Production (`compose.yaml`)

Spaces playlist, no bind mount:

```yaml
    environment:
      - PLAYLIST_URL=s3://my-music-bucket/playlist.m3u
      - AWS_ACCESS_KEY_ID=${SPACES_KEY}
      - AWS_SECRET_ACCESS_KEY=${SPACES_SECRET}
      - S3_ENDPOINT=https://nyc3.digitaloceanspaces.com
      - S3_REGION=nyc3
```

> Tip: keep the dev override in a separate `compose.override.yaml` (Docker
> Compose merges it automatically) so prod settings stay in `compose.yaml`.

## Tradeoffs / notes

- Each track is downloaded on demand at play time (small latency on track
  change, then cached in a temp file until done).
- A remote playlist is re-read on the `reload` interval, not instantly.
- Keep the Space `private` and rely on the credentials; no need to make objects
  public.
