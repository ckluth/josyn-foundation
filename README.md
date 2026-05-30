# josyn-foundation

**josyn-foundation** enthält die drei Kern-Bausteine von **JOSYN** (*JobSystem Next*) —
als eigenständige NuGet-Pakete, die unabhängig voneinander versioniert und eingesetzt
werden können.

---

## Bausteine

| Paket | Rolle | Abhängigkeiten |
|---|---|---|
| [`JOSYN.Foundation.ResultPattern`](josyn-foundation-result-pattern/README.md) | Errors-as-Values — das durchgängige Fehlerbehandlungsprinzip von JOSYN | — |
| [`JOSYN.Foundation.PropertyBag`](josyn-foundation-property-bag/README.md) | Serialisierung flacher Records für JOSYN-IPC-Kanäle (INI / JSON) | ResultPattern |
| [`JOSYN.Foundation.JIP`](josyn-foundation-jip/README.md) | Named-Pipe-IPC-Transport (JOSYN Interprocess Protocol) | ResultPattern |

### Abhängigkeitskette

```
JOSYN.Foundation.ResultPattern
        ↑                ↑
JOSYN.Foundation.   JOSYN.Foundation.
   PropertyBag           JIP
```

`ResultPattern` ist die einzige gemeinsame Abhängigkeit. PropertyBag und JIP kennen
sich gegenseitig nicht.

---

## Lokales Arbeiten

Jedes Sub-Repo ist autark. Aus dem jeweiligen Verzeichnis:

```
.local-build\build.cmd      # Release-Build
.local-build\test.cmd       # Tests ausführen
.local-build\pack.cmd       # NuGet-Paket → ..\..\local-packages\
```

**Reihenfolge beim ersten Setup** (wegen Abhängigkeiten):

```
1. josyn-foundation-result-pattern\  → pack
2. josyn-foundation-property-bag\    → pack
3. josyn-foundation-jip\             → pack
```

---

## Status

Reifer PoC — Milestone 1. Die Pakete sind intern produktionsreif;
die `preview`-Kennzeichnung spiegelt den noch offenen Abnahme-Prozess wider.
Bekannte PoC-Einschränkungen sind in den jeweiligen `POC-HACKS.md`-Dateien dokumentiert.

---

*JOSYN Foundation — © 2026 HAEVG AG — MIT License*
