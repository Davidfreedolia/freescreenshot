# Contribuir a FreeScreenshot

Gràcies per voler ajudar. Aquest document explica com participar de forma útil i sense fricció.

## Com pots col·laborar

- **Codi:** bugs, features, refactor, tests. Mira els issues amb l'etiqueta `good first issue`.
- **Disseny:** mockups, icones, millores d'UX. Obre un issue i adjunta.
- **Traduccions:** vols afegir el teu idioma? Mira `docs/UX-COPY.md` i obre un PR amb el nou fitxer.
- **Reports de bugs:** obre un issue amb passos per reproduir, versió de Windows, log si en tens.
- **Idees:** Discussions activades al repo. Sense compromís.

## Flux per contribuir codi

1. **Fork** del repo.
2. **Branca** nova: `git checkout -b feat/nom-curt` o `fix/nom-curt`.
3. **Codi.** Segueix la guia d'estil del `.editorconfig`. Run `dotnet format` abans del commit.
4. **Tests.** Si toques `Core/`, afegeix-hi test a `FreeScreenshot.Tests`.
5. **Commit.** Format curt i en imperatiu: `feat: afegir captura amb scroll`, `fix: solucionar DPI a multi-monitor`. Prefixos: `feat`, `fix`, `refactor`, `docs`, `test`, `chore`.
6. **Pull request.** Descripció clara del què i el perquè. Captures si afecta la UI.

## Setup local

```powershell
git clone https://github.com/<el-teu-usuari>/freescreenshot.git
cd freescreenshot
dotnet restore
dotnet build
dotnet test
dotnet run --project src/FreeScreenshot.Tray
```

## Estil de codi

- C# 12 / .NET 8. Namespaces `file-scoped`.
- Noms de classes en `PascalCase`. Camps privats en `_camelCase`.
- Pocs comments. Si has d'explicar el "què", el codi està mal escrit. Si expliques el "per què", endavant.
- Mai `Console.WriteLine` en codi de producció — usa el logger.

## Estil de commits

- Una idea per commit. No mesclis refactor amb feature.
- Subject line: imperatiu, sense punt final, max 72 caràcters.
- Body opcional explicant el "per què", separat del subject per una línia buida.

## Codi de conducta

Mira [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md). Resum: sigues respectuós. Punt.

## Llicència del que aportis

Tot el que contribueixis cau sota la mateixa llicència del projecte (GPLv3). En obrir un PR ho acceptes implícitament.

## Preguntes

Obre un issue o ves a Discussions del repo.
