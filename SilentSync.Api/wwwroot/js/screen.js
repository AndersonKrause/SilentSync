const params = new URLSearchParams(location.search);
const ROOM = (params.get("room") || "").trim().toUpperCase();

const video = document.getElementById("video");
const roomText = document.getElementById("roomText");
const qrImg = document.getElementById("qrImg");
const qrHint = document.getElementById("qrHint");
const hud = document.querySelector(".hud");

const startOverlay = document.getElementById("startOverlay");
const startBtn = document.getElementById("startBtn");

let conn = null;
let currentVideoUrl = "";
let lastState = null;

function debug(...args) {
    console.log("[screen]", ...args);
}

function setRoomText() {
    roomText.textContent = `${t("roomLabel")}: ${ROOM || t("emptyValue")}`;
}

function getRegisterUrl() {
    if (!ROOM) {
        return `${location.origin}/pages/register.html`;
    }

    return `${location.origin}/pages/register.html?room=${encodeURIComponent(ROOM)}`;
}

function configureQr() {
    if (!ROOM) {
        qrImg.style.display = "none";
        qrHint.textContent = t("openScreenWithRoom");
        return;
    }

    const registerUrl = getRegisterUrl();

    qrImg.src =
        "https://api.qrserver.com/v1/create-qr-code/?size=320x320&data=" +
        encodeURIComponent(registerUrl);

    qrImg.style.display = "block";

    if (location.hostname === "localhost" || location.hostname === "127.0.0.1") {
        qrHint.textContent = t("screenQrHintLocalhost");
    } else {
        qrHint.textContent = t("screenQrHintDefault");
    }
}

function validateRoomOrWarn() {
    if (ROOM) return true;

    alert(t("openScreenWithRoom"));
    debug("ROOM missing", {
        href: location.href,
        search: location.search,
        room: ROOM
    });

    return false;
}

function showHudFiveMinutes() {
    hud.classList.remove("hidden");

    setTimeout(() => {
        hud.classList.add("hidden");
    }, 300000);
}

async function enterFullscreen() {
    const el = document.documentElement;

    if (el.requestFullscreen) {
        await el.requestFullscreen();
    } else if (el.webkitRequestFullscreen) {
        await el.webkitRequestFullscreen();
    }
}

function absUrl(u) {
    try {
        return new URL(u, location.origin).toString();
    } catch {
        return u;
    }
}

async function applyState(state) {
    if (!state) return;
    if (!ROOM) return;

    if ((state.roomCode || "").toUpperCase() !== ROOM) {
        debug("Ignoring state from another room", {
            expected: ROOM,
            received: state.roomCode
        });
        return;
    }

    lastState = state;

    const videoUrl = absUrl(state.videoUrl);

    if (videoUrl && videoUrl !== currentVideoUrl) {
        currentVideoUrl = videoUrl;
        video.src = videoUrl;
        video.load();
        debug("Video source updated", videoUrl);
    }

    if (state.isPlaying) {
        try {
            await video.play();
        } catch (e) {
            debug("Autoplay/play failed", e?.message || e);
        }
    } else {
        video.pause();
    }

    const target = Math.max(0, state.positionMs || 0) / 1000;

    if (video.readyState >= 1) {
        if (Math.abs(video.currentTime - target) > 0.35) {
            video.currentTime = target;
            debug("Video seek applied", {
                current: video.currentTime,
                target
            });
        }
    }
}

async function joinAndPull() {
    if (!ROOM) return;

    await conn.invoke("JoinScreen", ROOM);
    debug("Joined screen room", ROOM);

    const state = await conn.invoke("GetPlayerState", ROOM);
    debug("Initial player state", state);

    if (state) {
        await applyState(state);
    }
}

async function start() {
    if (!validateRoomOrWarn()) return;

    showHudFiveMinutes();

    const hubUrl = `${location.origin}/hubs/rooms`;

    conn = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl)
        .withAutomaticReconnect()
        .build();

    conn.on("playerStateChanged", (state) => {
        debug("playerStateChanged", state);
        applyState(state).catch(err => debug("applyState error", err?.message || err));
    });

    conn.onreconnecting((err) => {
        debug("SignalR reconnecting", err?.message || err);
    });

    conn.onreconnected(() => {
        debug("SignalR reconnected");
    });

    conn.onclose((err) => {
        debug("SignalR closed", err?.message || err);
    });

    await conn.start();
    debug("SignalR connected", hubUrl);

    await joinAndPull();
}

setRoomText();
configureQr();

debug("Screen init", {
    href: location.href,
    search: location.search,
    room: ROOM
});

startBtn.onclick = async () => {
    try {
        await enterFullscreen();
    } catch (e) {
        debug("Fullscreen failed", e?.message || e);
    }

    startOverlay.classList.add("hidden");

    start().catch((err) => {
        debug("start error", err?.message || err);
        console.warn(err);
    });
};