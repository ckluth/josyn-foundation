# JOSYN.Foundation.PropertyBag

Serialisiert und deserialisiert flache C#-`record`-Typen zu und aus String-Formaten —
sectionloses INI oder JSON — mit vollständiger Integration des JOSYN-Result-Patterns.
Designed für den Einsatz in JOSYN-IPC-Protokollen, wo strukturierte Daten als inspektierbarer
String über Named-Pipes transportiert werden.

> **Scope.** Dies ist keine General-Purpose-Bibliothek. Ihre spezifische Rolle ist die
> Serialisierung flacher Records und Methodenparameter für JOSYN's Named-Pipe-IPC-Kanal.
> Für allgemeine JSON-Serialisierung: `System.Text.Json` direkt verwenden.

---

## Schnellstart

```csharp
// Record definieren — beide Schreibweisen funktionieren
public record JobRequest
{
    public required string JobId   { get; init; }
    public int             Retries { get; init; }
    public bool            Urgent  { get; init; }
}

var req = new JobRequest { JobId = "JOB-42", Retries = 3, Urgent = true };

// Zu INI serialisieren
var ini = PropertyBag.Serialize(req, IniDictionarySerializer.Serialize);
// JobId=JOB-42
// Retries=3
// Urgent=True

// Zu JSON serialisieren
var json = PropertyBag.Serialize(req, JsonDictionarySerializer.Serialize);
// {
//   "JobId": "JOB-42",
//   "Retries": "3",
//   "Urgent": "True"
// }

// Deserialisieren — Format wird automatisch erkannt
var result = PropertyBag.Deserialize<JobRequest>(ini.Value);
// result.Value.JobId   == "JOB-42"
// result.Value.Retries == 3
```

---

## API-Übersicht

Alle Methoden geben `Result` oder `Result<T>` zurück. Keine Exceptions propagieren nach oben.

### `PropertyBag` — Haupt-Einstiegspunkt

| Methode | Beschreibung |
|---------|-------------|
| `Serialize<TRecord>(record, serializer)` | Serialisiert eine Record-Instanz mit dem angegebenen Format-Serializer. |
| `Serialize(object, Type, serializer)` | Dasselbe, mit dem Typ zur Laufzeit (für Reflection-basierte Aufrufer). |
| `Deserialize<TRecord>(string)` | Erkennt Format automatisch, deserialisiert in einen stark typisierten Record. |
| `Deserialize(string, Type)` | Dasselbe, mit dem Zieltyp zur Laufzeit. Gibt `Result<object>` zurück. |
| `Deserialize(string, ParameterInfo[])` | Erkennt Format automatisch, deserialisiert in ein `object[]` ausgerichtet an den gegebenen Methodenparametern. Für Reflection-basierten Dispatch. |

### Format-Serializer

Als `serializer`-Argument an `PropertyBag.Serialize` übergeben:

| Serializer | Format |
|------------|--------|
| `IniDictionarySerializer.Serialize` | Sectionloses INI (`Key=Value`-Zeilen) |
| `JsonDictionarySerializer.Serialize` | Eingerücktes JSON mit String-Werten |

`IniDictionarySerializer` und `JsonDictionarySerializer` sind auch direkt nutzbar für
Low-Level-Dictionary-Zugriff.

### Format-Erkennung

`Deserialize` prüft das erste Nicht-Whitespace-Zeichen des Inputs: `{` → JSON, sonst INI.
Der Aufrufer muss das Format nicht separat tracken — beide Serializer erzeugen round-trip-fähige Ausgaben.

---

## Unterstützte Property-Typen

Alle Properties eines serialisierten Records müssen einem der folgenden Typen entsprechen.
Nullable-Wrapper (`T?`) sind für jeden Eintrag erlaubt. Alle `enum`-Typen werden unterstützt.

| Kategorie | Typen |
|---|---|
| Text | `string` |
| Zeichen | `char` |
| Boolean | `bool` |
| Integer | `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong` |
| Fließkomma | `float`, `double`, `decimal` |
| Datum / Zeit | `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan` |
| Identität | `Guid` |
| Enum | beliebiger `enum`-Typ |

Records mit anderen Property-Typen (verschachtelte Records, Collections, Arrays usw.)
schlagen bei der Serialisierung mit einer informativen Fehlermeldung fehl.

---

## Nullable Properties

- Eine nullable Property (`T?`), die im Input fehlt, wird stillschweigend auf `null` gesetzt.
- Eine nullable Property mit leerem Wert (`Key=`) wird ebenfalls auf `null` gesetzt.
- Eine nicht-nullable Property, die im Input fehlt, erzeugt einen Fehler.

```csharp
public record Config
{
    public required string Host    { get; init; }
    public int?            Timeout { get; init; }   // optional — darf fehlen
}
```

---

## Enum-Serialisierung

Enums werden nach Name serialisiert (`Color.Green` → `"Green"`) und case-insensitiv
deserialisiert (`"green"` → `Color.Green`).

---

## INI-Format — Details

