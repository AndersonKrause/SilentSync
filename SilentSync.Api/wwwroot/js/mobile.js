// ========= Helpers =========
const statusEl = document.getElementById('status');
const logEl = document.getElementById('log');
const offsetEl = document.getElementById('offset');
const rttEl = document.getElementById('rtt');
const targetPosEl = document.getElementById('targetPos');
const audioUrlText = document.getElementById('audioUrlText');
const audio = document.getElementById('audio');
const playBtn = document.getElementById('playBtn');

const joinInfo = document.getElementById('joinInfo');

const infoText = document.getElementById('infoText');
const goRegisterBtn = document.getElementById('goRegisterBtn');
const logoutBtn = document.getElementById('logoutBtn');
const roomEcho = document.getElementById('roomEcho');

const joinCard = document.getElementById("joinCard");
const joinBtn = document.getElementById("joinBtn");

const norm = (s) => (s || "").trim().toUpperCase();
const sleep = (ms) => new Promise(r => setTimeout(r, ms));

function log(...a) {
    logEl.textContent += a
        .map(x => typeof x === 'string' ? x : JSON.stringify(x, null, 2))
        .join(' ') + "\n";
    logEl.scrollTop = logEl.scrollHeight;
}

function setStatus(s) {
    statusEl.textContent = s;
}

function setStatusKey(key) {
    setStatus(t(key));
}

function nowMs() {
    return Date.now();
}

function getBaseUrl() {
    return window.location.origin;
}

function normalizeMediaUrl(input) {
    if (!input) return "";
    try {
        const u = new URL(input, window.location.origin);
        if (u.hostname === "localhost" || u.hostname === "127.0.0.1") {
            u.protocol = window.location.protocol;
            u.host = window.location.host;
        }
        return u.toString();
    } catch {
        return input;
    }
}

function getRoomFromQuery() {
    const qsRoom = new URLSearchParams(location.search).get("room");
    return norm(qsRoom || "");
}

// ========= Local Storage =========
const LS = {
    token: "ss_token",
    deviceId: "ss_deviceId",
    displayName: "ss_displayName",
    roomCode: "ss_roomCode",
    memberIdByRoom: (room) => `ss_memberId_${room}`
};

function getOrCreateDeviceId() {
    let id = localStorage.getItem(LS.deviceId);
    if (!id) {
        id = (crypto?.randomUUID?.() || (Date.now() + "-" + Math.random().toString(16).slice(2)));
        localStorage.setItem(LS.deviceId, id);
    }
    return id;
}

function getToken() {
    return localStorage.getItem(LS.token) || "";
}

function setToken(token) {
    if (token) localStorage.setItem(LS.token, token);
    else localStorage.removeItem(LS.token);
}

function goRegister() {
    const room = norm(document.getElementById('roomCode').value) || getRoomFromQuery();
    const url = room
        ? `/pages/register.html?room=${encodeURIComponent(room)}`
        : `/pages/register.html`;
    location.href = url;
}

async function api(path, { method = "GET", body = null, auth = false } = {}) {
    const headers = {};
    if (body) headers["Content-Type"] = "application/json";
    if (auth) headers["Authorization"] = "Bearer " + getToken();

    const res = await fetch(getBaseUrl() + path, {
        method,
        headers,
        body: body ? JSON.stringify(body) : null
    });

    const text = await res.text();
    let json = null;

    try {
        json = text ? JSON.parse(text) : null;
    } catch {}

    if (!res.ok) {
        const msg = (json && (json.message || json.title)) || text || ("HTTP " + res.status);
        throw new Error(msg);
    }

    return json;
}

function updateUiAuth() {
    const logged = !!getToken();
    joinBtn.disabled = !logged;

    infoText.innerHTML = logged
        ? `<span class='ok'>${t("loggedInOk")} ✅</span>`
        : `<span class='warn'>${t("notLoggedRedirecting")}</span>`;
}

// ========= JOIN ROOM =========
let roomCodeCurrent = "";
let memberIdCurrent = "";
let conn = null;
let heartbeatTimer = null;

