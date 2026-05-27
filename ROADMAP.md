# FreeScreenshot — Pla Tècnic i Roadmap

**Projecte:** Eina de captura de pantalla per Windows, inspirada en la usabilitat de CleanShot X (Mac).
**Nom definitiu:** FreeScreenshot
**Model:** Gratuït + donacions, **open source**. Cloud opcional via versió Premium amb **BYOC (Bring Your Own Cloud)**.
**Llicència objectiu:** Producte propi (codi, disseny i nom propis — sense calcar la UI de CleanShot).
**Document:** v0.2 — 27 maig 2026

---

## 1. Decisió de stack

**Triat: C# + WPF + .NET 8**

Per què aquest i no un altre:

| Stack | Pros | Contres | Veredicte |
|---|---|---|---|
| **C# + WPF + .NET 8** | API natives de Windows (Graphics.Capture, Media.Ocr, RegisterHotKey), .exe petit, rendiment alt, integració total amb safata, portapapers, hotkeys globals | Corba si no coneixes XAML | **Triat** |
| Python + PyQt6 | Desenvolupament molt ràpid, fàcil OCR amb Tesseract | .exe pesat (PyInstaller), captura de pantalla menys eficient, hotkeys globals tediosos | Bo per prototip, dolent per producte final |
| Electron + JS | Bonic ràpid, ecosistema web | 150MB+ RAM en idle, captura de pantalla amb limitacions, no és el que vols per una eina que ha de ser ràpida | Descartat |
| Rust + Tauri | Petit i ràpid | Ecosistema immadur per Windows desktop APIs, més temps invertit | Descartat per ara |

**Raó decisiva:** una eina de captura ha d'arrencar instantàniament, consumir mínim RAM en idle (vius a la safata 24/7) i tenir accés natiu a APIs de Windows. C#/WPF guanya en tots tres.

---

## 2. Funcionalitats i com s'implementen

### Bloc A — Captura (MVP)
| Funció | Com es fa | Llibreria / API |
|---|---|---|
| Captura d'àrea selectivable | Overlay fullscreen translúcid + selecció arrossegant | `Windows.Graphics.Capture` (Win10 1903+) |
| Captura de finestra | Detecció de finestres actives | `User32.dll` (P/Invoke: `EnumWindows`, `GetWindowRect`) |
| Captura de pantalla sencera / monitor concret | API directe | `Windows.Graphics.Capture` |
| Captura amb scroll (pàgina llarga) | Stitching d'imatges automàtic | Implementació pròpia + `OpenCvSharp` per alinear |
| Hotkeys globals | Tecles configurables des de qualsevol app | `RegisterHotKey` (Win32) o `NHotkey.Wpf` |

### Bloc B — Editor post-captura (MVP)
| Funció | Implementació |
|---|---|
| Fletxes, rectangles, cercles, línies | Canvas WPF amb `Shape` elements |
| Text amb font configurable | `TextBox` editable sobre canvas |
| Blur / pixelat (per ocultar dades) | `WriteableBitmapEx` per processament de píxels |
| Highlights | Rectangle semitransparent amb blend mode |
| Crop | Selecció + retall del bitmap |
| Desfer/refer | Pila de commands (patró Command) |

### Bloc C — Sortida (MVP)
| Funció | Implementació |
|---|---|
| Copiar al portapapers (PNG) | `Clipboard.SetImage()` |
| Guardar a disc (PNG, JPG, WebP) | `System.Drawing.Imaging` + `SixLabors.ImageSharp` per WebP |
| Format de nom personalitzable | Plantilla amb `{date}`, `{time}`, `{counter}` |

### Bloc D — Avançades (v1)
| Funció | Implementació |
|---|---|
| Gravació de vídeo / GIF | `FFmpeg` (via `Xabe.FFmpeg`) + `Windows.Graphics.Capture` per frames |
| OCR (extreure text) | `Windows.Media.Ocr` (natiu, sense dependències externes) |
| Pinned screenshots (captures flotants) | Finestra WPF `Topmost=true` amb la imatge |
| Historial de captures | SQLite local (`Microsoft.Data.Sqlite`) + miniatures |
| Anotacions amb numeració (1,2,3...) | Element gràfic propi al canvas |

