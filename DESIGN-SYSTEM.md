# FreeScreenshot — Design System (Freedolia)

**Versió:** v0.1 — 27 maig 2026
**Identitat:** Freedolia (empresa paraigua, freedolia.com)
**Producte:** FreeScreenshot
**Principis:** minimalisme, calidesa, velocitat, claredat

---

## 1. Principis de disseny

| # | Principi | Què vol dir a la pràctica |
|---|---|---|
| 1 | **Invisible quan no cal** | L'app viu a la safata. No molesta. No notificacions per cosa òbvia. |
| 2 | **Un sol clic** | Cap acció freqüent darrere més d'un clic o tecla. |
| 3 | **Calidesa** | Grisos amb tints càlids. Mai blau-fred clínic. Sensació humana. |
| 4 | **Espai per respirar** | Padding generós, pocs elements per pantalla, sense densitat agressiva. |
| 5 | **Tipografia com a UI** | Pocs accents de color. Que la jerarquia la faci el text. |

---

## 2. Paleta — Mode fosc (principal)

| Token | Hex | Ús |
|---|---|---|
| `bg/base` | `#1A1814` | Fons general de finestres |
| `bg/surface-1` | `#25221E` | Targetes, panells |
| `bg/surface-2` | `#2F2C27` | Elements interactius elevats |
| `bg/surface-3` | `#3A3631` | Hover, divisors |
| `border/subtle` | `#3A3631` | Vores discretes |
| `border/strong` | `#524C44` | Vores marcades |
| `text/primary` | `#F5F2EC` | Text principal |
| `text/secondary` | `#A8A39B` | Text de suport |
| `text/muted` | `#6B6760` | Placeholder, hints |
| `text/disabled` | `#4A4640` | Estats inactius |
| `accent/base` | `#A3E635` | Verd llima — accent únic |
| `accent/hover` | `#BEF264` | Hover sobre accent |
| `accent/pressed` | `#84CC16` | Estat clicat |
| `accent/soft-bg` | `#A3E63520` | Fons subtil amb 12% opacitat |
| `feedback/success` | `#A3E635` | Mateix accent |
| `feedback/warning` | `#FBBF24` | Avís |
| `feedback/error` | `#F87171` | Error |
| `feedback/info` | `#94A3B8` | Info neutra |

## 3. Paleta — Mode clar (secundari)

| Token | Hex | Ús |
|---|---|---|
| `bg/base` | `#FAF8F4` | Fons general |
| `bg/surface-1` | `#FFFFFF` | Targetes |
| `bg/surface-2` | `#F0EDE7` | Panells secundaris |
| `bg/surface-3` | `#E5E0D8` | Hover |
| `border/subtle` | `#E5E0D8` | Vores discretes |
| `border/strong` | `#CFC9BE` | Vores marcades |
| `text/primary` | `#1A1814` | Text principal |
| `text/secondary` | `#5C5852` | Text de suport |
| `text/muted` | `#8B867F` | Placeholder |
| `text/disabled` | `#B8B3AB` | Inactius |
| `accent/base` | `#65A30D` | Lime més fosc per contrast |
| `accent/hover` | `#4D7C0F` | |
| `accent/pressed` | `#84CC16` | |

---

## 4. Tipografia

**Família UI:** Inter (variable, open source — SIL Open Font License). Fallback: `Segoe UI Variable, Segoe UI, system-ui, sans-serif`.

**Família mono:** JetBrains Mono (per valors numèrics, dimensions, dreceres). Fallback: `Consolas, Cascadia Mono, monospace`.

### Escala tipogràfica

| Nom | Mida | Line-height | Pes | Ús |
|---|---|---|---|---|
| `display` | 32px | 40px | 600 | Onboarding, splash |
| `heading-1` | 22px | 28px | 600 | Títol de finestra |
| `heading-2` | 17px | 24px | 600 | Seccions |
| `body-large` | 15px | 22px | 400 | Cos principal |
| `body` | 13px | 20px | 400 | Cos estàndard UI |
| `body-small` | 12px | 18px | 400 | Text de suport |
| `caption` | 11px | 16px | 500 | Etiquetes, ratllats |
| `mono-dim` | 12px | 16px | 500 | Dimensions "1920 × 1080" |

**Tracking:** -0.005em a heading. 0 a body. +0.02em a caption uppercase.

---

## 5. Espais (8pt grid amb mig pas)

```
xs: 4px   sm: 8px   md: 12px   base: 16px
lg: 20px  xl: 24px  2xl: 32px  3xl: 40px  4xl: 56px
```

**Regla:** mai usar valors fora d'aquesta escala. Si necessites alguna cosa diferent, és que el component està malament.

---

## 6. Radis

| Token | Valor | Ús |
|---|---|---|
| `radius/sm` | 6px | Botons, inputs, chips |
| `radius/md` | 10px | Targetes, panells |
| `radius/lg` | 14px | Finestres flotants, diàlegs |
| `radius/xl` | 20px | Finestra principal (Win11 ja ho aplica) |
| `radius/full` | 9999px | Píndoles, indicadors circulars |

---

## 7. Ombres (subtils, tint càlid)

