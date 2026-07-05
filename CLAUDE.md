# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository overview

Sayeh.AspNetCore.Components ("Sayeh.Blazor.Components") is a set of Blazor UI components built on top of
Microsoft's `Microsoft.FluentUI.AspNetCore.Components` (FluentUI Blazor). The flagship component is
`SayehDataGrid`, a fork/extension of FluentUI's `FluentDataGrid` that adds inline cell/row editing,
column resize, multi-column sort, and filter UI. Published to NuGet as four separate packages.

Each project below has its own `CLAUDE.md` with project-specific architecture notes — read the one
relevant to whatever you're editing in addition to this file.

## Solution layout

- `src/Sayeh.AspNetCore.Components` — the component library (Razor components + code-behind). See
  `src/Sayeh.AspNetCore.Components/CLAUDE.md`.
- `src/Sayeh.AspNetCore.Essentials.Core` — framework-agnostic helpers/services. See
  `src/Sayeh.AspNetCore.Essentials.Core/CLAUDE.md`.
- `src/Sayeh.AspNetCore.Essentials.Server` — server-side ASP.NET Core helpers. See
  `src/Sayeh.AspNetCore.Essentials.Server/CLAUDE.md`.
- `src/Sayeh.AspNetCore.Essentials.WebAssembly` — Blazor WASM-side helpers. See
  `src/Sayeh.AspNetCore.Essentials.WebAssembly/CLAUDE.md`.
- `Sample/Sample` + `Sample/Sample.Client` — Blazor Web App host + WASM client with demo pages for every
  component. Run this to manually exercise a change. See `Sample/CLAUDE.md`.
- `Test/Sayeh.AspNetCore.Components.Test/Sayeh.AspNetCore.Components.Test` — MSTest + bUnit test project.
  See `Test/Sayeh.AspNetCore.Components.Test/Sayeh.AspNetCore.Components.Test/CLAUDE.md`.

Project references vs. package references are configuration-conditional: **Debug** builds use
`ProjectReference` between the four library projects; **Release** builds use `PackageReference` to the
published NuGet packages (see the `Condition="'$(Configuration)|$(Platform)'=='...'"` groups in each
`.csproj`). This means Release-configuration builds require the referenced package versions to already
exist on the configured NuGet feed — normal local development should use the Debug configuration.

Shared MSBuild properties (target frameworks, FluentUI version, package version) live in
`Directory.Build.props` at the repo root — bump `PackageVersion` there when cutting a release ("update
version" commits touch only this file).

## Common commands

```
# restore / build everything (Debug uses project-to-project refs)
dotnet build Sayeh.AspNetCore.Components.sln -c Debug

# run the sample app (Blazor Web App hosting the WASM client) to manually verify UI changes
dotnet run --project Sample/Sample/Sample.csproj

# run the full test suite
dotnet test Test/Sayeh.AspNetCore.Components.Test/Sayeh.AspNetCore.Components.Test/Sayeh.AspNetCore.Components.Test.csproj

# run a single test by fully-qualified name
dotnet test Test/Sayeh.AspNetCore.Components.Test/Sayeh.AspNetCore.Components.Test/Sayeh.AspNetCore.Components.Test.csproj --filter "FullyQualifiedName~SayehTreeViewTest.SetSelectedItemOnInitializeTest"

# target a single TFM (project multi-targets net8.0;net10.0)
dotnet test Test/Sayeh.AspNetCore.Components.Test/Sayeh.AspNetCore.Components.Test/Sayeh.AspNetCore.Components.Test.csproj -f net10.0
```

## The Microsoft FluentUI Blazor Source code is available on H:\OpenSource\Blazor\BlazorFluent

## CI/CD

`azure-pipelines.yml` (triggers on the `Live` branch) builds, packs, and pushes each of the four
projects to NuGet **in dependency order** (Essentials.Core → Essentials.Server → Essentials.WebAssembly →
Components), sleeping 5 minutes after each push so the just-published package is available on the feed
before the next project restores it via `PackageReference` in Release configuration. Keep this ordering
in mind if you add a new project or change inter-project dependencies.
