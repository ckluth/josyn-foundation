# Copilot Instructions — josyn-foundation

## Repository overview

This mono-repo contains three independent NuGet packages, each in its own subdirectory with its own solution (`.slnx`), test project, and local build scripts:

| Package | Subdirectory | Purpose |
|---------|--------------|---------|
| `JOSYN.Foundation.ResultPattern` | `JOSYN.Foundation.ResultPattern/` | Result pattern (`Result`/`Result<T>`) — errors as values, no exceptions |
| `JOSYN.Foundation.PropertyBag` | `JOSYN.Foundation.PropertyBag/` | Flat-record serializer (sectionless INI / JSON) for IPC payloads |
| `JOSYN.Foundation.JIP` | `JOSYN.Foundation.JIP/` | Named-pipe IPC transport (JOSYN Interprocess Protocol) |

Each package depends on `JOSYN.Foundation.ResultPattern`. `JOSYN.Foundation.JIP` additionally depends on `JOSYN.Foundation.PropertyBag`. Both consume the upstream packages from a **local NuGet feed** at `..\..\local-packages\` (relative to each package root).

**Stack:** .NET 10, C# `latest`, NUnit for tests. Build outputs go to `C:\Temp\VS.OUT\JOSYN\` (set in each `Directory.Build.props`).

---

## Build, test, and pack commands

Run from within each package's root directory (e.g., `JOSYN.Foundation.ResultPattern\`):

```cmd
.local-build\build.cmd              # Release build (default)
.local-build\build.cmd Debug        # Debug build
.local-build\test.cmd               # Run all tests (dotnet test)
.local-build\pack.cmd               # Pack NuGet → ..\..\local-packages\
```

**Run a single test** (from the package root):
```cmd
dotnet test --filter "FullyQualifiedName~ResultTests.Fail_String_Succeeded"
```
or by class:
```cmd
dotnet test --filter "ClassName~ResultTests"
```

---

## Coding conventions

### Static-first

Default to `static class` and `static` methods. Choose instance types only when mutable state, genuine polymorphism, or a natural identity is required. When in doubt — static wins.

### Immutability by default

- `record` over `class`; `readonly` fields; `init`-only properties.
- Mutable state requires explicit justification.

### Errors as values — never exceptions

`Result` / `Result<T>` is the single error-propagation mechanism throughout all three packages.

```csharp
// Return success
return Result.Success;
return Result<MyType>.Success(value);

// Return failure — idiomatic short form
return Result.Error("Fehler aufgetreten.");

// Catch at the lowest layer only — convert exception to Result
catch (Exception ex) { return ex; }

// Propagate up the call chain (builds the call stack)
var r = DoSomething();
if (!r.Succeeded) return Result.Propagate(r);

// Cross-type propagation
var r = LoadString();                          // Result<string>
if (!r.Succeeded) return Result<int>.Propagate(r.ToResult<int>());
```

**Never** re-wrap a failure manually. **Never** use `throw` / `try-catch` above the lowest conversion layer.

### Interfaces as API contracts

Every public static type has a corresponding interface with `static abstract` members, placed in a `Contracts/` folder. Implementations use `/// <inheritdoc cref="IXxx.Member"/>` (static classes) or `/// <inheritdoc/>` (non-static implementations).

### XML documentation

XML docs go on the **interface**, not the implementation. Implementations reference them via `<inheritdoc>`.

### Language

- Error messages: **German** (project convention).
- XML docs and all code comments: **English**.
- Thread culture: `de-DE` (`JosynCulture.Default`). Every JOSYN host process must set this at startup:
  ```csharp
  CultureInfo.DefaultThreadCurrentCulture   = JosynCulture.Default;
  CultureInfo.DefaultThreadCurrentUICulture = JosynCulture.Default;
  ```

### Namespace declaration

Each file uses `#pragma warning disable/restore IDE0130` around the namespace declaration — this is intentional and must be preserved.

---

## Package-specific notes

### ResultPattern

`Result` and `Result<T>` are sealed records. Record equality compares all properties — two `Fail` results with the same message are **not** record-equal because their `Callers` lists differ.

`Result.Error(message)` returns an `Error` value that implicitly converts to both `Result` and `Result<T>`. This is the preferred short form for returning failures.

`Propagate()` is only valid on a failed result — a `Debug.Assert` fires in Debug builds if called on a success.

### PropertyBag

Serializes **flat** `record` types only. Nested records and collections are not supported and will produce an informative error.

Format auto-detection: if the first non-whitespace character is `{`, JSON is assumed; otherwise INI.

Key matching for record deserialization is **case-sensitive**. The `ParameterInfo[]` overload applies a first-character case toggle as a convenience.

### JIP (JOSYN Interprocess Protocol)

Two-layer architecture:
- **Transport layer** (`PipesServer`, `PipesClient`, `PipesProtocol`): raw byte exchange over named pipes using a length-prefix protocol (little-endian `int32` + bytes).
- **Conventions layer** (`JipClient`, `JipServer`, `JipProtocol`, `JipDispatcher`): JSON-based `Request`/`Response` wire types on top of the transport.

Each session uses exactly two named pipes (`req-pipe-<sessionKey>` and `res-pipe-<sessionKey>`). The server passes the session key to the client as a CLI argument: `"JOSYN-IPC <sessionKey>"`.

`JipDispatcher` is the preferred way to register handlers server-side: `RegisterAll<TProtocol>` registers all handlers of a protocol type in one call.

Current limitation (accepted PoC constraint): single-in-flight — requests are processed strictly sequentially, no multiplexing.
