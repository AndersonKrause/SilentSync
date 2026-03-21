// Dicionário de traduções
const translations = {
    pt: {
        // ===== COMUM =====
        emptyValue: "—",
        failedWithReason: "Falhou: {reason}",
        statusReady: "pronto",
        statusSending: "enviando...",
        statusLoggingIn: "entrando...",
        statusConnecting: "conectando...",
        statusConnected: "conectado",
        statusDisconnected: "desconectado",
        statusReconnecting: "reconectando...",
        statusConfirming: "confirmando...",
        statusSaving: "salvando...",

        // ===== LOGIN =====
        login: "Entrar",
        loginSuccess: "Login realizado com sucesso",
        loginFailed: "Falha no login",
        noToken: "Token não recebido",

        email: "Email",
        password: "Senha",
        logout: "Sair",

        loginSubtitle: "Acesse sua conta para continuar",
        emailPlaceholder: "voce@exemplo.com",
        passwordPlaceholder: "Digite sua senha",

        fillEmail: "Preencha o email",
        fillPassword: "Preencha a senha",
        fillCode: "Preencha o código",

        minPassword: "Senha mínima: 6 caracteres",
        passwordsMismatch: "As senhas não coincidem",

        // ===== CREATE ACCOUNT =====
        createAccount: "Criar conta",
        createSubtitle: "Preencha seus dados para receber um código de confirmação",
        confirmSignupSubtitle: "Digite o código enviado para",

        confirmPassword: "Confirmar senha",
        confirmPasswordPlaceholder: "Repita a senha",

        newPassword: "Nova senha",
        newPasswordPlaceholder: "Mínimo 6 caracteres",

        confirmNewPassword: "Confirmar nova senha",
        confirmNewPasswordPlaceholder: "Repita a senha",

        completeSignup: "Concluir cadastro",

        createSendingCode: "Enviando código...",
        createCodeSent: "Código enviado",
        createAccountExists: "Conta já existe",
        createResending: "Reenviando código...",
        createCodeResent: "Código reenviado",
        createConfirming: "Confirmando...",
        createAccountCreated: "Conta criada com sucesso",

        // ===== FORGOT =====
        forgotPassword: "Esqueci minha senha",
        forgotSubtitle: "Informe seu email para receber um código de redefinição",
        resetSubtitle: "Digite o código enviado para",

        forgotSendingCode: "Enviando código...",
        forgotCodeSent: "Código enviado",
        forgotResending: "Reenviando código...",
        forgotCodeResent: "Código reenviado",
        forgotSaving: "Salvando nova senha...",
        forgotPasswordUpdated: "Senha atualizada com sucesso",

        // ===== NAV =====
        backToLogin: "Voltar para entrar",
        sendCode: "Enviar código",
        confirmCode: "Confirmar código",
        resetPassword: "Redefinir senha",
        enterRoom: "Entrar na sala"
    },

    en: {
        emptyValue: "—",
        failedWithReason: "Failed: {reason}",
        statusReady: "ready",
        statusSending: "sending...",
        statusLoggingIn: "logging in...",
        statusConnecting: "connecting...",
        statusConnected: "connected",
        statusDisconnected: "disconnected",
        statusReconnecting: "reconnecting...",
        statusConfirming: "confirming...",
        statusSaving: "saving...",

        login: "Sign in",
        loginSuccess: "Login successful",
        loginFailed: "Login failed",
        noToken: "No token received",

        email: "Email",
        password: "Password",
        logout: "Log out",

        loginSubtitle: "Access your account to continue",
        emailPlaceholder: "you@example.com",
        passwordPlaceholder: "Enter your password",

        fillEmail: "Enter your email",
        fillPassword: "Enter your password",
        fillCode: "Enter the code",

        minPassword: "Minimum password length: 6 characters",
        passwordsMismatch: "Passwords do not match",

        createAccount: "Create account",
        createSubtitle: "Fill in your details to receive a confirmation code",
        confirmSignupSubtitle: "Enter the code sent to",

        confirmPassword: "Confirm password",
        confirmPasswordPlaceholder: "Repeat your password",

        newPassword: "New password",
        newPasswordPlaceholder: "Minimum 6 characters",

        confirmNewPassword: "Confirm new password",
        confirmNewPasswordPlaceholder: "Repeat your password",

        completeSignup: "Complete sign up",

        createSendingCode: "Sending code...",
        createCodeSent: "Code sent",
        createAccountExists: "Account already exists",
        createResending: "Resending code...",
        createCodeResent: "Code resent",
        createConfirming: "Confirming...",
        createAccountCreated: "Account created successfully",

        forgotPassword: "Forgot password",
        forgotSubtitle: "Enter your email to receive a reset code",
        resetSubtitle: "Enter the code sent to",

        forgotSendingCode: "Sending code...",
        forgotCodeSent: "Code sent",
        forgotResending: "Resending code...",
        forgotCodeResent: "Code resent",
        forgotSaving: "Saving new password...",
        forgotPasswordUpdated: "Password updated successfully",

        backToLogin: "Back to sign in",
        sendCode: "Send code",
        confirmCode: "Confirm code",
        resetPassword: "Reset password",
        enterRoom: "Join room"
    },

    de: {
        emptyValue: "—",
        failedWithReason: "Fehlgeschlagen: {reason}",
        statusReady: "bereit",
        statusSending: "wird gesendet...",
        statusLoggingIn: "Anmeldung läuft...",
        statusConnecting: "verbinde...",
        statusConnected: "verbunden",
        statusDisconnected: "getrennt",
        statusReconnecting: "verbinde erneut...",
        statusConfirming: "wird bestätigt...",
        statusSaving: "wird gespeichert...",

        login: "Anmelden",
        loginSuccess: "Anmeldung erfolgreich",
        loginFailed: "Anmeldung fehlgeschlagen",
        noToken: "Kein Token erhalten",

        email: "E-Mail",
        password: "Passwort",
        logout: "Abmelden",

        loginSubtitle: "Melden Sie sich an, um fortzufahren",
        emailPlaceholder: "sie@beispiel.com",
        passwordPlaceholder: "Passwort eingeben",

        fillEmail: "E-Mail eingeben",
        fillPassword: "Passwort eingeben",
        fillCode: "Code eingeben",

        minPassword: "Mindestens 6 Zeichen",
        passwordsMismatch: "Passwörter stimmen nicht überein",

        createAccount: "Konto erstellen",
        createSubtitle: "Daten eingeben, um Code zu erhalten",
        confirmSignupSubtitle: "Code eingeben",

        confirmPassword: "Passwort bestätigen",
        confirmPasswordPlaceholder: "Passwort wiederholen",

        newPassword: "Neues Passwort",
        newPasswordPlaceholder: "Mindestens 6 Zeichen",

        confirmNewPassword: "Neues Passwort bestätigen",
        confirmNewPasswordPlaceholder: "Passwort wiederholen",

        completeSignup: "Registrierung abschließen",

        createSendingCode: "Code wird gesendet...",
        createCodeSent: "Code gesendet",
        createAccountExists: "Konto existiert bereits",
        createResending: "Code wird erneut gesendet...",
        createCodeResent: "Code erneut gesendet",
        createConfirming: "Wird bestätigt...",
        createAccountCreated: "Konto erfolgreich erstellt",

        forgotPassword: "Passwort vergessen",
        forgotSubtitle: "E-Mail eingeben, um Code zu erhalten",
        resetSubtitle: "Code eingeben",

        forgotSendingCode: "Code wird gesendet...",
        forgotCodeSent: "Code gesendet",
        forgotResending: "Code wird erneut gesendet...",
        forgotCodeResent: "Code erneut gesendet",
        forgotSaving: "Passwort wird gespeichert...",
        forgotPasswordUpdated: "Passwort erfolgreich geändert",

        backToLogin: "Zurück zur Anmeldung",
        sendCode: "Code senden",
        confirmCode: "Code bestätigen",
        resetPassword: "Passwort zurücksetzen",
        enterRoom: "Raum beitreten"
    }
};

