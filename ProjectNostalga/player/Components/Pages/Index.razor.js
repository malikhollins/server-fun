import IcecastMetadataPlayer from "https://esm.sh/icecast-metadata-player";

window.startIcecastPlayer = function (url, dotnetRef) {

    console.log(`Starting Icecast player with URL: ${url}`);
    const player = new IcecastMetadataPlayer(url, 
    {
        onMetadata: (metadata) => {
            dotnetRef.invokeMethodAsync("OnMetadataChanged", metadata.StreamTitle);
        }
    });
    player.play();
}

window.setVolume = function (volume) {
    const audio = document.getElementById("audio");
    if (audio) audio.volume = volume;
};

window.playAudio = function () {
    const audio = document.getElementById("audio");
    if (audio) audio.play();
};

window.pauseAudio = function () {
    const audio = document.getElementById("audio");
    if (audio) audio.pause();
};


