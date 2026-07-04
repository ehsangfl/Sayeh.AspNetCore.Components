# CLAUDE.md — Sample

Guidance for Claude Code when working in this project specifically. See the repo-root `CLAUDE.md` for
solution-wide build/test/CI info.

## What this is

A two-project Blazor Web App used as the manual test bed / live documentation site for every component
in `src/Sayeh.AspNetCore.Components`:

- `Sample/Sample` (`Microsoft.NET.Sdk.Web`, `net10.0`) — the server host. `Program.cs` wires up the
  Blazor Web App hosting model and serves `Sample.Client`. Project-references
  `Sayeh.AspNetCore.Essentials.Server` and `Sample.Client`.
- `Sample/Sample.Client` (`Microsoft.NET.Sdk.BlazorWebAssembly`, `net10.0`) — the WASM client with all
  demo pages. Project-references `Sayeh.AspNetCore.Components` and `Sayeh.AspNetCore.Essentials.WebAssembly`
  directly (not via the Debug/Release-conditional pattern used between the library projects), so it always
  builds against the local source.

Run with `dotnet run --project Sample/Sample/Sample.csproj` to manually verify a component change end to
end before considering it done — this is the standard way to exercise a UI change in this repo.

## Structure

Demo pages live under `Sample.Client/Pages/<Component>/*.razor`, one file per scenario (e.g.
`Pages/DataGrid/DataGrid-EditItem.razor`, `Pages/DataGrid/DataGrid-Virtualize.razor`,
`Pages/TreeView/TreeView-Selector.razor`). When adding a new capability to a component, add a
corresponding scenario page here rather than overloading an existing one — that's the existing
convention.

Shared demo models live in `Sample.Client/Model/*.cs` (e.g. `WeatherForecastModel`, `HierarchycalItem`,
`NestedItem`). **These files are also linked directly into the test project** via `<Compile Include>` in
`Test/.../Sayeh.AspNetCore.Components.Test.csproj` (not copied) — renaming or moving a model file here
will break the test project's compile unless that `.csproj` is updated too.
