# JOSYN.Foundation.ResultPattern

**Exceptions sind kein Rückgabekanal.** Dieses Paket stellt das *Result-Pattern* bereit:
eine typsichere Alternative zu `throw`/`catch`, bei der jede Operation ihren Erfolg oder
Misserfolg als Rückgabewert transportiert — samt Fehlermeldung, optionaler Exception und
einem automatisch aufgebautem Callstack.

---

## Warum Result statt Exceptions?

`try`/`catch` hat seinen Platz — aber als primäres Mittel zur Fehlerbehandlung bringt es
Probleme mit:

- **Unsichtbarer Kontrollfluss.** Ein `throw` in einer Hilfsmethode kann den Aufrufer
  überraschend beenden, ohne dass die Signatur irgendeinen Hinweis gibt.
- **Erzwungener Kontext-Wechsel.** Der Aufrufer muss entscheiden, ob er fängt oder
  weiter wirft — und vergisst er es, propagiert die Exception unkontrolliert.
- **Kein strukturierter Callstack.** `Exception.StackTrace` zeigt den technischen
  Ausführungspfad, nicht den *fachlichen* Aufrufpfad.

Das Result-Pattern macht Fehler zu **first-class-Werten**: Der Compiler zwingt den
Aufrufer, den Misserfolgsfall zu berücksichtigen, und `Propagate()` baut dabei
automatisch einen lesbaren Callstack auf.

---

## Schnellstart

```csharp
// Erfolg
return Result.Success;
return Result<MyType>.Success(value);

// Misserfolg
return Result.Fail("Etwas ist schiefgelaufen.");
return Result.Error("Ungültige Eingabe.");      // idiomatische Kurzform

// Exception abfangen — immer so
catch (Exception ex) { return ex; }

// Fehler in der Aufrufkette weitergeben
var result = TuEtwas();
if (!result.Succeeded) return Result.Propagate(result);

// Ergebnis auswerten
if (!result.Succeeded)
{
    logger.Error(result.ErrorMessage);
    logger.Error(result.CallStackAsString);
}

var value = result.Value; // nur nach Succeeded == true zugreifen
```

---

## Kernkonzepte

### `Result` und `Result<T>`

Zwei Record-Typen: `Result` für void-Operationen, `Result<T>` für Operationen mit
Rückgabewert. Beide implementieren dasselbe Interface und verhalten sich identisch.

```csharp
Result         DoSomething()        { ... }
Result<string> LoadUserName(int id) { ... }
```

### `Error` — idiomatische Rückgabe

`Result.Error(...)` erzeugt einen `Error`-Wert, der implizit in `Result` *und*
`Result<T>` konvertiert. Das erlaubt eine kompakte Schreibweise ohne explizites Casting:

```csharp
Result<int> Parse(string s) =>
    int.TryParse(s, out var n) ? n : Result.Error($"Kein Integer: {s}");
```

### Implizite Konvertierungen

```csharp
// Wert → Result<T>
Result<int> result = 42;

// Exception → Result (im catch-Block)
catch (Exception ex) { return ex; }

// Error → Result oder Result<T>
return Result.Error("Nachricht");
```

> **Ternary-Falle bei `Result<string>`:** Wenn `TValue = string` ist, löst C# den gemeinsamen
> Typ beider Ternary-Zweige auf — **nicht** den Zieltyp der Methode. Den Erfolgs-Zweig daher
> immer explizit mit `Result<string>.Success(value)` schreiben:
>
> ```csharp
> // FALSCH — "INT" wird lautlos zu Error("INT"):
> Result<string> GetValue(string key) =>
>     dict.TryGetValue(key, out var v) ? v : Result.Error("not found");
>
> // RICHTIG:
> Result<string> GetValue(string key) =>
>     dict.TryGetValue(key, out var v) ? Result<string>.Success(v) : Result.Error("not found");
> ```

### `Propagate()` — Callstack aufbauen

Jeder Aufruf von `Propagate()` fügt den aktuellen Aufrufer zur Kette hinzu. Am Ende
enthält `CallStackAsString` den vollständigen fachlichen Pfad — nützlich für Logging
und Debugging.

```csharp
private Result<string> ReadUserName()
{
    var result = LoadUserRecord();
    if (!result.Succeeded) return Result<string>.Propagate(result.ToResult<string>());
    return result.Value.ToString();
}
```

### Zwischen Typen wechseln

`ToResult()` und `ToResult<T>()` konvertieren einen *fehlgeschlagenen* Result-Wert in
einen anderen Typ, ohne den Fehler zu verlieren — nötig, wenn die Aufrufkette verschiedene
`Result<T>`-Typen durchläuft.