// Detecta o idioma automaticamente
function detectLanguage() {
    const saved = localStorage.getItem("ss_lang");
    if (saved) return saved;

    const lang = (navigator.language || "en").toLowerCase();

    if (lang.startsWith("pt")) return "pt";
    if (lang.startsWith("de")) return "de";
    return "en";
}

// Retorna o idioma atual
function getCurrentLanguage() {
    return localStorage.getItem("ss_lang") || detectLanguage();
}

// Busca uma tradução pela chave
function t(key) {
    const lang = getCurrentLanguage();
    return translations[lang]?.[key] || translations.en?.[key] || key;
}

// Aplica o idioma na página
function applyLanguage(lang) {
    const dict = translations[lang] || translations.en;

    // Define o atributo lang no HTML
    document.documentElement.lang = lang;

    // Traduz textos com data-i18n
    document.querySelectorAll("[data-i18n]").forEach(el => {
        const key = el.dataset.i18n;
        if (key in dict) el.textContent = dict[key];
    });

    // Traduz placeholders com data-i18n-placeholder
    document.querySelectorAll("[data-i18n-placeholder]").forEach(el => {
        const key = el.dataset.i18nPlaceholder;
        if (key in dict) el.placeholder = dict[key];
    });

    // Salva idioma escolhido
    localStorage.setItem("ss_lang", lang);
}

// Inicializa seletor de idioma
function initLanguageSelector() {
    const select = document.getElementById("languageSelect");
    const lang = getCurrentLanguage();

    applyLanguage(lang);

    if (select) {
        select.value = lang;

        select.addEventListener("change", e => {
            applyLanguage(e.target.value);
            location.reload();
        });
    }
}

// Inicializa ao carregar a página
document.addEventListener("DOMContentLoaded", initLanguageSelector);