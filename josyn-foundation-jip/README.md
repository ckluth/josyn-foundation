# JOSYN.Foundation.JIP

**JIP** (JOSYN Interprocess Protocol) ist der Transportschicht-Baustein von JOSYN.
Er koppelt zwei Prozesse über Named Pipes vollständig voneinander entkoppelt: Der
**JobHost** (Frontend) und der **JAPServer** (Backend) kommunizieren ausschließlich
über dieses Protokoll — ohne gemeinsamen Speicher, ohne geteilte Abhängigkeiten.

---

## Warum Named Pipes?

Named Pipes sind ideal für session-isolierte, sequenzielle IPC innerhalb einer Maschine:

- **Prozessisolation.** Frontend und Backend laufen in getrennten Prozessen —
  ein Absturz auf einer Seite betrifft die andere nicht.
- **Kein gemeinsamer Zustand.** Die Kommunikation ist vollständig serialisiert;
  kein Shared-Memory, kein Locking, keine Race-Conditions.
- **Einfaches Protokoll.** Length-Prefix (`int32` + Bytes, Little-Endian) ist
  ausreichend für JOSYN — kein HTTP-Overhead, keine Bibliotheksabhängigkeit.

---

## Schnellstart

### Server starten

```csharp
var args = new ServerStartArguments
{
    HandleStringRequest = async request =>
    {
        // Anfrage verarbeiten, Antwort zurückgeben
        return await Task.FromResult($"Echo: {request}");
    },
    HandleErrorNotification = async (req, ex) =>
    {
        Console.Error.WriteLine($"Fehler bei '{req}': {ex.Message}");
        await Task.CompletedTask;
    }
};

var result = await PipesServer.RunAsync(args);
if (!result.Succeeded) Console.Error.WriteLine(result.ErrorMessage);
```

### Client verbinden und Anfrage senden

```csharp
var connect = await PipesClient.ConnectAsync(sessionKey);
if (!connect.Succeeded) { /* Fehler behandeln */ return; }

var pipes = connect.Value;

var response = await PipesClient.SendRequestAsync("PING", pipes);
if (!response.Succeeded) { /* Fehler behandeln */ return; }

Console.WriteLine(response.Value); // "Echo: PING"

await PipesClient.DisconnectAsync(pipes);
```

### JIP-Konventions-Layer (empfohlene API)

Für strukturierte Anfragen mit Dispatcher:

```csharp
// Server-Seite
var handler = JipServer.WrapHandler(async (Request req) =>
{
    return req.What switch
    {
        "PING" => Result<string?>.Success("PONG"),
        _      => Result<string?>.Fail($"Unbekannte Anfrage: {req.What}")
    };
});

var args = new ServerStartArguments
{
    HandleStringRequest = handler,
    HandleErrorNotification = async (req, ex) => { /* ... */ }
};

// Client-Seite
var result = await JipClient.SendAsync(pipes, "PING");
```

---

## Architektur

JIP besteht aus zwei Schichten:

### Transportschicht (`PipesServer`, `PipesClient`, `PipesProtocol`)

Die untere Schicht versteht ausschließlich Bytes:

- **`PipesServer`** — Verwaltet den Server-Lifecycle; richtet Request- und Response-Pipe
  ein, wartet auf Verbindungen, verarbeitet Anfragen sequenziell (Single-in-Flight).
- **`PipesClient`** — Verbindet sich mit dem laufenden Server; exponentieller Backoff
  beim Verbindungsaufbau; Busy-Guard verhindert parallele Anfragen.
- **`PipesProtocol`** — Protokollkonstanten (Magic Tokens), CLI-Hilfsmethoden und
  Pipe-Namensableitung aus dem Session-Key.

### Konventions-Layer (`JipClient`, `JipServer`, `JipProtocol`, `JipDispatcher`)

Die obere Schicht definiert das JSON-basierte Anfrage/Antwort-Format:

- **`JipClient`** — Kapselt `PipesClient.SendRequestAsync` + JSON-Serialisierung.
- **`JipServer`** — Kapselt Handler-Signatur in die `Func<string, Task<string>>` des Servers.
- **`JipProtocol`** — Parst/serialisiert `Request` und `Response` (JSON); konvertiert
  in/aus `Result<string?>`.
