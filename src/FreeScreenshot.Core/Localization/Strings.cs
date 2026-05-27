using System.Collections.Generic;
using System.Globalization;

namespace FreeScreenshot.Core.Localization;

/// <summary>
/// Tiny, dependency-free i18n. Picks the system UI language (CA / ES / EN)
/// at startup; everything else is hardcoded English-fallback.
/// </summary>
public static class Strings
{
    public const string DonationUrl = "https://freedolia.com/donate";
    public const string SupportUrl  = "https://freedolia.com/freescreenshot";

    public static string Current { get; private set; } = "en";

    public static IReadOnlyList<string> Available => new[] { "ca", "es", "en" };

    /// <summary>Pick best language from system, then load it.</summary>
    public static void InitFromSystem()
    {
        var two = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
        SetLang(All.ContainsKey(two) ? two : "en");
    }

    public static void SetLang(string lang) => Current = All.ContainsKey(lang) ? lang : "en";

    public static string T(string key)
    {
        if (All.TryGetValue(Current, out var dict) && dict.TryGetValue(key, out var v)) return v;
        if (All["en"].TryGetValue(key, out var fallback)) return fallback;
        return key;
    }

    // ---- the dictionary ----
    private static readonly Dictionary<string, Dictionary<string, string>> All = new()
    {
        ["ca"] = new()
        {
            ["app.tagline"] = "Captures sense embuts.",
            ["tray.tooltip"] = "FreeScreenshot",
            ["tray.balloon.title"] = "FreeScreenshot",
            ["tray.balloon.message"] = "Visc a la safata. Clic dret sobre la icona per opcions.",
            ["menu.settings"] = "Configuració…",
            ["menu.about"] = "Quant a",
            ["menu.donate"] = "Donar",
            ["menu.quit"] = "Sortir",
            ["settings.title"] = "FreeScreenshot — Configuració",
            ["settings.nav.general"] = "General",
            ["settings.nav.privacy"] = "Privacitat",
            ["settings.nav.about"] = "Quant a",
            ["settings.general.heading"] = "General",
            ["settings.general.sub"] = "Comportament de l'app a Windows.",
            ["settings.general.lang"] = "Idioma",
            ["settings.general.placeholder"] = "Aviat: dreceres, format de sortida, carpeta de captures.",
            ["settings.privacy.heading"] = "Privacitat",
            ["settings.privacy.sub"] = "Tres senyals opt-in que FreeScreenshot envia a freedolia.com. Pots desactivar-los aquí.",
            ["settings.privacy.toggle.title"] = "Compartir dades anònimes amb freedolia.com",
            ["settings.privacy.toggle.desc"] = "Inclou l'avís d'instal·lació, la comprovació silenciosa de versions noves i l'enquesta opcional en desinstal·lar.",
            ["settings.privacy.install_id_label"] = "Identificador local d'instal·lació:",
            ["settings.privacy.open_policy"] = "Llegir política de privadesa",
            ["settings.about.heading"] = "Quant a",
            ["settings.about.sub"] = "freescreenshot · una eina de freedolia.com",
            ["settings.about.version"] = "Versió",
            ["settings.about.license"] = "Llicència",
            ["settings.about.code"] = "Codi",
            ["settings.about.donate"] = "Si t'agrada, suporta el projecte",
            ["settings.about.donate.btn"] = "Donar via Stripe",
            ["uninstall.title"] = "Per què desinstal·les FreeScreenshot?",
            ["uninstall.sub"] = "Opcional. Triga 5 segons i ens ajudaràs a millorar.",
            ["uninstall.reason.no_what_expected"] = "No és el que esperava",
            ["uninstall.reason.missing_feature"] = "Em falta una funció que necessito",
            ["uninstall.reason.too_slow"] = "Massa lent o pesat",
            ["uninstall.reason.found_alternative"] = "He trobat una alternativa millor",
            ["uninstall.reason.not_using"] = "No el faig servir",
            ["uninstall.reason.bugs"] = "Errors o crashes",
            ["uninstall.reason.temporary"] = "Reinstal·lo / temporal",
            ["uninstall.reason.other"] = "Altres",
            ["uninstall.note.placeholder"] = "Algun comentari? (opcional)",
            ["uninstall.send"] = "Enviar i tancar",
            ["uninstall.skip"] = "Saltar",
            ["common.close"] = "Tancar",
        },
        ["es"] = new()
        {
            ["app.tagline"] = "Capturas sin enredos.",
            ["tray.tooltip"] = "FreeScreenshot",
            ["tray.balloon.title"] = "FreeScreenshot",
            ["tray.balloon.message"] = "Vivo en la bandeja. Clic derecho sobre el icono para opciones.",
            ["menu.settings"] = "Configuración…",
            ["menu.about"] = "Acerca de",
            ["menu.donate"] = "Donar",
            ["menu.quit"] = "Salir",
            ["settings.title"] = "FreeScreenshot — Configuración",
            ["settings.nav.general"] = "General",
            ["settings.nav.privacy"] = "Privacidad",
            ["settings.nav.about"] = "Acerca de",
            ["settings.general.heading"] = "General",
            ["settings.general.sub"] = "Comportamiento de la app en Windows.",
            ["settings.general.lang"] = "Idioma",
            ["settings.general.placeholder"] = "Pronto: atajos, formato de salida, carpeta de capturas.",
            ["settings.privacy.heading"] = "Privacidad",
            ["settings.privacy.sub"] = "Tres señales opt-in que FreeScreenshot envía a freedolia.com. Puedes desactivarlas aquí.",
            ["settings.privacy.toggle.title"] = "Compartir datos anónimos con freedolia.com",
            ["settings.privacy.toggle.desc"] = "Incluye el aviso de instalación, la comprobación silenciosa de versiones nuevas y la encuesta opcional al desinstalar.",
            ["settings.privacy.install_id_label"] = "Identificador local de instalación:",
            ["settings.privacy.open_policy"] = "Leer política de privacidad",
            ["settings.about.heading"] = "Acerca de",
            ["settings.about.sub"] = "freescreenshot · una herramienta de freedolia.com",
            ["settings.about.version"] = "Versión",
            ["settings.about.license"] = "Licencia",
            ["settings.about.code"] = "Código",
            ["settings.about.donate"] = "Si te gusta, apoya el proyecto",
            ["settings.about.donate.btn"] = "Donar vía Stripe",
            ["uninstall.title"] = "¿Por qué desinstalas FreeScreenshot?",
            ["uninstall.sub"] = "Opcional. Tarda 5 segundos y nos ayudas a mejorar.",
            ["uninstall.reason.no_what_expected"] = "No es lo que esperaba",
            ["uninstall.reason.missing_feature"] = "Me falta una función que necesito",
            ["uninstall.reason.too_slow"] = "Demasiado lento o pesado",
            ["uninstall.reason.found_alternative"] = "Encontré una alternativa mejor",
            ["uninstall.reason.not_using"] = "No lo uso",
            ["uninstall.reason.bugs"] = "Errores o crashes",
            ["uninstall.reason.temporary"] = "Reinstalo / temporal",
            ["uninstall.reason.other"] = "Otros",
            ["uninstall.note.placeholder"] = "¿Algún comentario? (opcional)",
            ["uninstall.send"] = "Enviar y cerrar",
            ["uninstall.skip"] = "Saltar",
            ["common.close"] = "Cerrar",
        },
        ["en"] = new()
        {
            ["app.tagline"] = "Screenshots without the fuss.",
            ["tray.tooltip"] = "FreeScreenshot",
            ["tray.balloon.title"] = "FreeScreenshot",
            ["tray.balloon.message"] = "I live in the tray. Right-click the icon for options.",
            ["menu.settings"] = "Settings…",
            ["menu.about"] = "About",
            ["menu.donate"] = "Donate",
            ["menu.quit"] = "Quit",
            ["settings.title"] = "FreeScreenshot — Settings",
            ["settings.nav.general"] = "General",
            ["settings.nav.privacy"] = "Privacy",
            ["settings.nav.about"] = "About",
            ["settings.general.heading"] = "General",
            ["settings.general.sub"] = "How the app behaves on Windows.",
            ["settings.general.lang"] = "Language",
            ["settings.general.placeholder"] = "Coming soon: shortcuts, output format, capture folder.",
            ["settings.privacy.heading"] = "Privacy",
            ["settings.privacy.sub"] = "Three opt-in signals FreeScreenshot sends to freedolia.com. You can turn them off here.",
            ["settings.privacy.toggle.title"] = "Share anonymous data with freedolia.com",
            ["settings.privacy.toggle.desc"] = "Includes the install ping, the silent version check and the optional uninstall survey.",
            ["settings.privacy.install_id_label"] = "Local install identifier:",
            ["settings.privacy.open_policy"] = "Read privacy policy",
            ["settings.about.heading"] = "About",
            ["settings.about.sub"] = "freescreenshot · a freedolia.com tool",
            ["settings.about.version"] = "Version",
            ["settings.about.license"] = "License",
            ["settings.about.code"] = "Code",
            ["settings.about.donate"] = "If it helps you, support the project",
            ["settings.about.donate.btn"] = "Donate via Stripe",
            ["uninstall.title"] = "Why are you uninstalling FreeScreenshot?",
            ["uninstall.sub"] = "Optional. Takes 5 seconds and helps us improve.",
            ["uninstall.reason.no_what_expected"] = "Not what I expected",
            ["uninstall.reason.missing_feature"] = "A feature I need is missing",
            ["uninstall.reason.too_slow"] = "Too slow or heavy",
            ["uninstall.reason.found_alternative"] = "Found a better alternative",
            ["uninstall.reason.not_using"] = "I don't use it",
            ["uninstall.reason.bugs"] = "Bugs or crashes",
            ["uninstall.reason.temporary"] = "Reinstalling / temporary",
            ["uninstall.reason.other"] = "Other",
            ["uninstall.note.placeholder"] = "Any comment? (optional)",
            ["uninstall.send"] = "Send and close",
            ["uninstall.skip"] = "Skip",
            ["common.close"] = "Close",
        },
    };
}