```
shadow/sm:   0 1px 2px rgba(26, 24, 20, 0.08)
shadow/md:   0 4px 12px rgba(26, 24, 20, 0.12), 0 1px 2px rgba(26, 24, 20, 0.06)
shadow/lg:   0 12px 32px rgba(26, 24, 20, 0.20), 0 2px 6px rgba(26, 24, 20, 0.10)
shadow/xl:   0 24px 60px rgba(26, 24, 20, 0.30), 0 4px 12px rgba(26, 24, 20, 0.15)
shadow/accent: 0 0 0 3px rgba(163, 230, 53, 0.25)   /* focus ring */
```

---

## 8. Components base

### Botó

**Variants:** `primary` (accent lime), `secondary` (surface-2 + border), `ghost` (només text + hover subtle), `danger` (vermell).

**Mides:** `sm` (h28, px12), `md` (h36, px16), `lg` (h44, px20).

**Estats:** default, hover, pressed, focus (anell accent 3px), disabled (40% opacity).

```
Primary:    bg=accent/base   text=#1A1814 (negre sobre lime, no blanc)
Secondary:  bg=surface-2     text=text/primary    border=border/subtle
Ghost:      bg=transparent   text=text/primary    hover=surface-2
Danger:     bg=error         text=#FFFFFF
```

### Input

```
height: 36px
padding: 0 12px
bg: surface-2
border: 1px solid border/subtle
radius: radius/sm
focus: border accent/base + shadow/accent
```

### Toolbar flotant (post-captura)

Píndola arrodonida (`radius/full` als extrems, `radius/lg` al centre). Botons d'icona 36px. Separadors verticals 1px de `border/subtle`. Ombra `shadow/lg`.

### Targeta de captura (historial)

```
amplada: 240px
ratio miniatura: 16:10
padding inferior: 12px (info)
radius: radius/md
hover: lift +2px + shadow/md
```

### Selecció d'àrea (overlay)

- Fons exterior: `rgba(26, 24, 20, 0.55)` (dim càlid)
- Vora selecció: 1px sòlid `accent/base`
- Cantonades (4): quadrats 8×8 lime sòlid
- Crosshair: línies 1px `text/secondary` amb 60% opacitat
- Indicador de mida: píndola `surface-1` + text mono `mono-dim` a 8px sota el cursor

---

## 9. Icones

**Set base:** [Lucide Icons](https://lucide.dev) (ISC license, lliure). Stroke 1.75px, tamany base 18px.

**Mai usar icones tretes de CleanShot, Shottr, o qualsevol altre producte propietari.**

**Icones clau a l'app:**

| Icona Lucide | Ús |
|---|---|
| `crop` | Capturar àrea |
| `monitor` | Pantalla sencera |
| `app-window` | Finestra |
| `scroll-text` | Scroll capture |
| `video` | Gravació |
| `arrow-up-right` | Fletxa anotació |
| `square` | Rectangle |
| `circle` | Cercle |
| `type` | Text |
| `pen-tool` | Dibuix lliure |
| `eraser` | Esborrar |
| `pin` | Pin a sobreposició |
| `cloud-upload` | Pujar |
| `copy` | Copiar |
| `download` | Desar |
| `settings` | Configuració |
| `undo-2` / `redo-2` | Desfer / Refer |

---

## 10. Animacions

**Filosofia:** ràpides, no decoratives. Cap easing rebot. Sempre `cubic-bezier(0.32, 0.72, 0, 1)` (suavitzat tipus iOS) o `ease-out` per entrades.

| Acció | Durada | Easing |
|---|---|---|
| Hover de botó | 120ms | ease-out |
| Aparició toolbar flotant | 180ms | cubic-bezier(0.32, 0.72, 0, 1) |
| Obertura editor | 220ms | cubic-bezier(0.32, 0.72, 0, 1) |
| Tancament | 160ms | ease-in |
| Drag de selecció | 0ms (immediat) | — |

---

## 11. Estats globals

| Estat | Visual |
|---|---|
| **Loading** | Spinner 16px stroke lime + text "Processant..." |
| **Empty state** | Icona 32px `text/muted` + missatge + CTA |
| **Error** | Banner inferior `feedback/error` 4px barra + missatge |
| **Èxit (toast)** | Píndola surface-1 + check lime + auto-dismiss 2.5s |

---

## 12. Accessibilitat

- Contrast text/fons mínim **4.5:1** (WCAG AA). Verificat amb la paleta seleccionada.
- Focus visible sempre: anell lime 3px (`shadow/accent`).
- Cap funcionalitat només a hover — tot accessible per teclat.
- Mida mínima de tap target: 32×32px.
- `prefers-reduced-motion` respectat — animacions reduïdes a 0ms.

---

## 13. No fer

- Gradients (tret de l'efecte glass molt subtil al toolbar flotant).
- Drop shadows colorides — sempre tint càlid neutre.
- Més d'un color d'accent al mateix temps. **Una sola família** (lime).
- Icones omplertes (filled). Sempre outline.
- Tipografies decoratives. Només Inter + JetBrains Mono.
- Borders més gruixuts d'1px (excepte focus rings).
