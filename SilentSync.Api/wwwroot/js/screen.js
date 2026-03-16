    const params = new URLSearchParams(location.search);
    const ROOM = (params.get("room") || "").toUpperCase();

    if(!ROOM){
    alert("Abra como /pages/screen.html?room=ABC123");
}

    const video = document.getElementById("video");
    const roomText = document.getElementById("roomText");
    const qrImg = document.getElementById("qrImg");
    const qrHint = document.getElementById("qrHint");
    const hud = document.querySelector(".hud");

    const startOverlay = document.getElementById("startOverlay");
    const startBtn = document.getElementById("startBtn");

    roomText.textContent = "ROOM: " + ROOM;

    const registerUrl = location.origin + "/pages/register.html?room=" + encodeURIComponent(ROOM);

    qrImg.src =
    "https://api.qrserver.com/v1/create-qr-code/?size=320x320&data="
    + encodeURIComponent(registerUrl);

    if(location.hostname === "localhost"){
    qrHint.textContent =
        "Abra o telão pelo IP para o QR funcionar no celular";
}else{
    qrHint.textContent =
        "Escaneie para entrar e ouvir o áudio no celular";
}

    function showHudFiveMinutes(){

    hud.classList.remove("hidden");

    setTimeout(()=>{
    hud.classList.add("hidden");
},300000);

}

    async function enterFullscreen(){

    const el = document.documentElement;

    if(el.requestFullscreen){
    await el.requestFullscreen();
}else if(el.webkitRequestFullscreen){
    await el.webkitRequestFullscreen();
}

}

    let conn;
    let currentVideoUrl="";
    let lastState=null;

    function absUrl(u){
    try{
    return new URL(u,location.origin).toString();
}catch{
    return u;
}
}

    async function applyState(state){

    if(!state) return;

    if((state.roomCode||"").toUpperCase()!==ROOM) return;

    lastState = state;

    const videoUrl = absUrl(state.videoUrl);

    if(videoUrl && videoUrl!==currentVideoUrl){

    currentVideoUrl = videoUrl;

    video.src = videoUrl;
    video.load();

}

    if(state.isPlaying){

    try{
    await video.play();
}catch{}

}else{

    video.pause();

}

    const target = Math.max(0,state.positionMs||0)/1000;

    if(video.readyState>=1){

    if(Math.abs(video.currentTime-target)>0.35){
    video.currentTime = target;
}

}

}

    async function joinAndPull(){

    await conn.invoke("JoinScreen",ROOM);

    const state = await conn.invoke("GetPlayerState",ROOM);

    if(state){
    await applyState(state);
}

}

    async function start(){

    showHudFiveMinutes();

    const hubUrl = location.origin + "/hubs/rooms";

    conn = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl)
    .withAutomaticReconnect()
    .build();

    conn.on("playerStateChanged",(state)=>{
    applyState(state);
});

    await conn.start();

    await joinAndPull();

}

    startBtn.onclick = async () => {

    await enterFullscreen();

    startOverlay.classList.add("hidden");

    start().catch(console.warn);

};
