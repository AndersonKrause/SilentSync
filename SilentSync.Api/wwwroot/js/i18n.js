const translations = {
    pt: {
        room: "Sala",
        login: "Entrar",
        email: "Email",
        password: "Senha",
        logout: "Sair",
        createAccount: "Criar conta",
        forgotPassword: "Esqueci minha senha",
        sendCode: "Enviar código",
        confirmCode: "Confirmar código",
        resetPassword: "Redefinir senha",
        play: "Play",
        audio: "Áudio sincronizado",
        enterRoom: "Entrar na sala",

        loginSubtitle: "Acesse sua conta para continuar",
        emailPlaceholder: "voce@exemplo.com",
        passwordPlaceholder: "Digite sua senha",
        confirmPassword: "Confirmar senha",
        confirmPasswordPlaceholder: "Repita a senha",
        newPassword: "Nova senha",
        newPasswordPlaceholder: "Mínimo 6 caracteres",
        confirmNewPassword: "Confirmar nova senha",
        confirmNewPasswordPlaceholder: "Repita a senha",
        backToLogin: "Voltar para entrar",
        completeSignup: "Concluir cadastro",
        forgotSubtitle: "Informe seu email para receber um código de redefinição",
        createSubtitle: "Preencha seus dados para receber um código de confirmação",
        confirmSignupSubtitle: "Digite o código enviado para",
        resetSubtitle: "Digite o código enviado para"
    },

    en: {
        room: "Room",
        login: "Sign in",
        email: "Email",
        password: "Password",
        logout: "Log out",
        createAccount: "Create account",
        forgotPassword: "Forgot password",
        sendCode: "Send code",
        confirmCode: "Confirm code",
        resetPassword: "Reset password",
        play: "Play",
        audio: "Synced audio",
        enterRoom: "Join room",

        loginSubtitle: "Access your account to continue",
        emailPlaceholder: "you@example.com",
        passwordPlaceholder: "Enter your password",
        confirmPassword: "Confirm password",
        confirmPasswordPlaceholder: "Repeat your password",
        newPassword: "New password",
        newPasswordPlaceholder: "Minimum 6 characters",
        confirmNewPassword: "Confirm new password",
        confirmNewPasswordPlaceholder: "Repeat your password",
        backToLogin: "Back to sign in",
        completeSignup: "Complete sign up",
        forgotSubtitle: "Enter your email to receive a reset code",
        createSubtitle: "Fill in your details to receive a confirmation code",
        confirmSignupSubtitle: "Enter the code sent to",
        resetSubtitle: "Enter the code sent to"
    },

    de: {
        room: "Raum",
        login: "Anmelden",
        email: "E-Mail",
        password: "Passwort",
        logout: "Abmelden",
        createAccount: "Konto erstellen",
        forgotPassword: "Passwort vergessen",
        sendCode: "Code senden",
        confirmCode: "Code bestätigen",
        resetPassword: "Passwort zurücksetzen",
        play: "Play",
        audio: "Synchroner Ton",
        enterRoom: "Raum beitreten",

        loginSubtitle: "Melden Sie sich an, um fortzufahren",
        emailPlaceholder: "sie@beispiel.com",
        passwordPlaceholder: "Geben Sie Ihr Passwort ein",
        confirmPassword: "Passwort bestätigen",
        confirmPasswordPlaceholder: "Passwort wiederholen",
        newPassword: "Neues Passwort",
        newPasswordPlaceholder: "Mindestens 6 Zeichen",
        confirmNewPassword: "Neues Passwort bestätigen",
        confirmNewPasswordPlaceholder: "Passwort wiederholen",
        backToLogin: "Zurück zur Anmeldung",
        completeSignup: "Registrierung abschließen",
        forgotSubtitle: "Geben Sie Ihre E-Mail ein, um einen Code zu erhalten",
        createSubtitle: "Geben Sie Ihre Daten ein, um einen Bestätigungscode zu erhalten",
        confirmSignupSubtitle: "Geben Sie den Code ein, der gesendet wurde an",
        resetSubtitle: "Geben Sie den gesendeten Code ein"
    }
};

function detectLanguage() {
    const saved = localStorage.getItem("ss_lang");
    if (saved) return saved;

    const lang = (navigator.language || "en").toLowerCase();

    if (lang.startsWith("pt")) return "pt";
    if (lang.startsWith("de")) return "de";
    return "en";
}

function applyLanguage(lang) {
    const dict = translations[lang] || translations.en;

    document.documentElement.lang = lang;

    document.querySelectorAll("[data-i18n]").forEach(el => {
        const key = el.dataset.i18n;
        if (dict[key]) el.textContent = dict[key];
    });

    document.querySelectorAll("[data-i18n-placeholder]").forEach(el => {
        const key = el.dataset.i18nPlaceholder;
        if (dict[key]) el.placeholder = dict[key];
    });

    localStorage.setItem("ss_lang", lang);
}
function initLanguageSelector() {
    const select = document.getElementById("languageSelect");

    const lang = detectLanguage();
    applyLanguage(lang);

    if (select) {
        select.value = lang;
        select.addEventListener("change", e => {
            applyLanguage(e.target.value);
        });
    }
}

document.addEventListener("DOMContentLoaded", initLanguageSelector);