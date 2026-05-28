# JOSYN-IPC-Protocol

> **Status:** Draft · **Version:** 0.1 · **Zielgruppe:** Interne Entwickler

---

## Übersicht

Das JOSYN-IPC-Protokoll definiert einen strukturierten, schichtbasierten Kommunikationskanal zwischen genau zwei Prozessen (Client und Server) auf Basis von `System.IO.Pipes`. Es ist für schnelle, robuste Übertragung kleiner bis mittelgroßer Datenmengen konzipiert – etwa Commands, Status-Updates oder kleine Dateipayloads.

Durch die Nutzung von `System.IO.Pipes` ist das Protokoll plattformübergreifend portabel: Auf Windows werden Named Pipes verwendet, auf Linux werden diese transparent auf Unix-Domain-Sockets abgebildet.

---

## Schichtenmodell

Das Protokoll ist in aufeinander aufbauenden Schichten organisiert:

| Schicht | Bezeichnung                  | Status        | Beschreibung                                                               |
|---------|------------------------------|---------------|----------------------------------------------------------------------------|
| 0       | Transport                    | Implementiert | `System.IO.Pipes` – plattformübergreifende Pipe-Abstraktion                |
| 1       | Framing                      | Implementiert | Rohe Nutzdaten als `byte[]`-Arrays                                         |
|         |                              |               | [ 4 Bytes: Länge (int32, little-endian) ][ N Bytes: Payload (UTF-8) ]      |
| 2       | Textkodierung                | Implementiert | UTF-8-Enkodierung der Nutzdaten                                            |
| 3       | Konventionen-Protokoll       | Geplant       | Strukturierte Felder: Verb, Payload, Status – voraussichtlich JSON-basiert |

Applikationsspezifische Layer, die konkrete Request-Typen definieren, liegen außerhalb des Scopes dieses Protokolls.

---

## Architektur & Kommunikationsmodell

Das Protokoll folgt einem strikten **Request/Response-Muster** zwischen genau einem Client und einem Server (1:1-Topologie).

```
  Client                             Server
    │                                  │
    │        Session etabliert         │
    │─────────────────────────────────►│
    │                                  │
    │──── Request (req-pipe) ─────────►│
    │◄─── Response (res-pipe) ─────────│
    │                                  │
    │──── Request (req-pipe) ─────────►│
    │◄─── Response (res-pipe) ─────────│
    │                                  │
```

### Einschränkungen

| Eigenschaft                | Ausprägung                               |
|----------------------------|------------------------------------------|
| Topologie                  | 1:1 (one-to-one)                         |
| Kommunikationsrichtung     | Unidirektional Requests: Client → Server |
| Server-initiierte Anfragen | Nicht unterstützt                        |
| Multiplexing               | Nicht unterstützt                        |
| Große Payloads             | Nicht vorgesehen                         |

---

## Session-Management

### Session-Key

Jede Kommunikationssitzung wird durch einen eindeutigen **Session-Key** identifiziert. Der Session-Key ist eine GUID (Globally Unique Identifier) und dient als Basis für die Ableitung beider Pipe-Namen.

### Pipe-Namenskonvention

Aus dem Session-Key werden zwei Pipe-Endpunkte nach folgendem Schema abgeleitet:

| Pipe          | Namensschema             | Richtung        |
|---------------|--------------------------|-----------------|
| Request-Pipe  | `req-pipe-{sessionKey}`  | Client → Server |
| Response-Pipe | `res-pipe-{sessionKey}`  | Server → Client |

---

## Bootstrap & Handshake

### CLI-Argument-Konvention

Die Übergabe des Session-Keys zwischen den Prozessen erfolgt über standardisierte CLI-Argumente. Alle IPC-bezogenen Argumente beginnen mit dem Magic-Token `JOSYN-IPC`, gefolgt vom Session-Key und optional dem Pfad zur Client-Executable.

**Argumentstruktur:**

```
JOSYN-IPC <session-key> [<client-exe-path>]
```

| Argument        | Pflicht | Beschreibung                                    |
|-----------------|---------|-------------------------------------------------|
| Magic-Token     | Ja      | Fester Bezeichner `JOSYN-IPC`                   |
| Session-Key     | Ja      | GUID der aktuellen Session                      |
| Client-Exe-Pfad | Nein    | Absoluter Pfad zur Client-Executable (optional) |

---

### Szenario 1 – Autonomer Start (Produktivbetrieb)

Der Server ermittelt den Pfad der Client-Executable eigenständig und steuert den gesamten Startprozess.

```
  Dritte Partei          Server                    Client
       │                    │                          │
       │── startet ────────►│                          │
       │                    │── erzeugt Session-Key    │
       │                    │── ermittelt Client-Exe   │
       │                    │── startet Client ───────►│
       │                    │   (übergibt Session-Key) │
       │                    │◄──── verbindet ──────────│
       │                    │                          │
```

**Ablauf:**
1. Server wird ohne IPC-Argumente gestartet.
2. Server erzeugt einen neuen Session-Key (GUID).
3. Server ermittelt propriatär den Pfad der Client-Executable.
4. Server startet die Client-Executable und übergibt den Session-Key als CLI-Argument.
5. Client verbindet sich über die abgeleiteten Pipe-Namen.