### Bloc E — Cloud (v2)
| Funció | Implementació |
|---|---|
| Upload + URL curta | Backend propi (opcions: Cloudflare R2 + Workers, o Supabase Storage) |
| Sincronització multi-dispositiu | Compte d'usuari + API REST |
| Self-hosted | Suport per S3 compatible (Backblaze, MinIO) |

---

## 3. Arquitectura

```
FreeScreenshot/
├── FreeScreenshot.Core/         # Lògica pura, sense UI
│   ├── Capture/                 # Motor de captura (Graphics.Capture wrapper)
│   ├── Editor/                  # Model de capes, commands, undo/redo
│   ├── Storage/                 # SQLite, fitxers, configuració
│   └── Hotkeys/                 # Registre i gestió d'atalls globals
│
├── FreeScreenshot.UI/           # WPF — interfície
│   ├── Views/                   # Finestres XAML
│   │   ├── OverlayWindow.xaml   # Selecció d'àrea
│   │   ├── EditorWindow.xaml    # Editor post-captura
│   │   ├── PinnedWindow.xaml    # Captures flotants
│   │   └── SettingsWindow.xaml
│   ├── ViewModels/              # MVVM
│   └── Controls/                # Components reutilitzables
│
├── FreeScreenshot.Tray/         # App icon a la safata (entry point)
│   └── App.xaml.cs              # Single-instance, autostart
│
├── FreeScreenshot.Cloud/        # (v2) Client API per upload
│
└── FreeScreenshot.Tests/        # xUnit
```

**Patrons clau:**
- **MVVM** per separar UI de lògica
- **Command pattern** per undo/redo a l'editor
- **DI** amb `Microsoft.Extensions.DependencyInjection`
- **Single instance** — només una app corrent, segona invocació envia a la primera

---

## 4. Roadmap per fases

### Fase 0 — Preparació (Setmana 1)
- Instal·lar Visual Studio 2022 / Rider
- Crear solució amb projectes buits
- Configurar repo Git, CI bàsic (GitHub Actions amb build .exe)
- Decidir disseny visual propi (NO copiar CleanShot — fer mockups Figma o similar)

### Fase 1 — MVP (Setmanes 2–5)
**Objectiu:** captura + editor bàsic + guardar/copiar funcional
- [ ] Captura d'àrea amb overlay
- [ ] Captura de pantalla sencera
- [ ] Hotkeys globals configurables (PrintScreen, Ctrl+Shift+1, etc.)
- [ ] Editor amb: fletxa, rectangle, text, blur, crop
- [ ] Desfer/refer
- [ ] Copiar a portapapers + guardar a fitxer
- [ ] Icona safata + menú dret
- [ ] Configuració bàsica (carpeta de sortida, format)

**Entrega:** `.msi` instal·lable, funcional 100% offline.

### Fase 2 — Polit (Setmanes 6–8)
- [ ] Captura de finestra concreta amb hover
- [ ] Multi-monitor amb DPI correcte
- [ ] Numeració d'anotacions (1, 2, 3...)
- [ ] Pinned screenshots
- [ ] Historial local amb miniatures
- [ ] Plantilles de nom de fitxer
- [ ] Tema clar/fosc

### Fase 3 — Vídeo i OCR (Setmanes 9–12)
- [ ] Gravació de vídeo MP4
- [ ] Gravació de GIF
- [ ] Mostrar clics i tecles durant gravació
- [ ] OCR amb còpia directa a portapapers
- [ ] Captura amb scroll

