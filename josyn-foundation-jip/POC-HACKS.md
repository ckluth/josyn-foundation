# PoC-Hacks — JOSYN.Foundation.JIP

Diese Datei dokumentiert bewusst akzeptierte technische Schulden im PoC-Kontext.
**Einfach löschen**, sobald das Paket in eine produktive Infrastruktur überführt wird.

---

## 1. Lokaler NuGet-Feed (`nuget.config`)

**Datei:** `nuget.config`  
**Hack:** Der Package-Source zeigt auf `..\..\local-packages` — ein lokales Verzeichnis
relativ zur Mono-Repo-Struktur. Dort liegt `JOSYN.Foundation.ResultPattern`, von dem
dieses Paket abhängt.  
**Produktiv-Ersatz:** Eintrag auf einen echten NuGet-Feed (z. B. Azure Artifacts,
GitHub Packages oder NuGet.org) umstellen.

## 2. Pack-Ausgabepfad (`pack.cmd`)

**Datei:** `.local-build\pack.cmd`  
**Hack:** `dotnet pack` schreibt das `.nupkg` direkt in `..\..\local-packages` —
dasselbe lokale Verzeichnis wie oben.  
**Produktiv-Ersatz:** Entfällt; das Paketieren übernimmt die Build-Pipeline (z. B.
`dotnet pack` + `dotnet nuget push` im CI).

## 3. Hardcodierter Build-Ausgabepfad (`Directory.Build.props`)

**Datei:** `Directory.Build.props`  
**Hack:** Ausgabeverzeichnis ist auf `C:\Temp\VS.OUT\JOSYN\` festgelegt —
maschinenspezifisch, funktioniert nur auf dem Entwickler-Rechner.  
**Produktiv-Ersatz:** Entfernen oder durch einen relativen/CI-kompatiblen Pfad ersetzen
(Standard-MSBuild-Ausgabe ist ausreichend).

## 4. Single-in-Flight (architektonisch, kein Build-Hack)

**Datei:** `JOSYN.Foundation.JIP\Jip\` (gesamter Konventions-Layer)  
**Hack:** Anfragen werden strikt sequenziell verarbeitet — kein Multiplexing,
kein Request-ID-Konzept.  
**Produktiv-Ersatz:** Concurrent request handling mit Request-ID-Korrelation.
