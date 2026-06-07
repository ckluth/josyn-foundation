# josyn-foundation

**josyn-foundation** contains the three core building blocks of **JOSYN** (*JobSystem Next*) —
as independent NuGet packages that can be versioned and deployed separately.

---

## Building blocks

| Package | Role | Dependencies |
|---|---|---|
| [`JOSYN.Foundation.ResultPattern`](josyn-foundation-result-pattern/README.md) | Errors-as-values — JOSYN's consistent error-handling principle | — |
| [`JOSYN.Foundation.PropertyBag`](josyn-foundation-property-bag/README.md) | Serialization of flat records for JOSYN IPC channels (INI / JSON) | ResultPattern |
| [`JOSYN.Foundation.JIP`](josyn-foundation-jip/README.md) | Named-pipe IPC transport (JOSYN Interprocess Protocol) | ResultPattern |

### Dependency chain

```
JOSYN.Foundation.ResultPattern
        ↑                ↑
JOSYN.Foundation.   JOSYN.Foundation.
   PropertyBag           JIP
```

`ResultPattern` is the only shared dependency. `PropertyBag` and `JIP` are unaware of each other.

---

## Local development

Each sub-repo is self-contained. From the respective directory:

```
.local-build\build.cmd      # release build
.local-build\test.cmd       # run tests
.local-build\pack.cmd       # NuGet package → ..\..\local-packages\
```

**First-time setup order** (due to dependencies):

```
1. josyn-foundation-result-pattern\  → pack
2. josyn-foundation-property-bag\    → pack
3. josyn-foundation-jip\             → pack
```

---

## Status

Milestone 1. Packages are internally production-ready;
the `preview` label reflects the pending release process.

---

*JOSYN Foundation — © 2026 HAEVG AG — MIT License*
