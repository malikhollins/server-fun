meta = ref([("initial", "boot")])

audio_source = ref(blank())

def notify_track_change(m)
  json_data = json.stringify(meta["title"])
  ignore(http.post(
    headers=[("Content-Type", "application/json")],
    data=json_data,
    "http://admin:5179/api/Track/set/nowplaying"
  ))
end

playlistSource = playlist("/etc/music", reload_mode="watch")

radio = fallback(track_sensitive=false, [playlistSource, blank()])

radio.on_metadata(notify_track_change, radio)

audio_source := radio

output.icecast(%mp3, host="icecast", port=8000, password="hackme", mount="/stream", audio_source())

