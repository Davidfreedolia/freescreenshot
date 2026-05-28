# Cowork brief — publicar Freezshot a `freedolia.com`

**Repo destí:** `freedolia-hub`
**Repo origen:** aquest mateix (`freescreenshot` / Freezshot)
**Versió actual:** v2.1.0, ja pujada a R2 com a `Setup_Freezshot.exe`.

Aquest fitxer és la **llista d'accions concretes**. Per al detall de paleta,
tipografia i QA, llegeix `HANDOVER.md` en aquesta mateixa carpeta.

> **Regla d'or.** Tot el que veu l'usuari porta el nom **Freezshot**.
> Tot el que és infra (`/api/freescreenshot/*`, bucket R2 `freescreenshot`,
> taula `freescreenshot_latest`, repo GitHub `…/freescreenshot`) **es queda
> com està**. No els canviïs.

---

## Pas 1 — Copia els assets de marca

Des d'aquest repo a `freedolia-hub`:

```
freezshot/web/icon.svg       →  public/static/freezshot/icon.svg
freezshot/web/icon.ico       →  public/static/freezshot/icon.ico
freezshot/web/wordmark.svg   →  public/static/freezshot/wordmark.svg
```

Si vols generar una `og.png` (1200×630) per Open Graph, centra `icon.svg`
sobre fons `#0A1A1C` amb el wordmark a sota.

## Pas 2 — Crea les 3 rutes de landing

| Origen                       | Ruta pública                       |
| ---------------------------- | ---------------------------------- |
| `freezshot/web/landing-ca.html` | `freedolia.com/ca/freezshot`      |
| `freezshot/web/landing-es.html` | `freedolia.com/es/freezshot`      |
| `freezshot/web/landing-en.html` | `freedolia.com/en/freezshot`      |

Mateix patró de layout que `/{ca,es,en}/freewisp` (header + footer
globals; body del fitxer és el contingut). El CSS està **només** a
`landing-es.html` — extreu-lo a un `freezshot.css` compartit o duplica'l
als altres dos.

## Pas 3 — Insereix el banner al home

Agafa `freezshot/web/banner-home.html` (HTML + estils scoped `.fs-banner__*`)
i col·loca'l **al costat del banner de FreeWisp** del home. L'enllaç apunta
a `/es/freezshot` per defecte; ajusta-ho al locale del visitant igual que
fas amb FreeWisp.

## Pas 4 — Afegeix Freezshot al header global

Afegeix l'entrada **"Freezshot"** al menú de productes (al costat de
"FreeWisp"), amb el mateix estil de pill teal.

## Pas 5 — Implementa lead capture al click "Descarrega'l gratis"

Mateix flux que ja tens per a FreeWisp:

1. Click al CTA `Descarrega'l gratis` / `Descárgalo gratis` / `Download free`.
2. Modal demanant l'email.
3. INSERT a `public.leads` amb:
   ```
   product = 'freezshot'
   email   = <email>
   lang    = <ca|es|en>
   ```
4. Redirigeix a `/api/freescreenshot/download` (que ja apunta al
   `Setup_Freezshot.exe` actual a R2 — no cal tocar res del rewrite).
5. A la pàgina de gràcies, **missatge de donació destacat** amb botó
   apuntant a:
   ```
   https://donate.stripe.com/6oUcN559jeWpcDZ8eMfYY04
   ```
   És el **mateix Payment Link** que fa servir l'app — si l'has de
   canviar, fes-ho als dos llocs en el mateix sprint.

## Pas 6 — Verifica les rutes API (no toques res)

Aquestes ja existeixen i s'han de mantenir intactes:

- `/api/freescreenshot/download`  → redirect a R2
- `/api/freescreenshot/install`   → POST install telemetry
- `/api/freescreenshot/latest`    → GET última versió (`freescreenshot_latest`)
- `/api/freescreenshot/uninstall` → POST motiu de desinstal·lació

Confirma amb `curl -IL https://freedolia.com/api/freescreenshot/download`
que retorna `Setup_Freezshot.exe` 200 OK.

## Pas 7 — QA i deploy

- [ ] Mòbil (≤ 720px) col·lapsa correctament les 3 landings.
- [ ] Hover del banner mostra el teal accent al border + lift -2px.
- [ ] Switch CA/ES/EN del header porta a la landing del locale correcte.
- [ ] Modal lead capture s'obre, valida email, insereix a `leads`,
      redirigeix a `download`.
- [ ] Pàgina de gràcies mostra el missatge de donació amb el Payment
      Link de Stripe.

---

## Notes que NO cal implementar

- Cap canvi al bucket R2.
- Cap canvi a `freescreenshot_latest`.
- Cap renamed d'endpoints `/api/*`.
- Cap renamed del repo GitHub `Davidfreedolia/freescreenshot`.

Tot això es queda intencionalment amb el nom antic. L'app instal·lada
hi apunta i no toleraria un canvi sense recompilar.

---

## Si trobes alguna cosa rara

Pregunta abans de tocar. Els fitxers origen viuen a:
`freescreenshot/brand/` (SVG/ICO masters) i
`freescreenshot/web/` (HTML/CSS de la landing i el banner).
