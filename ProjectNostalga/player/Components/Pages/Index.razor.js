let icecastPlayer;

window.startIcecastPlayer = async function (url, dotnetRef) {
    console.log(`Starting Icecast player with URL: ${url}`);

    const audio = document.getElementById("audio");
    if (!audio) {
        console.error("Cannot start Icecast player because audio#audio was not found.");
        return;
    }

    if (!icecastPlayer || icecastPlayer.endpoint !== url) {
        await icecastPlayer?.stop();

        icecastPlayer = new IcecastMetadataPlayer(url, {
            audioElement: audio,
            metadataTypes: ["icy"],
            enableLogging: true,
            onMetadata: (metadata, timestampOffset, timestamp) => {
                dotnetRef.invokeMethodAsync("OnMetadataChanged", metadata.StreamTitle, timestampOffset, timestamp);
            },
            onWarn: (...messages) => console.warn("Icecast player warning:", ...messages),
            onError: (message, error) => console.error("Icecast player error:", message, error),
            onRetry: () => console.warn("Retrying Icecast stream connection..."),
            onStreamStart: () => console.log("Icecast stream started.")
        });
    }

    await icecastPlayer.play();
};

window.setVolume = function (volume) {
    const audio = document.getElementById("audio");
    if (audio) audio.volume = volume;
};

window.playAudio = function () {
    const audio = document.getElementById("audio");
    if (audio) audio.play();
};

window.pauseAudio = async function () {
    if (icecastPlayer) {
        await icecastPlayer.stop();
        return;
    }
    const audio = document.getElementById("audio");
    if (audio) audio.pause();
};


