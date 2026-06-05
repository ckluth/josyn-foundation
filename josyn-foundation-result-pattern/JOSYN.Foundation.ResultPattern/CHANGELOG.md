# Changelog

Alle relevanten Änderungen an diesem Paket werden hier dokumentiert.  
Format orientiert sich an [Keep a Changelog](https://keepachangelog.com/de/1.0.0/).

---

## [1.0.0-preview02] — 2026-06-05

### Removed

- `implicit operator Error(string)` — removed from both `Error` and the `IError<TSelf>` contract.

**Why:** When `TValue = string`, the implicit `string → Error` conversion created a silent trap
in ternary expressions. Given `Result<string> GetValue() => found ? value : Result.Error("msg")`,
the C# compiler resolved the common type of both branches as `Error` (not `Result<string>`),
silently converting the success value `"value"` to `Error("value")` — a failure. The code
compiled without warnings and produced wrong results at runtime.
Removing this conversion eliminates the trap entirely. The idiomatic replacement is
`Result<T>.Success(value)` on the success branch, or `new Error("msg")` / `Result.Error("msg")`
when constructing an `Error` value directly.

---

## [1.0.0-preview01] — 2026-05-26

Erste stabile Kandidatenversion.
Das Paket gilt als produktionsreif für den internen Einsatz; die Preview-Kennzeichnung
spiegelt den noch offenen Abnahme-Prozess wider.

