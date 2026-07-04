# CLAUDE.md — Sayeh.AspNetCore.Components.Test

Guidance for Claude Code when working in this project specifically. See the repo-root `CLAUDE.md` for
solution-wide build/test/CI info and test commands.

## What this project is

MSTest (`MSTest.Sdk`) + `bunit` test project for `src/Sayeh.AspNetCore.Components`, targeting
`net8.0;net10.0`. Project-references the component library directly (no Debug/Release conditional).

## Conventions

- All test classes should inherit `TestBase` (`TestBase.cs`), a `BunitContext` subclass whose
  `[TestInitialize]` registers FluentUI DI services (`Services.AddFluentUIComponents()`) and a mocked
  `IJSRuntime` (`Mock/JsRuntimeMock.cs`). Don't re-register these manually in individual test classes.
- To assert against private component state that has no public API (e.g. a grid/tree's internal selected
  node field), use the vendored `PrivateObject`/`PrivateType` helpers in `PrivateObject/` — see
  `TreeView/SayehTreeView.Test.cs` (`new PrivateObject(cut.Instance)` then `.GetField("_selectedNode")`)
  for the pattern, including polling with a short delay when the assertion depends on an async
  post-render effect.
- Demo/domain model types used in tests (`CustomTypeModel`, `HierarchycalItem`, `NestedItem`,
  `WeatherForecastGroup`, `WeatherForecastModel`, etc.) are **not** owned by this project — they're
  linked in via `<Compile Include>` from `Sample/Sample.Client/Model/*.cs` in the `.csproj`. Add new
  shared test/demo models there, then add a matching `<Compile Include>` entry here if the test project
  needs it, rather than duplicating a model in both places.
