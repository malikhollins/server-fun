audio_source = ref(blank())

def notify_track_change(m)
  log("track change: #{m['title']}")
  json_data = json.stringify(m["title"])
  ignore(http.post(http.post("http://admin:5179/api/Track/set/nowplaying?title=#{m['title']}")))
end

playlistSource = playlist("/etc/music", reload_mode="watch")

radio = fallback(track_sensitive=false, [playlistSource, blank()])

radio.on_metadata(synchronous=false, notify_track_change)

audio_source := radio

output.icecast(%mp3, host="icecast", port=8000, password="hackme", mount="/stream", audio_source())