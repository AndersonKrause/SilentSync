const statusEl = document.getElementById("status");
const logEl = document.getElementById("log");
const roomText = document.getElementById("roomText");

const LS = { token: "ss_token" };

const norm = (s) => (s || "").trim();

function log(...a) {
    if (!logEl) return;
    logEl.textContent += a
        .map(x => typeof x === "string" ? x : JSON.stringify(x, null, 2))
        .join(" ") + "\n";
    logEl.scrollTop = logEl.scrollHeight;
}

function setStatus(s) {
    if (statusEl) statusEl.textContent = s;
}

function getRoom() {
    const qs = new URLSearchParams(location.search);
    return (qs.get("room") || "").trim().toUpperCase();
}

const ROOM = getRoom();
if (roomText) roomText.textContent = ROOM || "—";

async function fetchMe() {
    const token = localStorage.getItem(LS.token);
    if (!token) return null;

    const res = await fetch("/api/auth/me", {
        headers: {
            "Authorization": "Bearer " + token
        }
    });

    if (!res.ok) return null;

    return await res.json();
}
 
async function gotoNext() {

    const me = await fetchMe();

    const role = (me?.role || "").toLowerCase();

    if (role === "admin" || role === "host") {
        location.href = "/pages/admin.html";
        return;
    }

    const url = ROOM
        ? `/pages/mobile.html?room=${encodeURIComponent(ROOM)}`
        : "/pages/mobile.html";

    location.href = url;
}

function setToken(t) {
    if (t) localStorage.setItem(LS.token, t);
    else localStorage.removeItem(LS.token);
}