- **`JipDispatcher`** — Fluent-Registrierung von Handlern; `RegisterAll<TProtocol>`
  registriert alle Handler eines Protokoll-Typs.

### Zwei-Pipe-Design

```mermaid
sequenceDiagram
    participant C as Client (JobHost)
    participant S as Server (JAPServer)

    C->>S: req-pipe-&lt;sessionKey&gt; (Request)
    S-->>C: res-pipe-&lt;sessionKey&gt; (Response)
```

Jede Session verwendet genau zwei Pipes — eine für Anfragen, eine für Antworten.
Der Session-Key (Guid) wird beim Server-Start als CLI-Argument an den Client übergeben:
`"JOSYN-IPC <sessionKey>"`.

---

## Bekannte Einschränkungen (PoC)

Diese Punkte sind bewusst akzeptiert und dokumentiert — kein Handlungsbedarf im PoC:

| Einschränkung | Beschreibung |
|---|---|
| Single-in-Flight | Kein Multiplexing; Anfragen werden strikt sequenziell verarbeitet — kein Request-ID-Konzept |

---

## Referenz

### Transportschicht

| Typ | Art | Beschreibung |
|---|---|---|
| `PipesServer` | `class` | Server-Lifecycle, Reconnect-Loop |
| `PipesClient` | `class` | Verbindungsaufbau, Request/Response |
| `PipesProtocol` | `class` | Magic Tokens, CLI-Helpers, Pipe-Namen |
| `ServerStartArguments` | `record` | Startkonfiguration für `PipesServer` |
| `ClientPipes` | `record` | Pipe-Handles für den Client |
| `ServerPipes` | `record` | Pipe-Handles für den Server |

### Konventions-Layer

| Typ | Art | Beschreibung |
|---|---|---|
| `JipClient` | `static class` | Sendet `Request`, empfängt `Result<string?>` |
| `JipServer` | `static class` | Kapselt Handler in Wire-Signatur |
| `JipProtocol` | `static class` | JSON-Parsing, Response-Konvertierung |
| `JipDispatcher` | `sealed class` | Fluent Handler-Registrierung |
| `Request` | `record` | Wire-Typ: `What` + optionales `Data` |
| `Response` | `record` | Wire-Typ: `Succeeded` + `Data?` + `Error?` |

---

## Für Maintainer

### Voraussetzungen

.NET 10 SDK, C# (latest)

### Bauen, Testen, Packen

Ausführen aus dem Repo-Wurzelverzeichnis (`JOSYN.Foundation.JIP\`):

```
.local-build\build.cmd          # Release-Build
.local-build\build.cmd Debug    # Debug-Build
.local-build\test.cmd           # Alle Tests ausführen
.local-build\pack.cmd           # NuGet-Paket erzeugen → ..\..\local-packages\
```

### Abhängigkeiten

- `JOSYN.Foundation.ResultPattern` (lokaler NuGet-Feed)

### Projektstruktur

```
JOSYN.Foundation.JIP\
├── PipesServer.cs              # Transport: Server-Lifecycle
├── PipesClient.cs              # Transport: Client-Verbindung
├── PipesProtocol.cs            # Transport: Protokoll-Konstanten
├── ServerStartArguments.cs     # Transport: Startkonfiguration
├── Contracts\                  # API-Verträge (Transportschicht)
│   ├── IPipesServer.cs
│   ├── IPipesClient.cs
│   ├── IPipesProtocol.cs
│   └── IServerStartArguments.cs
├── Jip\                        # Konventions-Layer
│   ├── JipClient.cs
│   ├── JipServer.cs
│   ├── JipProtocol.cs
│   ├── JipDispatcher.cs
│   ├── Wire\                   # Request/Response Wire-Typen
│   └── Contracts\              # API-Verträge (Konventions-Layer)
│       ├── IJipProtocol.cs
│       ├── IRequest.cs
│       └── IResponse.cs
└── Types\                      # ClientPipes, ServerPipes
```

---

*JOSYN.Foundation.JIP — © 2026 HAEVG AG — MIT License*
