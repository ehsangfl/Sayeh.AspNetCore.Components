# CLAUDE.md — Sayeh.AspNetCore.Essentials.Core

Guidance for Claude Code when working in this project specifically. See the repo-root `CLAUDE.md` for
solution-wide build/test/CI info.

## What this project is

Framework-agnostic helpers/services shared by every other project in the solution (e.g.
`ILocalizationManager`, `Reflection.cs` helpers, `ExtensionMethods.cs`, `NullResolver`). Targets
`net8.0;net10.0` with **no** ASP.NET Core or Blazor hosting-model dependency.

This is the first package built/packed/pushed in the release pipeline (`azure-pipelines.yml`) — every
other project in the solution ultimately depends on it, so a breaking change here has the widest blast
radius. If you add a hosting-specific dependency by mistake, it belongs in `Essentials.Server` or
`Essentials.WebAssembly` instead.

## Registration convention

Follow the existing `EssentialExtensions`-style static extension method on `IServiceCollection` when
adding new services that need DI registration, matching the pattern used in `Essentials.Server` and
`Essentials.WebAssembly`.
