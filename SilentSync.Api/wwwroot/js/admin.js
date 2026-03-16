    const PUBLIC_ORIGIN = window.location.origin;

    const LS = { token: "ss_token" };

    const logEl = document.getElementById("log");
    const statusEl = document.getElementById("status");
    const video = document.getElementById("video");

    const uploadBtn = document.getElementById("uploadBtn");
    const uploadProg = document.getElementById("uploadProg");
    const uploadInfo = document.getElementById("uploadInfo");

    const createRoomBtn = document.getElementById("createRoomBtn");
    const openScreenBtn = document.getElementById("openScreenBtn");
    const copyRegBtn = document.getElementById("copyRegBtn");
    const screenLinkText = document.getElementById("screenLinkText");
    const regLinkText = document.getElementById("regLinkText");
    const qrReg = document.getElementById("qrReg");
    const logoutBtn = document.getElementById("logoutBtn");
    const userInfo = document.getElementById("userInfo");

    const sleep = (ms) => new Promise(r => setTimeout(r, ms));
    const norm = (s) => (s || "").trim().toUpperCase();

    let conn = null;
    let lastSent = 0;
    let pending = false;

    function getToken() {
    return localStorage.getItem(LS.token) || "";
}

    function redirectToAdminLogin() {
    location.href = "/pages/register.html?next=admin";
}

    function logout() {
    localStorage.removeItem(LS.token);
    redirectToAdminLogin();
}

    async function fetchMe() {
    const token = getToken();
    if (!token) return null;

    const res = await fetch("/api/auth/me", {
    headers: {
    "Authorization": "Bearer " + token
}
});

    if (res.status === 401) {
    logout();
    return null;
}

    if (!res.ok) {
    throw new Error("Failed to load current user.");
}

    return await res.json();
}

    (async function checkAuth() {
    const token = getToken();
    if (!token) {
    redirectToAdminLogin();
    return;
}

    try {
    const me = await fetchMe();
    if (!me) return;

    const role = (me.role || "").toLowerCase();
    userInfo.textContent = `Email: ${me.email}, Function:${role}`;

    if (role !== "host" && role !== "admin") {
    alert("Você não tem permissão para acessar o Admin.");
    location.href = "/pages/mobile.html";
}
} catch (e) {
    console.error(e);
    logout();
}
})();

    function log(...a) {
    const line = a.map(x => typeof x === "string" ? x : JSON.stringify(x, null, 2)).join(" ");
    logEl.textContent += line + "\n";
    logEl.scrollTop = logEl.scrollHeight;
}

    function setStatus(s) {
    statusEl.textContent = s;
}

    logoutBtn.onclick = logout;

    function buildLinks(roomCode) {
    const screenUrl = `${PUBLIC_ORIGIN}/pages/screen.html?room=${encodeURIComponent(roomCode)}`;
    const regUrl = `${PUBLIC_ORIGIN}/pages/register.html?room=${encodeURIComponent(roomCode)}`;
    return { screenUrl, regUrl };
}

    function renderLinks(roomCode) {
    const rc = norm(roomCode);
    if (!rc) {
    screenLinkText.textContent = "—";
    regLinkText.textContent = "—";
    qrReg.style.display = "none";
    openScreenBtn.disabled = true;
    copyRegBtn.disabled = true;
    return;
}

    const { screenUrl, regUrl } = buildLinks(rc);
    screenLinkText.textContent = screenUrl;
    regLinkText.textContent = regUrl;

    qrReg.src = `https://api.qrserver.com/v1/create-qr-code/?size=260x260&data=${encodeURIComponent(regUrl)}`;
    qrReg.style.display = "block";
    openScreenBtn.disabled = false;
    copyRegBtn.disabled = false;
}

    (function initRoomFromQuery() {
    const qsRoom = new URLSearchParams(location.search).get("room");
    if (qsRoom) {
    document.getElementById("roomCode").value = norm(qsRoom);
}
    renderLinks(document.getElementById("roomCode").value);
})();

    document.getElementById("roomCode").addEventListener("input", (e) => renderLinks(e.target.value));

    createRoomBtn.onclick = async () => {
    createRoomBtn.disabled = true;
    createRoomBtn.textContent = "Criando...";

    try {
    const token = getToken();
    if (!token) {
    alert("Faça login antes de criar uma room.");
    redirectToAdminLogin();
    return;
}

    const res = await fetch("/api/rooms", {
    method: "POST",
    headers: {
    "Authorization": "Bearer " + token
}
});

    if (res.status === 401) {
    alert("Sua sessão expirou. Faça login novamente.");
    redirectToAdminLogin();
    return;
}

    if (res.status === 403) {
    alert("Seu usuário não tem permissão para criar rooms.");
    location.href = "/pages/mobile.html";
    return;
}

    const json = await res.json().catch(() => null);
    if (!res.ok) {
    throw new Error((json && (json.message || json.title)) || ("HTTP " + res.status));
}

    const code = json?.code || json?.Code || json?.roomCode || json?.room?.code;
    if (!code) throw new Error("Resposta sem code.");

    const rc = norm(code);
    document.getElementById("roomCode").value = rc;
    renderLinks(rc);

    log("room created:", json);

    const { screenUrl } = buildLinks(rc);
    window.open(screenUrl, "_blank", "noopener,noreferrer");
} catch (e) {
    alert("Falhou ao criar room: " + (e?.message || e));
    log("create room ERROR:", e?.message || e);
} finally {
    createRoomBtn.disabled = false;
    createRoomBtn.textContent = "Criar Room (abre Telão)";
}
};

    openScreenBtn.onclick = () => {
    const rc = norm(document.getElementById("roomCode").value);
    if (!rc) {
    alert("Room Code vazio");
    return;
}

    const { screenUrl } = buildLinks(rc);
    window.open(screenUrl, "_blank", "noopener,noreferrer");
};

    copyRegBtn.onclick = async () => {
    const rc = norm(document.getElementById("roomCode").value);
    if (!rc) {
    alert("Room Code vazio");
    return;
}

    const { regUrl } = buildLinks(rc);
    try {
    await navigator.clipboard.writeText(regUrl);
    copyRegBtn.textContent = "Copiado ✅";
    setTimeout(() => copyRegBtn.textContent = "Copiar link do Registro", 1200);
} catch {
    prompt("Copie o link do Registro:", regUrl);
}
};

    document.getElementById("file").addEventListener("change", (ev) => {
    const f = ev.target.files?.[0];
    if (!f) return;

    video.src = URL.createObjectURL(f);
    log("video loaded (admin preview):", f.name);
});

    uploadBtn.onclick = async () => {
    const f = document.getElementById("file").files?.[0];
    if (!f) {
    alert("Selecione um vídeo primeiro.");
    return;
}

    uploadProg.style.display = "block";
    uploadProg.value = 0;
    uploadInfo.innerHTML = "<span class='warn'>Enviando e processando...</span>";

    try {
    const xhr = new XMLHttpRequest();
    xhr.open("POST", "/api/media/upload");
    xhr.responseType = "json";

    xhr.upload.onprogress = (e) => {
    if (!e.lengthComputable) return;
    uploadProg.value = Math.round((e.loaded / e.total) * 100);
};

    const form = new FormData();
    form.append("file", f, f.name);

    const result = await new Promise((resolve, reject) => {
    xhr.onload = () => {
    if (xhr.status >= 200 && xhr.status < 300) {
    resolve(xhr.response);
} else {
    reject(new Error(
    (xhr.response && JSON.stringify(xhr.response)) ||
    xhr.responseText ||
    ("HTTP " + xhr.status)
    ));
}
};
    xhr.onerror = () => reject(new Error("Network error"));
    xhr.send(form);
});

    log("upload result:", result);

    const a = result?.audioPath || result?.audioUrl;
    const v = result?.videoPath || result?.videoUrl;

    if (a) document.getElementById("audioUrl").value = PUBLIC_ORIGIN + a;
    if (v) document.getElementById("videoUrl").value = PUBLIC_ORIGIN + v;

    if (a && v) {
    uploadInfo.innerHTML = "<span class='ok'>OK! áudio + vídeo prontos ✅</span>";
} else {
    uploadInfo.innerHTML = "<span class='warn'>Upload OK, mas faltou audioPath/videoPath na resposta.</span>";
}
} catch (e) {
    uploadInfo.innerHTML = "<span class='err'>Falhou: " + (e?.message || e) + "</span>";
    log("upload ERROR:", e?.message || e);
}
};

    function buildPayload() {
    const roomCode = norm(document.getElementById("roomCode").value);
    const audioUrl = (document.getElementById("audioUrl").value || "").trim();
    const videoUrl = (document.getElementById("videoUrl").value || "").trim();

    const isPlaying = !video.paused && !video.ended;
    const positionMs = Math.max(0, Math.floor(video.currentTime * 1000));

    return { roomCode, isPlaying, positionMs, audioUrl, videoUrl };
}

    async function sendState(reason) {
    if (!conn) return;

    const payload = buildPayload();
    if (!payload.roomCode) return;

    try {
    const state = await conn.invoke("UpdatePlayerState", {
    roomCode: payload.roomCode,
    isPlaying: payload.isPlaying,
    positionMs: payload.positionMs,
    audioUrl: payload.audioUrl,
    videoUrl: payload.videoUrl,
    serverTimeMs: Date.now()
});

    log("sent:", reason, state);
} catch (e) {
    log("ERROR sendState:", reason, e?.message || e);
}
}

    async function scheduleSend(reason) {
    const now = Date.now();

    if ((now - lastSent) >= 500) {
    lastSent = now;
    await sendState(reason);
    return;
}

    pending = true;
    await sleep(550);

    if (pending) {
    pending = false;
    lastSent = Date.now();
    await sendState("throttled");
}
}

    async function ensureConnection(roomCode) {
    if (conn) return conn;

    const hubUrl = PUBLIC_ORIGIN + "/hubs/rooms";
    const token = getToken();

    if (!token) {
    alert("Sua sessão expirou. Faça login novamente.");
    redirectToAdminLogin();
    return null;
}

    conn = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, {
    accessTokenFactory: () => getToken()
})
    .withAutomaticReconnect()
    .build();

    conn.onclose((e) => {
    setStatus("desconectado");
    log("hub closed:", e?.message || "");
});

    conn.onreconnecting((e) => {
    setStatus("reconectando...");
    log("hub reconnecting:", e?.message || "");
});

    conn.onreconnected(async () => {
    setStatus("conectado");
    log("hub reconnected");

    const rc = norm(document.getElementById("roomCode").value);
    if (!rc) return;

    try {
    await conn.invoke("JoinAsController", rc);
    log("rejoined as controller:", rc);
    await scheduleSend("reconnected");
} catch (e) {
    log("rejoin controller ERROR:", e?.message || e);
}
});

    await conn.start();
    setStatus("conectado");
    log("hub connected:", hubUrl);

    return conn;
}

    document.getElementById("connectBtn").onclick = async () => {
    const roomCode = norm(document.getElementById("roomCode").value);
    const audioUrl = (document.getElementById("audioUrl").value || "").trim();
    const videoUrl = (document.getElementById("videoUrl").value || "").trim();

    if (!roomCode) {
    alert("Preencha Room Code.");
    return;
}

    if (!audioUrl || !videoUrl) {
    alert("Faça o upload primeiro (precisamos de Audio e Video URL).");
    return;
}

    try {
    const connection = await ensureConnection(roomCode);
    if (!connection) return;

    await connection.invoke("JoinAsController", roomCode);
    log("joined as controller:", roomCode);

    await scheduleSend("connect");
} catch (e) {
    log("connect controller ERROR:", e?.message || e);
    alert("Falha ao conectar controller: " + (e?.message || e));
}
};

    document.getElementById("playBtn").onclick = async () => {
    await video.play();
    await scheduleSend("playBtn");
};

    document.getElementById("pauseBtn").onclick = async () => {
    video.pause();
    await scheduleSend("pauseBtn");
};

    document.getElementById("seekBtn").onclick = async () => {
    const v = parseInt(document.getElementById("seekMs").value, 10);
    if (!Number.isFinite(v)) {
    alert("Seek inválido");
    return;
}

    video.currentTime = v / 1000;
    await scheduleSend("seekBtn");
};

    video.addEventListener("play", () => scheduleSend("play"));
    video.addEventListener("pause", () => scheduleSend("pause"));
    video.addEventListener("seeking", () => scheduleSend("seeking"));
    video.addEventListener("seeked", () => scheduleSend("seeked"));

    setInterval(() => {
    if (!conn) return;
    if (video.paused || video.ended) return;
    scheduleSend("tick");
}, 2000);
    