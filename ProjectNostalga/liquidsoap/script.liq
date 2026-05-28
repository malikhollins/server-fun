meta = ref([("initial", "boot")])

audio_source = ref(blank())

def add_track_info_to_metadata(m)
  if audio_source().is_ready() then
    m = list.assoc.remove.all("elapsed", m)
    m = list.append(m, [("elapsed", string.float(audio_source().elapsed()))])
    m = list.assoc.remove.all("duration", m)
    m = list.append(m, [("duration", string.float(audio_source().duration()))])
    m 
  else
    m
  end
end

def notify_track_change(m)
  meta := add_track_info_to_metadata(m)
  json_data = json.stringify(meta())
  ignore(http.post(
    headers=[("Content-Type", "application/json")],
    data=json_data,
    "http://admin:5179/api/LiquidSoap/notifytrackchanged"
  ))
end

# Expose metadata via HTTP
def getMetadata(_)
  tmpMeta = add_track_info_to_metadata(meta())
  http.response(
    status_code=200,
    headers=[("Content-Type", "application/json")],
    data=json.stringify(tmpMeta)
  )
end

harbor.http.register.simple(port=8080, method="GET", "/metadata", getMetadata)

playlistSource = playlist("/etc/music", reload_mode="watch")

radio = fallback(track_sensitive=false, [playlistSource, blank()])

audio_source := radio

radio.on_metadata(synchronous=false, notify_track_change)

output.icecast(%mp3, host="icecast", port=8000, password="hackme", mount="/stream", audio_source())

