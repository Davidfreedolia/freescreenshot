# Freezshot — handover per al cowork de `freedolia-hub`

Aquesta carpeta (`freezshot/web/`) conté tot el que necessites per
publicar **Freezshot** a `freedolia.com`: banner per al home + landing
en tres idiomes + assets de marca.

Tot està fet seguint el mateix patró que ja vam fer servir per a FreeWisp,
així que aquesta integració hauria d'encaixar al mateix lloc.

> **Naming.** El producte ara es diu **Freezshot** (abans FreeScreenshot).
> Els endpoints `/api/freescreenshot/*` i el bucket R2 **es queden com
> estan** — és infra que no canviem. Tot el que veu l'usuari (landing,
> banner, header) ha d'anar com a **Freezshot** / `/freezshot`.

---

## Què s'ha de fer

### 1. Copiar assets al `freedolia-hub`

| Origen (`freezshot/web/`) | Destí al `freedolia-hub`                  | Per a què serveix          |
| ------------------------- | ----------------------------------------- | -------------------------- |
| `icon.svg`                | `public/static/freezshot/icon.svg`        | Favicon + logo a la landing |
| `icon.ico`                | `public/static/freezshot/icon.ico`        | Favicon fallback            |
| `wordmark.svg`            | `public/static/freezshot/wordmark.svg`    | Logo + text (per al header) |

Si vols generar `og.png` (imatge per a Open Graph 1200×630), exporta una
versió de `icon.svg` centrada sobre fons `#0A1A1C` amb el wordmark.

### 2. Publicar les landings

Les **tres landings** són pàgines independents amb el seu propi CSS,
preparades per encaixar dins de la mateixa estructura de layout que
fa servir `/es/freewisp` (header global + footer global del lloc).

| Fitxer            | Ruta pública                          |
| ----------------- | ------------------------------------- |
| `landing-ca.html` | `freedolia.com/ca/freezshot`          |
| `landing-es.html` | `freedolia.com/es/freezshot`          |
| `landing-en.html` | `freedolia.com/en/freezshot`          |

**Important:** `landing-ca.html` i `landing-en.html` no inclouen el
`<style>` per estalviar duplicació — agafa el bloc de CSS de
`landing-es.html` i posa'l com a fitxer compartit, o duplica'l a cada
plantilla, com prefereixis al teu sistema de plantilles.

### 3. Afegir el banner al home

`banner-home.html` és un bloc autocontingut (HTML + estils scoped amb
prefix `.fs-banner__*`). Va just al costat — o al lloc — del banner
de FreeWisp del home.

Si la home té un grid de productes, encaixa com a segona targeta.
Si és un stack vertical, va a sota del de FreeWisp.

L'enllaç apunta a `/es/freezshot` per defecte. Ajusta-ho al locale
del visitant si la teva home té detecció d'idioma.

### 4. Afegir l'enllaç al navegador principal

Al header global (el component que pinta `CA | ES | EN` i el menú de
productes), afegir entrada **"Freezshot"** apuntant al locale
corresponent — al costat de "FreeWisp", amb el mateix estil de pill
verd/teal que ja té FreeWisp.

### 5. Routes API (ja existeixen, NO les canviïs)

Aquestes rutes ja estaven creades per FreeScreenshot i es queden així
perquè l'app instal·lada hi apunta. Els endpoints són estables:

- `freedolia.com/api/freescreenshot/download` → redirect a R2
- `freedolia.com/api/freescreenshot/install`  → POST telemetria d'install
- `freedolia.com/api/freescreenshot/latest`   → GET versió més recent
- `freedolia.com/api/freescreenshot/uninstall` → POST motiu de desinstal·lació

Els rewrites estan a `vercel.json` del repo `freedolia-hub`, secció
`rewrites`, igual que els de FreeWisp.

El binari a R2 ara s'anomena `Setup_Freezshot.exe` dins del mateix bucket
`freescreenshot`. La taula `public.freescreenshot_latest` continua sent
la font de veritat per al "latest".

---

## Paleta i tipografia (per coherència)

```css
--fs-bg-deep:      #061214;
--fs-bg-base:      #0A1A1C;
--fs-bg-surface1:  #112528;
--fs-bg-surface2:  #193033;
--fs-border:       #1F3A3D;
--fs-text:         #F0FAF7;
--fs-text-muted:   #A8BDB8;
--fs-accent:       #2DD4BF;   /* CTAs, links */
--fs-accent-hover: #5EEAD4;
--fs-accent-ink:   #06302C;   /* text sobre fons accent */
```

Tipografia: la mateixa stack que la resta del lloc — Segoe UI Variable,
Inter, system-ui, Arial.

Border radius: `999px` per a CTAs (pill), `18-24px` per a targes,
`12px` per a botons secundaris.

---

## QA abans de publicar

- [ ] Carrega cada landing a mòbil (≤ 720 px). El grid de features
      hauria de col·lapsar a una columna.
- [ ] `/api/freescreenshot/download` retorna **Setup_Freezshot.exe**
      versió 2.0 o més recent — proveu-ho amb `curl -IL`.
- [ ] El logo (favicon) es veu bé al tab del navegador.
- [ ] El banner del home té el hover state (lleugera elevació + accent
      al border).
- [ ] Switch d'idioma del header navega a la landing correcta de
      Freezshot.

---

Qualsevol cosa que no quadri, els fonts originals d'imatge i SVG són
a aquest mateix repo `freezshot/` a la carpeta `brand/`.