- **Nur sectionlos** für Record-Serialisierung — kein `[Section]`-Header.
- **Whitespace wird getrimmt** — Keys und Values werden beidseitig getrimmt. `Key = value`
  und `Key=value` sind äquivalent. Intentioneller Whitespace in Werten muss auf Applikationsebene
  gehandhabt werden (z. B. durch Quoting).
- **Kommentare** — Zeilen mit `;` werden beim Deserialisieren ignoriert.
- **Doppelte Schlüssel** erzeugen einen Deserialisierungsfehler.

---

## JSON-Format — Details

- Ausgabe ist **eingerückt**; Enum-Werte als Strings.
- Der JSON-Input muss ein **flaches Objekt** mit ausschließlich String-Werten sein.
- Culture-aware Konverter für `DateTime`, `DateOnly`, `TimeOnly` und `decimal`
  (aktuell: `de-DE`).

---

## Kultur

Zahlen- und Datumsformatierung verwendet die **aktuelle Thread-Kultur** zum Zeitpunkt der
Serialisierung. Die kanonische JOSYN-Kultur ist `de-DE` (deklariert in `JosynCulture.Default`).

> **Wichtig:** `PropertyBag` setzt die Thread-Kultur **nicht** selbst. Jeder JOSYN-Host-Prozess
> setzt sie beim Start:
> ```csharp
> CultureInfo.DefaultThreadCurrentCulture   = JosynCulture.Default;
> CultureInfo.DefaultThreadCurrentUICulture = JosynCulture.Default;
> ```
> Serialisierte Daten und der lesende Prozess müssen dieselbe Kultur verwenden —
> sonst ist Round-Trip-Treue für Zahlen und Daten nicht garantiert.

---

## Einschränkungen

**Nur flache Records.** Verschachtelte Records und Collections (`List<T>`, Arrays usw.)
werden nicht unterstützt.

**Schlüssel-Matching ist case-sensitiv** bei der Record-Deserialisierung. Property-Namen
im Record (PascalCase) müssen exakt mit den Schlüsseln im serialisierten String übereinstimmen.
Die `ParameterInfo[]`-Überladung wendet ein Erster-Buchstabe-Toggle als Komfort an.

**Beide Record-Schreibweisen funktionieren** — Init-Property- und Primary-Constructor-Stil:

```csharp
// ✅ Init-Property-Stil
public record JobRequest
{
    public required string JobId  { get; init; }
    public int             Retries { get; init; }
}

// ✅ Primary-Constructor (positional) Stil
public record JobRequest(string JobId, int Retries);
```

---

## Delegate-Typen

Die Format-Plug-in-Punkte sind zwei Delegates, die `PropertyBag` von einem spezifischen
Format entkoppeln:

```csharp
// Dictionary<string, string> → string  (von Serialize genutzt)
public delegate Result<string> DictionaryToStringSerializer(Dictionary<string, string> data);

// string → Dictionary<string, string>  (intern von Deserialize genutzt)
public delegate Result<Dictionary<string, string>> StringToDictionarySerializer(string str);
```

Eigene Serializer können durch Implementierung dieser Delegate-Signaturen eingesteckt werden.

---

## Parameter-Deserialisierung

`Deserialize(string raw, ParameterInfo[] parameters)` ist der Einstiegspunkt für Reflection-
basierten Dispatch. Er parst den Input und konstruiert ein `object[]`, das positional zu den
gegebenen Parametern ausgerichtet ist — bereit für `MethodBase.Invoke`.

- Schlüssel werden mit einem Erster-Buchstabe-Toggle gegen Parameter-Namen abgeglichen
  (z. B. `jobId` passt zu einem Parameter `JobId`).
- Nullable Parameter, die im Input fehlen, werden auf `null` gesetzt.
- Nicht-nullable Parameter, die im Input fehlen, erzeugen einen Fehler.

---

## Abhängigkeiten

- `JOSYN.Foundation.ResultPattern` — das Result-Pattern durchgängig eingesetzt.
- `.NET 10` / C# `latest`.

---

## Für Maintainer

### Bauen, Testen, Packen

```
.local-build\build.cmd          # Release-Build
.local-build\build.cmd Debug    # Debug-Build
.local-build\test.cmd           # Alle Tests ausführen
.local-build\pack.cmd           # NuGet-Paket → ..\..\local-packages\
```

### Projektstruktur

```
JOSYN.Foundation.PropertyBag\
├── PropertyBag.cs
├── JosynCulture.cs
├── SupportedPropertyTypes.cs
├── Contracts\
│   ├── IPropertyBag.cs
│   ├── IIniDictionarySerializer.cs
│   ├── IJsonDictionarySerializer.cs
│   └── IJosynCulture.cs
├── DictionarySerializers\
│   ├── IniDictionarySerializer.cs
│   └── JsonDictionarySerializer.cs
├── CultureAwareConverters\
│   ├── CultureAwareDateOnlyConverter.cs
│   ├── CultureAwareDateTimeConverter.cs
│   ├── CultureAwareDecimalConverter.cs
│   └── CultureAwareTimeOnlyConverter.cs
└── DelegateTypes\
    ├── DictionaryToStringSerializer.cs
    └── StringToDictionarySerializer.cs
```

---

*JOSYN.Foundation.PropertyBag — © 2026 HAEVG AG — MIT License*
