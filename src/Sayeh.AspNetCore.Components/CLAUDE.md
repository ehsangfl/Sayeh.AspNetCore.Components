# CLAUDE.md — Sayeh.AspNetCore.Components

Guidance for Claude Code when working in this project specifically. See the repo-root `CLAUDE.md` for
solution-wide build/test/CI info.

## What this project is

The component library: Razor components + code-behind targeting `net8.0;net10.0`, built on
`Microsoft.FluentUI.AspNetCore.Components`. In Debug config it project-references
`Sayeh.AspNetCore.Essentials.Core`; in Release config it package-references it instead.

## SayehDataGrid

`Components/DataGrid/SayehDataGrid.razor(.cs)` is a large partial class (also split across
`SayehDataGrid.razor.Edit.cs` for edit-mode logic) modeled closely on FluentUI/QuickGrid's own
`FluentDataGrid` internals — the same patterns show up here (`InternalGridContext<TItem>` cascaded to
descendants, `GridItemsProvider<TItem>`/`GridItemsProviderRequest`/`GridItemsProviderResult` for paged
data loading, `Virtualize<T>` integration, a `.razor.js` companion module loaded via `IJSObjectReference`
for column-resize/keyboard interop). Its associated JS lives at
`Components/DataGrid/SayehDataGrid.razor.js` and is referenced by content path
(`_content/Sayeh.AspNetCore.Components/Components/DataGrid/SayehDataGrid.razor.js`), so any JS change
must also be checked in the Sample app (whose compiled/served static assets pick it up automatically).

Columns (`Components/DataGrid/Columns/*`) all derive from the abstract
`Components/DataGrid/Columns/SayehColumnBase.razor.cs`. Column *capability* is expressed via marker
interfaces rather than flags — a column automatically becomes sortable/filterable/editable if it
implements `ISortableColumn<TItem>`, `IFilterableColumn<TItem,TValue>`, or `IEditableColumn<TItem>`
(`Components/DataGrid/Infrastructure/ColumnTypes/*`); `SayehColumnBase`'s constructor checks
`GetType()` against these interfaces to set `IsEditable`/default sortability/filterability. When adding a
new column type, implement the relevant interface(s) rather than adding new boolean parameters.

Row/cell edit lifecycle (`IEditableColumn<TItem>`) is: `BeginEdit(item)` → user edits → either
`UpdateSource()` (commit) or `CancelEdit()`; `GetCurrentValue()`/`GetEditPropertyPath()` are used by the
grid to validate/locate the edited value, and `CellEditContent(builder)` renders the edit-mode UI for the
cell (see `DataGridBeginEditEventArgs`, `DataGridCellEditEndingEventArgs`/`DataGridCellEditEndedEventArgs`,
`DataGridRowEditEndingEventArgs`/`DataGridRowEditEndedEventArgs` in `Infrastructure/EditEventArgs/` for the
associated cancellable event args).

## Observable components

`Common/ObservableComponent<T>` (`T : INotifyPropertyChanged`) and `Common/ObservableCollectionComponent`
are base classes that re-render automatically when a bound model raises `PropertyChanged` (optionally
filtered to one `PropertyName`) — used for reactive detail panels bound to a `TItem` selected in a grid or
tree. See `Sample.Client/Pages/ObservableComponentPage.razor(.ViewModel.cs)` and
`Model/WeatherForecastModel.NotifyPropertyChanged.cs` in the Sample project for the intended usage pattern.

`Infrastructure/EventCallbackSubscribable<T>` / `EventCallbackSubscriber<T>` provide a pub/sub mechanism
for `EventCallback<T>` (as opposed to plain C# events), preserving Blazor's async render/error flow —
used internally by the grid for cross-component notifications (e.g. `ColumnsCollectedNotifier`).

## TreeView / HierarchyPicker

`Components/TreeView/SayehTreeView*` and `Components/HierarchyPicker/*` implement generic, virtualization-
capable tree/hierarchy UIs driven by `Children`/`Parent` selector delegates (not a fixed model type) — see
`SetSelectedItemOnInitializeTest` in the test project's `TreeView/SayehTreeView.Test.cs` for how selection
state is resolved by walking up `Parent` and expanding ancestors when `SelectedItem` is set imperatively
rather than via click.
