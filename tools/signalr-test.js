const signalR = require("@microsoft/signalr");

const roomCode = process.argv[2];
const memberId = process.argv[3];

if (!roomCode || !memberId) {
    console.log("Uso: node signalr-test.js <ROOM_CODE> <MEMBER_ID>");
    process.exit(1);
}

async function start() {
    const conn = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5031/hubs/rooms")
        .withAutomaticReconnect()
        .build();

    conn.on("memberJoined", (p) => console.log("memberJoined:", p));
    conn.on("activeCount", (p) => console.log("activeCount:", p));

    await conn.start();
    console.log("connected");

    await conn.invoke("JoinRoom", roomCode, memberId);
    console.log("joined room");

    setInterval(async () => {
        await conn.invoke("Heartbeat", roomCode, memberId);
        console.log("heartbeat sent");
    }, 20000);
}

start().catch(console.error);