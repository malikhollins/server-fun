meta = ref([("initial", "boot")])

audio_source = ref(blank())

playlistSource = playlist("/etc/music", reload_mode="watch")

radio = fallback(track_sensitive=false, [playlistSource, blank()])

audio_source := radio

output.icecast(%mp3, host="icecast", port=8000, password="hackme", mount="/stream", audio_source())

