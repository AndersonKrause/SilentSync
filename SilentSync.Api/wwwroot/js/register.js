    const statusEl = document.getElementById("status");
    const logEl = document.getElementById("log");
    const roomText = document.getElementById("roomText");

    const LS = { token:"ss_token" };

    const norm = (s)=> (s||"").trim();
    function log(...a){
    logEl.textContent += a.map(x => typeof x === "string" ? x : JSON.stringify(x,null,2)).join(" ") + "\n";
    logEl.scrollTop = logEl.scrollHeight;
}
    function setStatus(s){ statusEl.textContent = s; }

    function getRoom(){
    const qs = new URLSearchParams(location.search);
    return (qs.get("room") || "").trim().toUpperCase();
}
    const ROOM = getRoom();
    roomText.textContent = ROOM || "—";

    function getNext(){
    const qs = new URLSearchParams(location.search);
    return (qs.get("next") || "").trim().toLowerCase();
}

    function gotoNext(){
    const next = getNext();

    if (next === "admin") {
    location.href = "/admin.html";
    return;
}

    const url = ROOM ? `/mobile.html?room=${encodeURIComponent(ROOM)}` : "/mobile.html";
    location.href = url;
}

    function setToken(t){
    if (t) localStorage.setItem(LS.token, t);
    else localStorage.removeItem(LS.token);
}

    async function api(path, { method="GET", body=null } = {}){
    const headers = {};
    if (body) headers["Content-Type"] = "application/json";

    const res = await fetch(path, { method, headers, body: body ? JSON.stringify(body) : null });
    const text = await res.text();
    let json = null;
    try { json = text ? JSON.parse(text) : null; } catch {}

    if (!res.ok) {
    const msg = (json && (json.message || json.title)) || text || ("HTTP " + res.status);
    throw new Error(msg);
}
    return json;
}

    // ===== Tabs + Panels =====
    const tabLogin = document.getElementById("tabLogin");
    const tabCreate = document.getElementById("tabCreate");
    const tabForgot = document.getElementById("tabForgot");

    const panelLogin = document.getElementById("panelLogin");
    const panelCreate1 = document.getElementById("panelCreate1");
    const panelCreate2 = document.getElementById("panelCreate2");
    const panelForgot1 = document.getElementById("panelForgot1");
    const panelForgot2 = document.getElementById("panelForgot2");

    function setActiveTab(which){
    for (const b of [tabLogin, tabCreate, tabForgot]) b.classList.remove("active");
    tabLogin.classList.toggle("active", which === "login");
    tabCreate.classList.toggle("active", which === "create");
    tabForgot.classList.toggle("active", which === "forgot");

    panelLogin.classList.toggle("hide", which !== "login");
    panelCreate1.classList.toggle("hide", which !== "create");
    panelCreate2.classList.add("hide"); // começa no step1
    panelForgot1.classList.toggle("hide", which !== "forgot");
    panelForgot2.classList.add("hide"); // começa no step1
}

    tabLogin.onclick = ()=> setActiveTab("login");
    tabCreate.onclick = ()=> setActiveTab("create");
    tabForgot.onclick = ()=> setActiveTab("forgot");

    // ===== LOGIN (B1) =====
    const loginInfo = document.getElementById("loginInfo");
    document.getElementById("loginBtn").onclick = async () => {
    const email = norm(document.getElementById("loginEmail").value).toLowerCase();
    const password = document.getElementById("loginPassword").value || "";
    if (!email) return alert("Preencha o email");
    if (!password) return alert("Preencha a senha");

    loginInfo.textContent = "Entrando...";
    setStatus("entrando...");

    try {
    const r = await api("/api/auth/login", { method:"POST", body:{ email, password } });
    if (!r?.token) throw new Error("Resposta sem token");
    setToken(r.token);
    loginInfo.innerHTML = "<span class='ok'>OK ✅</span>";
    gotoNext();
} catch(e){
    loginInfo.innerHTML = "<span class='err'>Falhou: email ou senha inválidos.</span>";
    log("login ERROR:", e.message || e);
} finally {
    setStatus("pronto");
}
};

    // ===== CREATE (A) =====
    const createInfo = document.getElementById("createInfo");
    const createCodeInfo = document.getElementById("createCodeInfo");
    const createEmailEcho = document.getElementById("createEmailEcho");

    function showCreateStep2(email){
    panelCreate1.classList.add("hide");
    panelCreate2.classList.remove("hide");
    createEmailEcho.textContent = email;
}

    document.getElementById("createStartBtn").onclick = async () => {
    const email = norm(document.getElementById("createEmail").value).toLowerCase();
    const pw1 = document.getElementById("createPw1").value || "";
    const pw2 = document.getElementById("createPw2").value || "";

    if (!email) return alert("Preencha o email");
    if (pw1.length < 6) return alert("Senha mínima: 6 caracteres");
    if (pw1 !== pw2) return alert("As senhas não coincidem");

    createInfo.textContent = "Enviando código...";
    setStatus("enviando...");

    try {
    const r = await api("/api/auth/register-start", { method:"POST", body:{ email, password: pw1 }});
    log("register-start:", r);
    createInfo.innerHTML = "<span class='ok'>Código enviado ✅</span>";
    showCreateStep2(email);
} catch(e){
    // se usuário já existe, orienta a ir no login
    const msg = (e.message || "");
    if (msg.toLowerCase().includes("already")) {
    createInfo.innerHTML = "<span class='warn'>Este email já tem conta. Use a aba Entrar.</span>";
} else {
    createInfo.innerHTML = "<span class='err'>Falhou: " + msg + "</span>";
}
    log("register-start ERROR:", msg);
} finally {
    setStatus("pronto");
}
};

    document.getElementById("createResendBtn").onclick = async () => {
    const email = norm(document.getElementById("createEmail").value).toLowerCase();
    if (!email) return alert("Preencha o email");

    createCodeInfo.textContent = "Reenviando...";
    setStatus("enviando...");

    try {
    await api("/api/auth/request-code", { method:"POST", body:{ email }});
    createCodeInfo.innerHTML = "<span class='ok'>Reenviado ✅</span>";
} catch(e){
    createCodeInfo.innerHTML = "<span class='err'>Falhou: " + (e.message || e) + "</span>";
    log("resend ERROR:", e.message || e);
} finally {
    setStatus("pronto");
}
};

    document.getElementById("createCompleteBtn").onclick = async () => {
    const email = norm(document.getElementById("createEmail").value).toLowerCase();
    const code = norm(document.getElementById("createCode").value);
    if (!email) return alert("Preencha o email");
    if (!code) return alert("Preencha o código");

    createCodeInfo.textContent = "Confirmando...";
    setStatus("confirmando...");

    try {
    const r = await api("/api/auth/register-complete", { method:"POST", body:{ email, code }});
    if (!r?.token) throw new Error("Resposta sem token");
    setToken(r.token);
    createCodeInfo.innerHTML = "<span class='ok'>Conta criada ✅</span>";
    gotoNext();
} catch(e){
    createCodeInfo.innerHTML = "<span class='err'>Falhou: " + (e.message || e) + "</span>";
    log("register-complete ERROR:", e.message || e);
} finally {
    setStatus("pronto");
}
};

    // ===== FORGOT =====
    const forgotInfo = document.getElementById("forgotInfo");
    const forgotCodeInfo = document.getElementById("forgotCodeInfo");
    const forgotEmailEcho = document.getElementById("forgotEmailEcho");

    function showForgotStep2(email){
    panelForgot1.classList.add("hide");
    panelForgot2.classList.remove("hide");
    forgotEmailEcho.textContent = email;
}

    document.getElementById("forgotSendBtn").onclick = async () => {
    const email = norm(document.getElementById("forgotEmail").value).toLowerCase();
    if (!email) return alert("Preencha o email");

    forgotInfo.textContent = "Enviando código...";
    setStatus("enviando...");

    try{
    const r = await api("/api/auth/forgot-password", { method:"POST", body:{ email }});
    log("forgot-password:", r);
    forgotInfo.innerHTML = "<span class='ok'>Código enviado ✅</span>";
    showForgotStep2(email);
}catch(e){
    forgotInfo.innerHTML = "<span class='err'>Falhou: " + (e.message || e) + "</span>";
    log("forgot-password ERROR:", e.message || e);
}finally{
    setStatus("pronto");
}
};

    document.getElementById("forgotResendBtn").onclick = async () => {
    const email = norm(document.getElementById("forgotEmail").value).toLowerCase();
    if (!email) return alert("Preencha o email");

    forgotCodeInfo.textContent = "Reenviando...";
    setStatus("enviando...");

    try {
    await api("/api/auth/request-code", { method:"POST", body:{ email }});
    forgotCodeInfo.innerHTML = "<span class='ok'>Reenviado ✅</span>";
} catch(e){
    forgotCodeInfo.innerHTML = "<span class='err'>Falhou: " + (e.message || e) + "</span>";
    log("forgot resend ERROR:", e.message || e);
} finally {
    setStatus("pronto");
}
};

    document.getElementById("forgotResetBtn").onclick = async () => {
    const email = norm(document.getElementById("forgotEmail").value).toLowerCase();
    const code = norm(document.getElementById("forgotCode").value);
    const pw1 = document.getElementById("forgotPw1").value || "";
    const pw2 = document.getElementById("forgotPw2").value || "";

    if (!email) return alert("Preencha o email");
    if (!code) return alert("Preencha o código");
    if (pw1.length < 6) return alert("Senha mínima: 6 caracteres");
    if (pw1 !== pw2) return alert("As senhas não coincidem");

    forgotCodeInfo.textContent = "Salvando...";
    setStatus("salvando...");

    try{
    const r = await api("/api/auth/reset-password", { method:"POST", body:{ email, code, newPassword: pw1 }});
    if (!r?.token) throw new Error("Resposta sem token");
    setToken(r.token);
    forgotCodeInfo.innerHTML = "<span class='ok'>Senha atualizada ✅</span>";
    gotoNext();
}catch(e){
    forgotCodeInfo.innerHTML = "<span class='err'>Falhou: " + (e.message || e) + "</span>";
    log("reset-password ERROR:", e.message || e);
}finally{
    setStatus("pronto");
}
};

    // default tab
    setActiveTab("login");