joinBtn.onclick = async () => {
    const roomCode = norm(document.getElementById('roomCode').value);
    const displayName = document.getElementById('displayName').value.trim();
    const deviceId = getOrCreateDeviceId();

    if (!roomCode) return alert(t("fillRoomCode"));
    if (!displayName) return alert(t("fillDisplayName"));

    localStorage.setItem(LS.displayName, displayName);
    localStorage.setItem(LS.roomCode, roomCode);

    joinInfo.textContent = t("joiningRoom");

    try {
        const r = await api(`/api/rooms/${roomCode}/join-auth`, {
            method: "POST",
            auth: true,
            body: { displayName, deviceId }
        });

        memberIdCurrent = r.memberId;
        roomCodeCurrent = r.roomCode || roomCode;
        localStorage.setItem(LS.memberIdByRoom(roomCodeCurrent), memberIdCurrent);

        joinInfo.innerHTML = `<span class='ok'>${t("joinedOk")} ✅</span>`;
        log("join-auth OK:", r);

        await connectSignalR(roomCodeCurrent, memberIdCurrent);
    } catch (e) {
        joinInfo.innerHTML = `<span class='err'>${tf("failedWithReason", { reason: e.message || e })}</span>`;
        log("join-auth ERROR:", e.message || e);

        if ((e.message || "").toLowerCase().includes("unauthorized") || (e.message || "").includes("401")) {
            goRegister();
        }
    }
};

// ========= AUTO JOIN VIA QR =========
async function autoJoinFromQrIfPossible() {
    const room = getRoomFromQuery();
    if (!room) return false;

    if (joinCard) joinCard.style.display = "none";

    if (!getToken()) return false;

    let displayName = (localStorage.getItem(LS.displayName) || "").trim();
    if (!displayName) {
        displayName = t("guestName");
        localStorage.setItem(LS.displayName, displayName);
    }

    document.getElementById('roomCode').value = room;

    joinInfo.innerHTML = `<span class='warn'>${t("autoJoiningRoom")}</span>`;

    try {
        const deviceId = getOrCreateDeviceId();

        const r = await api(`/api/rooms/${room}/join-auth`, {
            method: "POST",
            auth: true,
            body: { displayName, deviceId }
        });

        memberIdCurrent = r.memberId;
        roomCodeCurrent = r.roomCode || room;
        localStorage.setItem(LS.memberIdByRoom(roomCodeCurrent), memberIdCurrent);

        joinInfo.innerHTML = `<span class='ok'>${t("connectedOk")} ✅</span>`;
        log("auto join-auth OK:", r);

        await connectSignalR(roomCodeCurrent, memberIdCurrent);
        return true;
    } catch (e) {
        joinInfo.innerHTML = `<span class='err'>${tf("autoJoinFailed", { reason: e?.message || e })}</span>`;
        log("autoJoinFromQr ERROR:", e?.message || e);

        if ((e?.message || "").toLowerCase().includes("unauthorized") || (e?.message || "").includes("401")) {
            goRegister();
        }
        return false;
    }
}

// ========= SignalR + Time Sync =========
let serverOffsetMs = 0;
let lastState = null;

function computeOffset(t0, t1, t2, t3) {
    const offset = ((t1 - t0) + (t2 - t3)) / 2;
    const rtt = (t3 - t0) - (t2 - t1);
    return { offset, rtt };
}

async function timeSync(samples = 10) {
    const results = [];
    for (let i = 0; i < samples; i++) {
        const t0 = nowMs();
        const resp = await conn.invoke("TimeSync", t0);
        const t3 = nowMs();
        const { offset, rtt } = computeOffset(resp.t0, resp.t1, resp.t2, t3);
        results.push({ offset, rtt });
        await sleep(120);
    }

    results.sort((a, b) => a.rtt - b.rtt);
    return results[0];
}

function serverNowMs() {
    return nowMs() + serverOffsetMs;
}

function calcTargetPositionMs(state) {
    const now = serverNowMs();
    if (!state.isPlaying) return state.positionMs;
    return Math.max(0, state.positionMs + (now - state.serverTimeMs));
}

// ========= Audio source (HLS/MP3) =========
let hls = null;
let currentAudioUrl = "";

function setAudioSource(url) {
    if (hls) {
        hls.destroy();
        hls = null;
    }

    audio.pause();
    audio.removeAttribute("src");
    audio.load();

    const isM3u8 = url.toLowerCase().includes(".m3u8");

    if (isM3u8 && audio.canPlayType("application/vnd.apple.mpegurl")) {
        audio.src = url;
        return;
    }

    if (isM3u8 && window.Hls && Hls.isSupported()) {
        hls = new Hls({ lowLatencyMode: true });
        hls.loadSource(url);
        hls.attachMedia(audio);

        hls.on(Hls.Events.ERROR, (evt, data) => {
            log("HLS error:", data?.type, data?.details, data?.fatal ? "(fatal)" : "");
        });
        return;
    }

    audio.src = url;
}

