playlist_url = environment.get("PLAYLIST_URL", default="/etc/music")
icecast_host = environment.get("ICECAST_HOST", default="icecast")
icecast_port = int_of_string(environment.get("ICECAST_PORT", default="8000"))
icecast_password = environment.get("ICECAST_SOURCE_PASSWORD", default="hackme")
icecast_mount = environment.get("ICECAST_MOUNT", default="/stream")
admin_base_url = environment.get("ADMIN_BASE_URL", default="http://admin:5179")

audio_source = ref(blank())

def notify_track_change(m)
  log("track change: #{m['title']}")
  url = "#{admin_base_url}/api/Track/set/nowplaying?title=#{m['title']}"
  ignore(http.post(url))
end

playlistSource = playlist(playlist_url, reload_mode="watch")

radio = fallback(track_sensitive=false, [playlistSource, blank()])

radio.on_metadata(synchronous=false, notify_track_change)

audio_source := radio

output.icecast(%mp3, host=icecast_host, port=icecast_port, password=icecast_password, mount=icecast_mount, audio_source())
