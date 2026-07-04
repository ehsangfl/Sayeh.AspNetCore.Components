# CLAUDE.md — Sayeh.AspNetCore.Essentials.Server

Guidance for Claude Code when working in this project specifically. See the repo-root `CLAUDE.md` for
solution-wide build/test/CI info.

## What this project is

Server-hosting-specific ASP.NET Core helpers: request culture providers
(`Localization/AcceptLanguageHeaderRequestCultureProvider.cs`,
`Localization/IdentityRequestCultureProvider.cs`), `Localization/LocalizationManager.cs`, and
`EssentialExtensions`/`SayehOptions` for DI registration. References `Microsoft.AspNetCore.App` via
`FrameworkReference`, and depends on `Sayeh.AspNetCore.Essentials.Core` (project reference in Debug,
package reference in Release — see repo-root `CLAUDE.md`).

Anything here should genuinely require server hosting (`HttpContext`, request pipeline, etc.); code with
no such dependency belongs in `Essentials.Core` instead so WASM-only consumers aren't forced to pull in
ASP.NET Core.
