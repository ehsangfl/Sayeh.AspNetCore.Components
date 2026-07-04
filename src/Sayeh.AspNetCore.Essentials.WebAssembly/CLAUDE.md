# CLAUDE.md — Sayeh.AspNetCore.Essentials.WebAssembly

Guidance for Claude Code when working in this project specifically. See the repo-root `CLAUDE.md` for
solution-wide build/test/CI info.

## What this project is

Blazor WebAssembly-side helpers: `LocalStorage/ILocalStorage.cs` + `LocalStorage/LocalStorage.cs` (wraps
`Blazored.LocalStorage`), and a client-side `LocalizationManager.cs` plus `EssentialExtensions` for DI
registration. Depends on `Sayeh.AspNetCore.Essentials.Core` (project reference in Debug, package
reference in Release — see repo-root `CLAUDE.md`).

Anything here should genuinely be WASM/browser-only (local storage, client-side culture handling); code
with no such dependency belongs in `Essentials.Core` instead.
