const signalR = require("@microsoft/signalr");

const roomCode = process.argv[2];
const memberId = process.argv[3];

if (!roomCode || !memberId) {
    console.log("Uso: node signalr-test.js <ROOM_CODE> <MEMBER_ID>");
    process.exit(1);
}

function norm(s) {
    return (s || "").trim().toUpperCase();
}

function nowMs() {
    return Date.now();
}

function positionNowMs(state) {
    // state: { roomCode, isPlaying, positionMs, serverTimeMs, audioUrl }
    if (!state.isPlaying) return state.positionMs;
    return Math.max(0, state.positionMs + (nowMs() - state.serverTimeMs));
}

async function start() {
    const conn = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5031/hubs/rooms")
        .withAutomaticReconnect()
        .build();

    conn.on("memberJoined", (p) => console.log("memberJoined:", p));
    conn.on("activeCount", (p) => console.log("activeCount:", p));

    // ✅ escutar mudanças do player
    conn.on("playerStateChanged", (state) => {
        console.log("playerStateChanged:", state);
        console.log("positionNowMs:", Math.floor(positionNowMs(state)));
    });

    await conn.start();
    console.log("connected");

    const code = norm(roomCode);

    await conn.invoke("JoinRoom", code, memberId);
    console.log("joined room");

    // ✅ pegar estado atual
    try {
        const state = await conn.invoke("GetPlayerState", code);
        console.log("GetPlayerState:", state);
        console.log("positionNowMs:", Math.floor(positionNowMs(state)));
    } catch (e) {
        console.log("GetPlayerState ERROR:", e?.message || e);
    }

    // heartbeat
    setInterval(async () => {
        await conn.invoke("Heartbeat", code, memberId);
        console.log("heartbeat sent");
    }, 20000);
}

start().catch(console.error);