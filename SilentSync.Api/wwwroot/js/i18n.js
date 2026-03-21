const translations = {
    pt: {
        // ===== COMMON =====
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

        // ===== REGISTER =====
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

        forgotPassword: "Esqueci minha senha",
        forgotSubtitle: "Informe seu email para receber um código de redefinição",
        resetSubtitle: "Digite o código enviado para",

        forgotSendingCode: "Enviando código...",
        forgotCodeSent: "Código enviado",
        forgotResending: "Reenviando código...",
        forgotCodeResent: "Código reenviado",
        forgotSaving: "Salvando nova senha...",
        forgotPasswordUpdated: "Senha atualizada com sucesso",

        backToLogin: "Voltar para entrar",
        sendCode: "Enviar código",
        confirmCode: "Confirmar código",
        resetPassword: "Redefinir senha",
        enterRoom: "Entrar na sala",

        // ===== SCREEN =====
        openScreenWithRoom: "Abra como /pages/screen.html?room=ABC123",
        roomLabel: "ROOM",
        screenQrHintLocalhost: "Abra o telão pelo IP para o QR funcionar no celular",
        screenQrHintDefault: "Escaneie para entrar e ouvir o áudio no celular",

        // ===== MOBILE =====
        loggedInOk: "Logado",
        notLoggedRedirecting: "Não logado. Redirecionando para o Registro...",
        fillRoomCode: "Preencha o código da sala",
        fillDisplayName: "Preencha o nome",
        joiningRoom: "Entrando na sala...",
        joinedOk: "Entrou",
        guestName: "Convidado",
        autoJoiningRoom: "Entrando automaticamente na sala...",
        connectedOk: "Conectado",
        autoJoinFailed: "Auto-join falhou: {reason}",
        waitingAudioUrl: "aguardando audioUrl no PlayerState...",
        audioStillLoading: "áudio ainda carregando, esperando...",
        autoplayBlocked: "autoplay bloqueado:",
        manualPlayOk: "play manual OK",
        manualPlayFailed: "play manual falhou:",

        // ===== ADMIN / CONTROLLER =====
        failedLoadCurrentUser: "Falha ao carregar o usuário atual.",
        userInfoText: "Email: {email}, Função: {role}",
        adminNoPermission: "Você não tem permissão para acessar o Admin.",

        creatingRoom: "Criando...",
        loginBeforeCreateRoom: "Faça login antes de criar uma room.",
        sessionExpired: "Sua sessão expirou. Faça login novamente.",
        noPermissionCreateRooms: "Seu usuário não tem permissão para criar rooms.",
        responseWithoutCode: "Resposta sem code.",
        failedCreateRoom: "Falhou ao criar room: {reason}",

        createRoomOpenScreen: "Criar Room (abre Telão)",
        openScreen: "Abrir Telão",
        emptyRoomCode: "Room Code vazio",
        roomCode: "Room Code",
        roomTitle: "Sala (Room)",
        roomCodePlaceholder: "6 Digits Code",
        screenLinkLabel: "Link Telão:",
        registerLinkLabel: "Link Registro (QR):",
        registerQrAlt: "QR Registro",
        publicOriginHint: "Importante: os links/QR aqui sempre usam a origem pública atual.",

        copied: "Copiado",
        copyRegisterLink: "Copiar link do Registro",
        copyRegisterLinkPrompt: "Copie o link do Registro:",

        mediaConnectionTitle: "Mídia + Conexão",
        selectVideoLocal: "Selecione o vídeo (local)",
        mediaFlowHint: "Fluxo: Criar Room → selecione vídeo → Upload → Conectar → Play/Pause/Seek.",
        uploadBtnLabel: "Upload (salvar vídeo + gerar áudio)",
        audioUrlLabel: "Audio URL (gerada automaticamente)",
        audioUrlPlaceholder: "(preenchida após upload)",
        videoUrlLabel: "Video URL (gerada automaticamente)",
        videoUrlPlaceholder: "(preenchida após upload)",
        connectController: "Conectar (controller)",
        openScreenHint: "Dica: abra o telão em outra aba.",

        controlsTitle: "Controles",
        play: "Play",
        pause: "Pause",
        seek: "Seek",
        seekLabel: "Seek (ms)",
        seekPlaceholder: "ex: 83400",
        adminPreviewHint: "O vídeo no admin é só preview. O telão toca o vídeo via URL enviada no estado.",

        selectVideoFirst: "Selecione um vídeo primeiro.",
        uploadingAndProcessing: "Enviando e processando...",
        networkError: "Erro de rede",

        uploadOkAudioVideoReady: "OK! áudio + vídeo prontos",
        uploadOkMissingPaths: "Upload OK, mas faltou audioPath/videoPath na resposta.",

        uploadFirstNeedAudioVideo: "Faça o upload primeiro (precisamos de Audio e Video URL).",
        failedConnectController: "Falha ao conectar controller: {reason}",
        invalidSeek: "Seek inválido",

        adminTitle: "Administrador",
        adminSubtitle: "Gerencie a sala, envie mídia e controle a reprodução.",

        audio: "Áudio sincronizado",
        room: "Sala"
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
        enterRoom: "Join room",

        openScreenWithRoom: "Open as /pages/screen.html?room=ABC123",
        roomLabel: "ROOM",
        screenQrHintLocalhost: "Open screen via IP for mobile QR",
        screenQrHintDefault: "Scan to join and hear audio",

        loggedInOk: "Logged in",
        notLoggedRedirecting: "Not logged in. Redirecting to Register...",
        fillRoomCode: "Enter room code",
        fillDisplayName: "Enter your name",
        joiningRoom: "Joining room...",
        joinedOk: "Joined",
        guestName: "Guest",
        autoJoiningRoom: "Joining automatically...",
        connectedOk: "Connected",
        autoJoinFailed: "Auto-join failed: {reason}",
        waitingAudioUrl: "waiting for audioUrl in PlayerState...",
        audioStillLoading: "audio still loading, waiting...",
        autoplayBlocked: "autoplay blocked:",
        manualPlayOk: "manual play OK",
        manualPlayFailed: "manual play failed:",

        failedLoadCurrentUser: "Failed to load current user.",
        userInfoText: "Email: {email}, Role: {role}",
        adminNoPermission: "You do not have permission to access Admin.",

        creatingRoom: "Creating...",
        loginBeforeCreateRoom: "Please sign in before creating a room.",
        sessionExpired: "Your session has expired. Please sign in again.",
        noPermissionCreateRooms: "You do not have permission to create rooms.",
        responseWithoutCode: "Response without code.",
        failedCreateRoom: "Failed to create room: {reason}",

        createRoomOpenScreen: "Create Room (opens Screen)",
        openScreen: "Open Screen",
        emptyRoomCode: "Empty room code",
        roomCode: "Room Code",
        roomTitle: "Room",
        roomCodePlaceholder: "6 Digits Code",
        screenLinkLabel: "Screen Link:",
        registerLinkLabel: "Register Link (QR):",
        registerQrAlt: "Register QR",
        publicOriginHint: "Important: links/QR here always use the current public origin.",

        copied: "Copied",
        copyRegisterLink: "Copy register link",
        copyRegisterLinkPrompt: "Copy the register link:",

        mediaConnectionTitle: "Media + Connection",
        selectVideoLocal: "Select the video (local)",
        mediaFlowHint: "Flow: Create Room → select video → Upload → Connect → Play/Pause/Seek.",
        uploadBtnLabel: "Upload (save video + generate audio)",
        audioUrlLabel: "Audio URL (generated automatically)",
        audioUrlPlaceholder: "(filled after upload)",
        videoUrlLabel: "Video URL (generated automatically)",
        videoUrlPlaceholder: "(filled after upload)",
        connectController: "Connect (controller)",
        openScreenHint: "Tip: open the screen in another tab.",

        controlsTitle: "Controls",
        play: "Play",
        pause: "Pause",
        seek: "Seek",
        seekLabel: "Seek (ms)",
        seekPlaceholder: "e.g.: 83400",
        adminPreviewHint: "The admin video is only a preview. The screen plays the video via the URL sent in state.",

        selectVideoFirst: "Select a video first.",
        uploadingAndProcessing: "Uploading and processing...",
        networkError: "Network error",

        uploadOkAudioVideoReady: "OK! audio + video ready",
        uploadOkMissingPaths: "Upload OK, but audioPath/videoPath was missing in the response.",

        uploadFirstNeedAudioVideo: "Upload first (audio and video URL are required).",
        failedConnectController: "Failed to connect controller: {reason}",
        invalidSeek: "Invalid seek",

        adminTitle: "Admin",
        adminSubtitle: "Manage the room, upload media and control playback.",

        audio: "Synced audio",
        room: "Room"
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
        enterRoom: "Raum beitreten",

        openScreenWithRoom: "Öffnen mit /pages/screen.html?room=ABC123",
        roomLabel: "ROOM",
        screenQrHintLocalhost: "Bildschirm über IP öffnen, damit QR auf dem Handy funktioniert",
        screenQrHintDefault: "Scannen, um beizutreten und den Ton auf dem Handy zu hören",

        loggedInOk: "Eingeloggt",
        notLoggedRedirecting: "Nicht eingeloggt. Weiterleitung zur Registrierung...",
        fillRoomCode: "Room Code eingeben",
        fillDisplayName: "Name eingeben",
        joiningRoom: "Raum wird betreten...",
        joinedOk: "Beigetreten",
        guestName: "Gast",
        autoJoiningRoom: "Automatisches Beitreten...",
        connectedOk: "Verbunden",
        autoJoinFailed: "Auto-Join fehlgeschlagen: {reason}",
        waitingAudioUrl: "warte auf audioUrl im PlayerState...",
        audioStillLoading: "Audio lädt noch, bitte warten...",
        autoplayBlocked: "Autoplay blockiert:",
        manualPlayOk: "manuelles Abspielen OK",
        manualPlayFailed: "manuelles Abspielen fehlgeschlagen:",

        failedLoadCurrentUser: "Benutzer konnte nicht geladen werden.",
        userInfoText: "E-Mail: {email}, Rolle: {role}",
        adminNoPermission: "Sie haben keine Berechtigung, auf Admin zuzugreifen.",

        creatingRoom: "Wird erstellt...",
        loginBeforeCreateRoom: "Bitte vor dem Erstellen eines Raums anmelden.",
        sessionExpired: "Ihre Sitzung ist abgelaufen. Bitte erneut anmelden.",
        noPermissionCreateRooms: "Sie haben keine Berechtigung, Räume zu erstellen.",
        responseWithoutCode: "Antwort ohne Code.",
        failedCreateRoom: "Raum konnte nicht erstellt werden: {reason}",

        createRoomOpenScreen: "Raum erstellen (öffnet Bildschirm)",
        openScreen: "Bildschirm öffnen",
        emptyRoomCode: "Leerer Room Code",
        roomCode: "Room Code",
        roomTitle: "Raum",
        roomCodePlaceholder: "6-stelliger Code",
        screenLinkLabel: "Bildschirm-Link:",
        registerLinkLabel: "Registrierungs-Link (QR):",
        registerQrAlt: "Registrierungs-QR",
        publicOriginHint: "Wichtig: Links/QR verwenden hier immer die aktuelle öffentliche Origin.",

        copied: "Kopiert",
        copyRegisterLink: "Registrierungslink kopieren",
        copyRegisterLinkPrompt: "Registrierungslink kopieren:",

        mediaConnectionTitle: "Medien + Verbindung",
        selectVideoLocal: "Video auswählen (lokal)",
        mediaFlowHint: "Ablauf: Raum erstellen → Video auswählen → Upload → Verbinden → Play/Pause/Seek.",
        uploadBtnLabel: "Upload (Video speichern + Audio erzeugen)",
        audioUrlLabel: "Audio-URL (automatisch erzeugt)",
        audioUrlPlaceholder: "(wird nach Upload ausgefüllt)",
        videoUrlLabel: "Video-URL (automatisch erzeugt)",
        videoUrlPlaceholder: "(wird nach Upload ausgefüllt)",
        connectController: "Verbinden (Controller)",
        openScreenHint: "Tipp: Öffnen Sie den Bildschirm in einem anderen Tab.",

        controlsTitle: "Steuerung",
        play: "Play",
        pause: "Pause",
        seek: "Seek",
        seekLabel: "Seek (ms)",
        seekPlaceholder: "z. B.: 83400",
        adminPreviewHint: "Das Video im Admin ist nur eine Vorschau. Der Bildschirm spielt das Video über die im Status gesendete URL ab.",

        selectVideoFirst: "Bitte zuerst ein Video auswählen.",
        uploadingAndProcessing: "Wird hochgeladen und verarbeitet...",
        networkError: "Netzwerkfehler",

        uploadOkAudioVideoReady: "OK! Audio + Video bereit",
        uploadOkMissingPaths: "Upload OK, aber audioPath/videoPath fehlt in der Antwort.",

        uploadFirstNeedAudioVideo: "Bitte zuerst hochladen (Audio- und Video-URL erforderlich).",
        failedConnectController: "Controller-Verbindung fehlgeschlagen: {reason}",
        invalidSeek: "Ungültiger Seek",

        adminTitle: "Admin",
        adminSubtitle: "Verwalten Sie den Raum, laden Sie Medien hoch und steuern Sie die Wiedergabe.",
        
        audio: "Synchronisierter Ton",
        room: "Raum"
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

function getCurrentLanguage() {
    return localStorage.getItem("ss_lang") || detectLanguage();
}

function t(key) {
    const lang = getCurrentLanguage();
    return translations[lang]?.[key] || translations.en?.[key] || key;
}

function tf(key, vars = {}) {
    let text = t(key);

    for (const [k, v] of Object.entries(vars)) {
        text = text.replaceAll(`{${k}}`, v);
    }

    return text;
}

function applyLanguage(lang) {
    const dict = translations[lang] || translations.en;

    document.documentElement.lang = lang;

    document.querySelectorAll("[data-i18n]").forEach(el => {
        const key = el.dataset.i18n;
        if (key in dict) el.textContent = dict[key];
    });

    document.querySelectorAll("[data-i18n-placeholder]").forEach(el => {
        const key = el.dataset.i18nPlaceholder;
        if (key in dict) el.placeholder = dict[key];
    });

    document.querySelectorAll("[data-i18n-title]").forEach(el => {
        const key = el.dataset.i18nTitle;
        if (key in dict) el.title = dict[key];
    });

    document.querySelectorAll("[data-i18n-alt]").forEach(el => {
        const key = el.dataset.i18nAlt;
        if (key in dict) el.alt = dict[key];
    });

    localStorage.setItem("ss_lang", lang);
}

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

document.addEventListener("DOMContentLoaded", initLanguageSelector);