---

### Szenario 2 – Extern verwalteter Session-Key (Debug/Demo)

Ein Dritter steuert sowohl Server als auch Client und übergibt beiden denselben Session-Key.

```
  Dritte Partei          Server                    Client
       │                    │                         │
       │── startet ────────►│  (mit Session-Key)      │
       │                    │── öffnet Pipes          │
       │                    │── wartet auf Connection │
       │── startet ─────────────────────────────────► │  (mit Session-Key)
       │                    │◄──── verbindet ─────────│
       │                    │                         │
```

**Ablauf:**
1. Server erhält Session-Key als CLI-Argument.
2. Server öffnet die Pipes und wechselt in den Listen-Modus.
3. Client wird separat von einer dritten Partei mit demselben Session-Key gestartet.
4. Client verbindet sich über die abgeleiteten Pipe-Namen.

> **Hinweis:** Dieses Szenario ist ausschließlich für Debug- und Demozwecke vorgesehen.

---

### Szenario 3 – Extern verwalteter Session-Key mit Client-Pfad (Debug/Demo)

Erweiterung von Szenario 2: Die dritte Partei übergibt zusätzlich den Pfad zur Client-Executable, sodass der Server den Client selbst starten kann.

```
  Dritte Partei          Server                      Client
       │                    │                           │
       │── startet ────────►│ (Session-Key + Exe-Pfad)  │
       │                    │── öffnet Pipes            │
       │                    │── startet Client ────────►│
       │                    │◄──── verbindet ───────────│
       │                    │                           │
```

**Ablauf:**
1. Server erhält Session-Key und Exe-Pfad als CLI-Argumente.
2. Server öffnet die Pipes.
3. Server startet die Client-Executable mit dem vorhandenen Session-Key.
4. Client verbindet sich über die abgeleiteten Pipe-Namen.

> **Hinweis:** Dieses Szenario ist ausschließlich für Debug- und Demozwecke vorgesehen.

---

### Szenario-Vergleich

| Kriterium                    | Szenario 1          | Szenario 2          | Szenario 3          |
|------------------------------|---------------------|---------------------|---------------------|
| Session-Key-Erzeugung        | Server              | Dritte Partei       | Dritte Partei       |
| Client-Start                 | Server              | Dritte Partei       | Server              |
| Client-Exe-Pfad erforderlich | Nein (proprietär)   | Nein                | Ja (per Argument)   |
| Verwendungszweck             | Produktivbetrieb    | Debug / Demo        | Debug / Demo        |

---

## Verhalten & Robustheit

### Fehlerbehandlung

| Aspekt               | Status         | Beschreibung                                                                 |
|----------------------|----------------|------------------------------------------------------------------------------|
| Lokales Logging      | Vorgesehen     | Strukturiertes, definiertes Logging von Verbindungsabbrüchen und Fehlern     |
| Benachrichtigung     | Optional       | E-Mail-Benachrichtigung als mögliche Erweiterung evaluiert                   |
| Connection-Timeout   | Implementiert  | Feste, praxiserprobte Timeoutwerte; Konfigurierbarkeit als Option vorgesehen |

### Session-Teardown

Ein expliziter Schließ-Handshake ist nicht vorgesehen. Der Server beendet die Session stattdessen reaktiv und robust in folgenden Fällen:

| Beendigungsursache                            | Verhalten                |
|-----------------------------------------------|--------------------------|
| Client schließt Pipes explizit                | Server terminiert sauber |
| Client terminiert unsauber (Crash etc.)       | Server terminiert sauber |
| Expliziter Server-Shutdown bei aktivem Client | Nicht vorgesehen         |

**Cancellation-Mechanismus:** Die Server-Implementierung verwendet ein `CancellationToken`, das sowohl den Connection-Aufbau als auch die Request-Loop absichert. Abbruchbedingungen sind über ein frei definierbares Predicate darstellbar, sodass beliebige Shutdown-Trigger integriert werden können.

| Umgebung    | Cancellation-Implementierung                                      |
|-------------|-------------------------------------------------------------------|
| Demo        | Konsoleneingabe – Abbruch via `ESC`-Taste (`Console.ReadKey`)     |
| Produktion  | Applikationsspezifisch über Predicate zu implementieren           |

### Reconnect

Reconnect nach Verbindungsverlust ist aktuell nicht vorgesehen. Jede neue Verbindung erfordert den Aufbau einer neuen Session.

---

## Ausblick

| Thema                        | Beschreibung                                                                 |
|------------------------------|------------------------------------------------------------------------------|
| Layer 3 – Konventionsschicht | Definition von Verb, Payload und Status; Evaluierung JSON als Trägerformat   |
| Konfigurierbare Timeouts     | Externalisierung der Timeout-Werte in eine Konfigurationsschicht             |
| Reconnect-Unterstützung      | Derzeit nicht geplant; bei Bedarf als explizites Feature zu spezifizieren    |

---

*Dieses Dokument beschreibt den aktuellen Implementierungsstand sowie geplante Erweiterungen des JOSYN-IPC-Protokolls. Es ist als lebendes Dokument zu verstehen und wird mit fortschreitender Entwicklung aktualisiert.*