### Fase 4 — Cloud Premium opcional (Setmanes 13–18) — DIFERIT
**Decisió:** versió base 100% gratuïta i offline. Cloud només si surt versió Premium amb model BYOC (l'usuari hi posa el seu propi storage).
- [ ] Integració S3 compatible (Backblaze B2, MinIO, Wasabi, Cloudflare R2, AWS S3)
- [ ] Configuració des de Settings (endpoint, bucket, claus)
- [ ] Upload + URL curta (signed URL del propi bucket)
- [ ] Drop-in providers: Imgur, Dropbox, Google Drive com a alternatives gratis
- [ ] Sense backend propi mantenint = zero cost operatiu

### Fase 5 — Distribució (paral·lel a F3-F4)
- [ ] Signatura de codi (necessari per evitar avís Windows SmartScreen) — uns 200-400€/any
- [ ] Auto-updater (`Velopack` o `Squirrel.Windows`)
- [ ] Pàgina web simple
- [ ] Decidir model: gratuït, donació, llicència de pagament

---

## 5. Riscos i decisions

### Riscos legals — CRÍTIC perquè anem open source
Sent el codi públic, qualsevol pot comparar amb CleanShot. Per tant:

**Es pot fer (legal):**
- Replicar el **flux d'ús** i els **principis UX**: drag-to-select amb dimensions, toolbar flotant post-captura, editor amb anotacions minimalistes, dark mode polit, accions a un clic.
- Inspirar-se en l'**organització funcional** (eines d'anotació a dalt, exportació a baix, etc.).
- Imitar la **sensació de velocitat i minimalisme**.

**No es pot fer (risc legal — trade dress / copyright UI):**
- Calcar la UI **pixel a pixel** ni capturar disposicions idèntiques d'elements.
- Reusar **icones específiques** de CleanShot → usar packs lliures (Lucide, Fluent UI System Icons, Phosphor, Tabler).
- Copiar **paleta de colors exacta** ni tipografia com a identitat visual.
- Usar el **nom "CleanShot"** ni variants ("CleanShoot", "CleanShotWin"...). Marca registrada per MacPaw.
- **Descompilar** el binari de CleanShot. Tot des de zero.

**Estratègia visual:** fer mockups propis (Figma/Penpot) que captin l'esperit minimalista de CleanShot però amb identitat pròpia. Referents addicionals lliures: **Shottr**, **Flameshot** (open source, MIT), **ShareX** (open source). Mirar Flameshot dóna idees defensables perquè ja és OSS.

### Riscos tècnics
- **DPI scaling** amb multi-monitor és el típic infern de WPF — assignar temps específic per testar.
- **SmartScreen / antivirus** marcaran un .exe sense signar com a sospitós. Pressupost per certificat (Sectigo, DigiCert) o usar EV certificate.
- **Captura de finestres protegides** (DRM, Netflix, etc.) Windows ho bloqueja a nivell de SO — no es pot evitar legalment.
- **Hotkeys conflict** amb altres apps — cal UI per detectar conflictes.

### Decisions preses
1. **Monetització:** gratuït + donacions (model tipus OBS Studio, FreeCAD, Shotcut). Botó "Donate" a Settings i a la web. Plataforma: **Stripe** (el David ja en té compte des de Freedolia).
2. **Cloud:** ajornat. Si s'arriba a fer, només via BYOC (Bring Your Own Cloud) — l'usuari connecta el seu S3/Imgur/Dropbox. Zero infraestructura pròpia que mantenir.
3. **Nom:** **FreeScreenshot** (definitiu). Recomanat fer cerca ràpida a EUIPO/USPTO/TMview per confirmar que ningú ja l'ha registrat com a marca; també comprovar disponibilitat del domini (freescreenshot.com, .app, .io...).
4. **Llicència:** **Open source**. Recomanació: GPLv3 o MPL 2.0 (copyleft moderat, evita que algú agafi el codi i en faci una versió tancada de pagament sense contribuir). MIT seria més permissiu però facilita que algú s'aprofiti del teu treball comercialment sense aportar res.

---

## 6. Estimació total

| Escenari | Hores | Calendari (1 dev part-time, 15h/setmana) |
|---|---|---|
| Només MVP (F1) | 80–120h | 6–8 setmanes |
| MVP + Polit (F1+F2) | 140–200h | 10–14 setmanes |
| Producte complet sense cloud (F1+F2+F3) | 220–320h | 16–22 setmanes |
| Tot inclòs (F1–F5) | 380–520h | 26–36 setmanes |

---

## 7. Següent pas

Tres opcions per arrencar **avui**:

1. **Setup del repo i Hello World WPF** — projecte buit compilant a .exe.
2. **Mockups visuals primer** — dissenyar la teva UI pròpia (Figma/Excalidraw) abans de codi.
3. **Prova de concepte de captura** — només el bloc mínim de captura d'àrea, sense editor, per validar que `Windows.Graphics.Capture` funciona com cal.

Digues per quina anem i hi entrem.