async function api(path, { method = "GET", body = null } = {}) {
    const headers = {};
    if (body) headers["Content-Type"] = "application/json";

    const res = await fetch(path, {
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

// ===== Views =====
const viewLogin = document.getElementById("viewLogin");
const viewCreateStep1 = document.getElementById("viewCreateStep1");
const viewCreateStep2 = document.getElementById("viewCreateStep2");
const viewForgotStep1 = document.getElementById("viewForgotStep1");
const viewForgotStep2 = document.getElementById("viewForgotStep2");

function hideAllViews() {
    viewLogin?.classList.add("hide");
    viewCreateStep1?.classList.add("hide");
    viewCreateStep2?.classList.add("hide");
    viewForgotStep1?.classList.add("hide");
    viewForgotStep2?.classList.add("hide");
}

function showView(name) {
    hideAllViews();

    if (name === "login") viewLogin?.classList.remove("hide");
    if (name === "create-step1") viewCreateStep1?.classList.remove("hide");
    if (name === "create-step2") viewCreateStep2?.classList.remove("hide");
    if (name === "forgot-step1") viewForgotStep1?.classList.remove("hide");
    if (name === "forgot-step2") viewForgotStep2?.classList.remove("hide");
}

document.getElementById("showCreateBtn")?.addEventListener("click", () => showView("create-step1"));
document.getElementById("showForgotBtn")?.addEventListener("click", () => showView("forgot-step1"));
document.getElementById("backToLoginFromCreateBtn")?.addEventListener("click", () => showView("login"));
document.getElementById("backToLoginFromCreateCodeBtn")?.addEventListener("click", () => showView("login"));
document.getElementById("backToLoginFromForgotBtn")?.addEventListener("click", () => showView("login"));
document.getElementById("backToLoginFromForgotResetBtn")?.addEventListener("click", () => showView("login"));

// ===== LOGIN =====
const loginInfo = document.getElementById("loginInfo");

document.getElementById("loginBtn")?.addEventListener("click", async () => {
    const email = norm(document.getElementById("loginEmail")?.value).toLowerCase();
    const password = document.getElementById("loginPassword")?.value || "";

    if (!email) return alert("Preencha o email");
    if (!password) return alert("Preencha a senha");

    if (loginInfo) loginInfo.textContent = "Entrando...";
    setStatus("entrando...");

    try {
        const r = await api("/api/auth/login", {
            method: "POST",
            body: { email, password }
        });

        if (!r?.token) throw new Error("Resposta sem token");

        setToken(r.token);
        if (loginInfo) loginInfo.innerHTML = "<span class='ok'>Login realizado ✅</span>";
        gotoNext();
    } catch (e) {
        if (loginInfo) loginInfo.innerHTML = "<span class='err'>Falhou: email ou senha inválidos.</span>";
        log("login ERROR:", e.message || e);
    } finally {
        setStatus("pronto");
    }
});

// ===== CREATE ACCOUNT =====
const createInfo = document.getElementById("createInfo");
const createCodeInfo = document.getElementById("createCodeInfo");
const createEmailEcho = document.getElementById("createEmailEcho");

function showCreateStep2(email) {
    if (createEmailEcho) createEmailEcho.textContent = email;
    showView("create-step2");
}

document.getElementById("createStartBtn")?.addEventListener("click", async () => {
    const email = norm(document.getElementById("createEmail")?.value).toLowerCase();
    const pw1 = document.getElementById("createPw1")?.value || "";
    const pw2 = document.getElementById("createPw2")?.value || "";

    if (!email) return alert("Preencha o email");
    if (pw1.length < 6) return alert("Senha mínima: 6 caracteres");
    if (pw1 !== pw2) return alert("As senhas não coincidem");

    if (createInfo) createInfo.textContent = "Enviando código...";
    setStatus("enviando...");

    try {
        const r = await api("/api/auth/register-start", {
            method: "POST",
            body: { email, password: pw1 }
        });

        log("register-start:", r);
        if (createInfo) createInfo.innerHTML = "<span class='ok'>Código enviado ✅</span>";
        showCreateStep2(email);
    } catch (e) {
        const msg = e.message || "";
        if (msg.toLowerCase().includes("already")) {
            if (createInfo) createInfo.innerHTML = "<span class='warn'>Este email já tem conta. Use Entrar.</span>";
        } else {
            if (createInfo) createInfo.innerHTML = `<span class='err'>Falhou: ${msg}</span>`;
        }
        log("register-start ERROR:", msg);
    } finally {
        setStatus("pronto");
    }
});

document.getElementById("createResendBtn")?.addEventListener("click", async () => {
    const email = norm(document.getElementById("createEmail")?.value).toLowerCase();
    if (!email) return alert("Preencha o email");

    if (createCodeInfo) createCodeInfo.textContent = "Reenviando...";
    setStatus("enviando...");

    try {
        await api("/api/auth/request-code", {
            method: "POST",
            body: { email }
        });

        if (createCodeInfo) createCodeInfo.innerHTML = "<span class='ok'>Código reenviado ✅</span>";
    } catch (e) {
        if (createCodeInfo) createCodeInfo.innerHTML = `<span class='err'>Falhou: ${e.message || e}</span>`;
        log("create resend ERROR:", e.message || e);
    } finally {
        setStatus("pronto");
    }
});

document.getElementById("createCompleteBtn")?.addEventListener("click", async () => {
    const email = norm(document.getElementById("createEmail")?.value).toLowerCase();
    const code = norm(document.getElementById("createCode")?.value);

    if (!email) return alert("Preencha o email");
    if (!code) return alert("Preencha o código");

    if (createCodeInfo) createCodeInfo.textContent = "Confirmando...";
    setStatus("confirmando...");

    try {
        const r = await api("/api/auth/register-complete", {
            method: "POST",
            body: { email, code }
        });

        if (!r?.token) throw new Error("Resposta sem token");

        setToken(r.token);
        if (createCodeInfo) createCodeInfo.innerHTML = "<span class='ok'>Conta criada ✅</span>";
        gotoNext();
    } catch (e) {
        if (createCodeInfo) createCodeInfo.innerHTML = `<span class='err'>Falhou: ${e.message || e}</span>`;
        log("register-complete ERROR:", e.message || e);
    } finally {
        setStatus("pronto");
    }
});

// ===== FORGOT PASSWORD =====
const forgotInfo = document.getElementById("forgotInfo");
const forgotCodeInfo = document.getElementById("forgotCodeInfo");
const forgotEmailEcho = document.getElementById("forgotEmailEcho");

function showForgotStep2(email) {
    if (forgotEmailEcho) forgotEmailEcho.textContent = email;
    showView("forgot-step2");
}

document.getElementById("forgotSendBtn")?.addEventListener("click", async () => {
    const email = norm(document.getElementById("forgotEmail")?.value).toLowerCase();
    if (!email) return alert("Preencha o email");

    if (forgotInfo) forgotInfo.textContent = "Enviando código...";
    setStatus("enviando...");

    try {
        const r = await api("/api/auth/forgot-password", {
            method: "POST",
            body: { email }
        });

        log("forgot-password:", r);
        if (forgotInfo) forgotInfo.innerHTML = "<span class='ok'>Código enviado ✅</span>";
        showForgotStep2(email);
    } catch (e) {
        if (forgotInfo) forgotInfo.innerHTML = `<span class='err'>Falhou: ${e.message || e}</span>`;
        log("forgot-password ERROR:", e.message || e);
    } finally {
        setStatus("pronto");
    }
});

document.getElementById("forgotResendBtn")?.addEventListener("click", async () => {
    const email = norm(document.getElementById("forgotEmail")?.value).toLowerCase();
    if (!email) return alert("Preencha o email");

    if (forgotCodeInfo) forgotCodeInfo.textContent = "Reenviando...";
    setStatus("enviando...");

    try {
        await api("/api/auth/request-code", {
            method: "POST",
            body: { email }
        });

        if (forgotCodeInfo) forgotCodeInfo.innerHTML = "<span class='ok'>Código reenviado ✅</span>";
    } catch (e) {
        if (forgotCodeInfo) forgotCodeInfo.innerHTML = `<span class='err'>Falhou: ${e.message || e}</span>`;
        log("forgot resend ERROR:", e.message || e);
    } finally {
        setStatus("pronto");
    }
});

document.getElementById("forgotResetBtn")?.addEventListener("click", async () => {
    const email = norm(document.getElementById("forgotEmail")?.value).toLowerCase();
    const code = norm(document.getElementById("forgotCode")?.value);
    const pw1 = document.getElementById("forgotPw1")?.value || "";
    const pw2 = document.getElementById("forgotPw2")?.value || "";

    if (!email) return alert("Preencha o email");
    if (!code) return alert("Preencha o código");
    if (pw1.length < 6) return alert("Senha mínima: 6 caracteres");
    if (pw1 !== pw2) return alert("As senhas não coincidem");

    if (forgotCodeInfo) forgotCodeInfo.textContent = "Salvando...";
    setStatus("salvando...");

    try {
        const r = await api("/api/auth/reset-password", {
            method: "POST",
            body: { email, code, newPassword: pw1 }
        });

        if (!r?.token) throw new Error("Resposta sem token");

        setToken(r.token);
        if (forgotCodeInfo) forgotCodeInfo.innerHTML = "<span class='ok'>Senha atualizada ✅</span>";
        gotoNext();
    } catch (e) {
        if (forgotCodeInfo) forgotCodeInfo.innerHTML = `<span class='err'>Falhou: ${e.message || e}</span>`;
        log("reset-password ERROR:", e.message || e);
    } finally {
        setStatus("pronto");
    }
});

// default
showView("login");
setStatus("pronto");