```csharp
Result<int> ParseUserAge()
{
    var result = ReadUserName();                                    // Result<string>
    if (!result.Succeeded) return Result<int>.Propagate(result.ToResult<int>());
    return int.Parse(result.Value);
}
```

---

## Vollständiges Beispiel

```csharp
// Tiefste Ebene: Exception wird zum Result
Result<Guid> LoadUserRecord()
{
    try { return repository.GetUser(id); }
    catch (Exception ex) { return ex; }
}

// Mittlere Ebenen: Fehler wandern nach oben
Result<string> ReadUserName()
{
    var r = LoadUserRecord();
    if (!r.Succeeded) return Result<string>.Propagate(r.ToResult<string>());
    return r.Value.ToString();
}

Result<int> ParseUserAge()
{
    var r = ReadUserName();
    if (!r.Succeeded) return Result<int>.Propagate(r.ToResult<int>());
    return int.Parse(r.Value);
}

// Oberste Ebene: Fehler auswerten
Result result = ParseUserAge().ToResult();
if (!result.Succeeded)
{
    Console.WriteLine(result.ErrorMessage);
    Console.WriteLine(result.CallStackAsString);
    // z. B.:
    //   at LoadUserRecord in UserRepository.cs, line 42
    //   at ReadUserName in UserService.cs, line 18
    //   at ParseUserAge in UserService.cs, line 27
}
```

---

## Referenz

| Member | Beschreibung |
|---|---|
| `Result.Success` | Erfolgreiches void-Result |
| `Result<T>.Success(value)` | Erfolgreiches typisiertes Result |
| `Result.Fail(message)` | Fehlgeschlagenes Result mit Nachricht |
| `Result.Fail(exception)` | Fehlgeschlagenes Result aus Exception |
| `Result.Error(message)` | Kurzform: erzeugt `Error`-Wert für idiomatische Rückgabe |
| `result.Succeeded` | `true` wenn erfolgreich; `false` garantiert `ErrorMessage != null` |
| `result.Value` | Nur nach `Succeeded == true` zugreifen |
| `result.ErrorMessage` | Fehlermeldung |
| `result.Exception` | Ursprüngliche Exception, falls vorhanden |
| `result.Callers` | Liste der Propagierungs-Frames |
| `result.CallStackAsString` | Lesbarer Callstack für Logging |
| `result.ToString()` | `[Erfolgreich]` bzw. `[Erfolgreich] {Wert}` oder Fehlermeldung + Callstack |
| `Result.Propagate(result)` | Fügt aktuellen Aufrufer zur Kette hinzu |
| `result.ToResult()` | `Result<T>` → `Result` (nur für Failures) |
| `result.ToResult<T>()` | Konvertiert Failure in anderen `Result<T>`-Typ |

---

## Für Maintainer

### Versionierung

`Major.Minor.Patch` gemäß Semantic Versioning. Pre-Release-Versionen tragen ein Suffix
(z. B. `-preview01`) gemäß interner HAEVG-Versionierungsrichtlinie. Der aktuelle Stand
ist in `CHANGELOG.md` dokumentiert.

### Voraussetzungen

.NET 10 SDK, C# (latest)

### Projektstruktur

```
JOSYN.Foundation.ResultPattern\
├── Result.cs                   # void-Result
├── Result.generic.cs           # Result<T>
├── Support\
│   ├── CallerInfo.cs           # Ein Frame im Propagierungs-Callstack
│   ├── Error.cs                # Hilfstyp für idiomatische Fehlerrückgabe
│   ├── ResultHelper.cs         # Interne Hilfsfunktionen
│   └── ResultSuccess.cs        # Sentinel-Typ für Result.Success
└── Contracts\
    ├── IResult.cs              # API-Vertrag void-Result
    ├── IResult.generic.cs      # API-Vertrag Result<T>
    └── IError.cs               # Gemeinsamer Failure-Vertrag
```

### Abhängigkeiten

Keine externen Abhängigkeiten.

### Hinweise

- Fehlermeldungen sind bewusst auf **Deutsch** gehalten (interner Standard).
- `Result` und `Result<T>` sind C#-Records. Record-Gleichheit vergleicht alle
  Properties — zwei `Fail`-Results mit gleicher Nachricht sind **nicht** record-gleich,
  da ihre `Callers`-Listen unterschiedliche Instanzen sind.
- `Debug.Assert` in `Propagate()` schlägt an, wenn diese Methode auf einem erfolgreichen Result
  aufgerufen werden — nur im Debug-Build sichtbar.
  `ToResult<T>()` wirft immer eine `InvalidOperationException` wenn auf einem erfolgreichen Result aufgerufen.

---

*JOSYN.Foundation.ResultPattern — © 2026 HAEVG AG — MIT License*