async function applyState(state, reason) {
    lastState = state;

    if (!state || !state.audioUrl) {
        log(t("waitingAudioUrl"), state);
        return;
    }

    const abs = normalizeMediaUrl(state.audioUrl);
    audioUrlText.textContent = abs;

    if (currentAudioUrl !== abs) {
        currentAudioUrl = abs;
        setAudioSource(abs);
        log("audio src set:", abs);
        playBtn.disabled = false;
    }

    const target = calcTargetPositionMs(state);
    targetPosEl.textContent = String(Math.floor(target));

    if (audio.readyState < 2) {
        log(t("audioStillLoading"), reason);
        return;
    }

    const current = audio.currentTime * 1000;
    const diff = target - current;

    if (Math.abs(diff) > 350) {
        audio.currentTime = target / 1000;
        log("seek hard:", reason, "diff(ms)=", Math.floor(diff));
    }

    if (state.isPlaying) {
        try {
            await audio.play();
        } catch (e) {
            log(t("autoplayBlocked"), e?.message || e);
        }
    } else {
        audio.pause();
    }
}

setInterval(() => {
    if (!lastState) return;
    if (!lastState.isPlaying) return;
    if (audio.readyState < 2) return;

    const target = calcTargetPositionMs(lastState);
    const current = audio.currentTime * 1000;
    const diff = target - current;

    if (Math.abs(diff) > 180) {
        audio.currentTime = target / 1000;
        log("drift fix diff(ms)=", Math.floor(diff));
    }
}, 3000);

playBtn.onclick = async () => {
    try {
        await audio.play();
        log(t("manualPlayOk"));
    } catch (e) {
        log(t("manualPlayFailed"), e?.message || e);
    }
};

async function connectSignalR(roomCode, memberId) {
    if (conn) {
        try {
            await conn.stop();
        } catch {}
        conn = null;
    }

    if (heartbeatTimer) {
        clearInterval(heartbeatTimer);
        heartbeatTimer = null;
    }

    setStatusKey("statusConnecting");

    const hubUrl = getBaseUrl() + "/hubs/rooms";

    conn = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, { accessTokenFactory: () => getToken() })
        .withAutomaticReconnect()
        .build();

    conn.onclose(() => setStatusKey("statusDisconnected"));
    conn.onreconnecting(() => setStatusKey("statusReconnecting"));

    conn.onreconnected(async () => {
        setStatusKey("statusConnected");
        try {
            await conn.invoke("JoinRoom", roomCode, memberId);
            const state = await conn.invoke("GetPlayerState", roomCode);
            await applyState(state, "reconnected-pull");
        } catch (e) {
            log("reconnected flow ERROR:", e?.message || e);
        }
    });

    conn.on("playerStateChanged", (state) => {
        log("playerStateChanged:", state);
        applyState(state, "push");
    });

    await conn.start();
    setStatusKey("statusConnected");
    log("connected:", hubUrl);

    const best = await timeSync(10);
    serverOffsetMs = best.offset;
    offsetEl.textContent = best.offset.toFixed(1);
    rttEl.textContent = best.rtt.toFixed(1);
    log("timeSync best:", best);

    await conn.invoke("JoinRoom", roomCode, memberId);
    log("joined room:", roomCode);

    heartbeatTimer = setInterval(async () => {
        try {
            await conn.invoke("Heartbeat", roomCode);
        } catch {}
    }, 30000);

    const state = await conn.invoke("GetPlayerState", roomCode);
    log("GetPlayerState:", state);
    await applyState(state, "pull");
}

// ========= Init =========
(function init() {
    const qsRoom = getRoomFromQuery();

    if (qsRoom) {
        document.getElementById('roomCode').value = qsRoom;
        localStorage.setItem(LS.roomCode, qsRoom);
    } else {
        document.getElementById('roomCode').value = localStorage.getItem(LS.roomCode) || "";
    }

    roomEcho.textContent = norm(document.getElementById('roomCode').value) || t("emptyValue");
    document.getElementById('displayName').value = localStorage.getItem(LS.displayName) || "";

    getOrCreateDeviceId();
    updateUiAuth();

    goRegisterBtn.onclick = () => goRegister();
    logoutBtn.onclick = () => {
        setToken("");
        joinInfo.textContent = "";
        setStatusKey("statusDisconnected");
        updateUiAuth();
        goRegister();
    };

    if (!getToken()) {
        setTimeout(goRegister, 200);
        return;
    }

    autoJoinFromQrIfPossible();
})();