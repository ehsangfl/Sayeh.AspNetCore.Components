using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.DataGrid.Infrastructure;
using Microsoft.FluentUI.AspNetCore.Components.Utilities;
using Microsoft.JSInterop;
using Sayeh.AspNetCore.Components.DataGrid.Infrastructure;
using Sayeh.AspNetCore.Components.Infrastructure;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using static Microsoft.FluentUI.AspNetCore.Components.Icons.Filled.Size20;
using static Microsoft.FluentUI.AspNetCore.Components.Icons.Light.Size32;
using static System.Formats.Asn1.AsnWriter;

namespace Sayeh.AspNetCore.Components
{
    partial class SayehDataGrid<TItem> : FluentComponentBase, IHandleEvent, IAsyncDisposable where TItem : class
    {

        #region Fileds

        //private const string JAVASCRIPT_FILE = "./_content/Microsoft.FluentUI.AspNetCore.Components/Components/DataGrid/FluentDataGrid.razor.js";
        private const string JAVASCRIPT_FILE = "./_content/Sayeh.AspNetCore.Components/DataGrid/SayehDataGrid.razor.js";
        public const string EMPTY_CONTENT_ROW_CLASS = "empty-content-row";
        public const string LOADING_CONTENT_ROW_CLASS = "loading-content-row";

        /// <summary />
        [Inject] private LibraryConfiguration LibraryConfiguration { get; set; } = default!;
        [Inject] private IServiceScopeFactory ScopeFactory { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private IKeyCodeService KeyCodeService { get; set; } = default!;

        private ElementReference? _gridReference;
        private Virtualize<(int, TItem)>? _virtualizeComponent;
        private AsyncServiceScope? _scope;

        // We cascade the InternalGridContext to descendants, which in turn call it to add themselves to _columns
        // This happens on every render so that the column list can be updated dynamically
        private readonly InternalGridContext<TItem> _internalGridContext;
        internal readonly List<SayehColumnBase<TItem>> _columns;
        private bool _collectingColumns; // Columns might re-render themselves arbitrarily. We only want to capture them at a defined time.

        // Tracking state for options and sorting
        private SayehColumnBase<TItem>? _displayOptionsForColumn;
        private SayehColumnBase<TItem>? _displayResizeForColumn;
        private List<SayehColumnBase<TItem>> _sortableColumns;
        private List<SayehColumnBase<TItem>> _filterableColumns;

        //if user applied multiple column sort to datasource, we need to display a column sort order badge to user. by this flag, we enable the badge part. 
        //for single column sort or no sort, we dont display sort order part.we can achieve this on each column, but this can be caused a performance issue
        internal bool multiColumnSorted;

        private bool _checkColumnOptionsPosition;
        private bool _checkColumnResizePosition;
        private bool _manualGrid;

        // The associated ES6 module, which uses document-level event listeners
        private IJSObjectReference? Module;
        private IJSObjectReference? _jsEventDisposable;

        // Caches of method->delegate conversions
        private readonly RenderFragment _renderColumnHeaders;
        private readonly RenderFragment _renderNonVirtualizedRows;


        private readonly RenderFragment _renderEmptyContent;
        private readonly RenderFragment _renderLoadingContent;

        private string? _internalGridTemplateColumns;

        // We try to minimize the number of times we query the items provider, since queries may be expensive
        // We only re-query when the developer calls RefreshDataAsync, or if we know something's changed, such
        // as sort order, the pagination state, or the data source itself. These fields help us detect when
        // things have changed, and to discard earlier load attempts that were superseded.
        private PaginationState? _lastRefreshedPaginationState;
        private IEnumerable<TItem>? _lastAssignedItems;
        private GridItemsProvider<TItem>? _lastAssignedItemsProvider;
        private CancellationTokenSource? _pendingDataLoadCancellationTokenSource;

        // If the PaginationState mutates, it raises this event. We use it to trigger a re-render.
        private readonly EventCallbackSubscriber<PaginationState> _currentPageItemsChanged;

        private SayehDataGridRow<TItem>? _currentRow { get; set; }

        bool ImplementedIEditableObject = false;
        bool observableHandled = false;
        IEnumerable<TItem> _oldItems;
        SayehDataGridRowsPart? RowsPart;
        bool _showRowHeaders;
        RowHeaderColumn<TItem> _rowHeader;
        /// <summary>
        /// Gets or sets a value indicating whether the grid's dataset is not expected to change during its lifetime.
        /// When set to true, reduces automatic refresh checks for better performance with static datasets.
        /// Default is false to maintain backward compatibility.
        /// </summary>
        bool _isFixed { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// A enumerable source of data for the grid.
        ///
        /// You should supply either <see cref="Items"/> or <see cref="RowsDataProvider"/>, but not both.
        /// </summary>
        [Parameter] public IEnumerable<TItem>? Items { get; set; }

        /// <summary>
        /// A callback that supplies data for the rid.
        ///
        /// You should supply either <see cref="Items"/> or <see cref="ItemsProvider"/>, but not both.
        /// </summary>
        [Parameter] public GridItemsProvider<TItem>? ItemsProvider { get; set; }

        /// <summary>
        /// Gets or sets the child components of this instance. For example, you may define columns by adding
        /// components derived from the <see cref="SayehColumnBase{TItem}"/> base class.
        /// </summary>
        [Parameter] public RenderFragment? ChildContent { get; set; }

        bool _virtualize { get; set; }
        /// <summary>
        /// If true, the grid will be rendered with virtualization. This is normally used in conjunction with
        /// scrolling and causes the grid to fetch and render only the data around the current scroll viewport.
        /// This can greatly improve the performance when scrolling through large data sets.
        ///
        /// If you use <see cref="Virtualize"/>, you should supply a value for <see cref="ItemSize"/> and must
        /// ensure that every row renders with the same constant height.
        ///
        /// Generally it's preferable not to use <see cref="Virtualize"/> if the amount of data being rendered
        /// is small or if you are using pagination.
        /// </summary>
        [Parameter] public bool Virtualize { get; set; }

        /// <summary>
        /// This is applicable only when using <see cref="Virtualize"/>. It defines how many additional items will be rendered
        /// before and after the visible region to reduce rendering frequency during scrolling. While higher values can improve
        /// scroll smoothness by rendering more items off-screen, they can also increase initial load times. Finding a balance
        /// based on your data set size and user experience requirements is recommended. The default value is 3.
        /// </summary>
        [Parameter] public int OverscanCount { get; set; } = 3;

        /// <summary>
        /// This is applicable only when using <see cref="Virtualize"/>. It defines an expected height in pixels for
        /// each row, allowing the virtualization mechanism to fetch the correct number of items to match the display
        /// size and to ensure accurate scrolling.
        /// </summary>
        [Parameter] public float ItemSize { get; set; } = 32;

        /// <summary>
        /// If true, renders draggable handles around the column headers, allowing the user to resize the columns
        /// manually. Size changes are not persisted.
        /// </summary>
        [Parameter] public bool ResizableColumns { get; set; }


        /// <summary>
        /// Optionally defines a value for @key on each rendered row. Typically this should be used to specify a
        /// unique identifier, such as a primary key value, for each data item.
        ///
        /// This allows the grid to preserve the association between row elements and data items based on their
        /// unique identifiers, even when the <typeparamref name="TGridItem"/> instances are replaced by new copies (for
        /// example, after a new query against the underlying data store).
        ///
        /// If not set, the @key will be the <typeparamref name="TGridItem"/> instance itself.
        /// </summary>
        [Parameter] public Func<TItem, object> ItemKey { get; set; } = x => x!;

        /// <summary>
        /// Optionally links this <see cref="DataGrid{TItem}"/> instance with a <see cref="PaginationState"/> model,
        /// causing the grid to fetch and render only the current page of data.
        ///
        /// This is normally used in conjunction with a <see cref="FluentPaginator"/> component or some other UI logic
        /// that displays and updates the supplied <see cref="PaginationState"/> instance.
        /// </summary>
        [Parameter] public PaginationState? Pagination { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the component will not add itself to the tab queue.
        /// Default is false.
        /// </summary>
        [Parameter]
        public bool NoTabbing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grid should automatically generate a header row and its type.
        /// See <see cref="GenerateHeaderOption"/>
        /// </summary>
        [Parameter]
        public GenerateHeaderOption? GenerateHeader { get; set; } = GenerateHeaderOption.Sticky;

        /// <summary>
        /// Gets or sets the value that gets applied to the css gridTemplateColumns attribute of child rows.
        /// Can be specified here or on the column level with the Width parameter but not both.
        /// Needs to be a valid CSS string of space-separated values, such as "auto 1fr 2fr 100px".
        /// </summary>
        [Parameter]
        public string? GridTemplateColumns { get; set; } = null;

        /// <summary>
        /// Gets or sets a callback when a row is focused.
        /// </summary>
        [Parameter]
        public EventCallback<SayehDataGridRow<TItem>> OnRowFocus { get; set; }

        /// <summary>
        /// Gets or sets a callback when a row is focused.
        /// </summary>
        [Parameter]
        public EventCallback<SayehDataGridCell<TItem>> OnCellFocus { get; set; }

        /// <summary>
        /// Gets or sets a callback when a row is clicked.
        /// </summary>
        [Parameter]
        public EventCallback<SayehDataGridRow<TItem>> OnRowClick { get; set; }

        /// <summary>
        /// Gets or sets a callback when a row is double-clicked.
        /// </summary>
        [Parameter]
        public EventCallback<SayehDataGridRow<TItem>> OnRowDoubleClick { get; set; }

        /// <summary>
        /// Optionally defines a class to be applied to a rendered row.
        /// </summary>
        [Parameter] public Func<TItem, string>? RowClass { get; set; }

        /// <summary>
        /// Optionally defines a style to be applied to a rendered row.
        /// Do not use to dynamically update a row style after rendering as this will interfere with the script that use this attribute. Use <see cref="RowClass"/> instead.
        /// </summary>
        [Parameter] public Func<TItem, string>? RowStyle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grid should show a hover effect on rows.
        /// </summary>
        [Parameter] public bool ShowHover { get; set; }

        /// <summary>
        /// Gets or sets a data bounded to selected row 
        /// </summary>
        [Parameter]
        public TItem? SelectedItem { get; set; }

        [Parameter]
        public EventCallback<TItem> SelectedItemChanged { get; set; }

        /// <summary>
        /// If specified, grids render this fragment when there is no content.
        /// </summary>
        [Parameter] public RenderFragment? EmptyContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grid is in a loading data state.
        /// </summary>
        [Parameter] public bool Loading { get; set; }

        /// <summary>
        /// Gets or sets the content to render when <see cref="Loading"/> is true.
        /// A default fragment is used if loading content is not specified.
        /// </summary>
        [Parameter] public RenderFragment? LoadingContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grid should allow multiple lines of text in cells.
        /// </summary>
        [Parameter]
        public bool MultiLine { get; set; } = false;

        /// <summary>
        /// Gets or sets the size of each row in the grid based on the <see cref="DataGridRowSize"/> enum.
        /// </summary>
        [Parameter]
        public DataGridRowSize RowSize { get; set; } = DataGridRowSize.Small;

        private DataGridDisplayMode _displayMode;
        [Parameter]
        public DataGridDisplayMode DisplayMode { get; set; } = DataGridDisplayMode.Grid;

        /// <summary>
        /// Sets <see cref="GridTemplateColumns"/> to automatically fit the columns to the available width as best it can.
        /// </summary>
        [Parameter]
        public bool AutoFit { get; set; }

        /// <summary>
        /// Show or hide the RowHeaders column
        /// Row headers is a additional column with display the row state
        /// </summary>
        [Parameter]
        public bool ShowRowHeaders { get; set; } = true;

        /// <summary>
        /// (Aria) Labels used in the column resize UI.
        /// </summary>
        [Parameter]
        public ColumnResizeLabels ColumnResizeLabels { get; set; } = ColumnResizeLabels.Default;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructs an instance of <see cref="FluentDataGrid{TGridItem}"/>.
        /// </summary>
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DataGridCellFocusEventArgs))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DataGridRowFocusEventArgs))]

        public SayehDataGrid()
        {
            Id = Identifier.NewId();
            _columns = new();
            _internalGridContext = new(this);
            _currentPageItemsChanged = new(EventCallback.Factory.Create<PaginationState>(this, RefreshDataCoreAsync));
            _renderColumnHeaders = RenderColumnHeaders;
            _renderNonVirtualizedRows = RenderNonVirtualizedRows;
            _renderEmptyContent = RenderEmptyContent;
            _renderLoadingContent = RenderLoadingContent;

            _sortableColumns = new();
            _filterableColumns = new();
            ImplementedIEditableObject = typeof(IEditableObject).IsAssignableFrom(typeof(TItem));

            // As a special case, we don't issue the first data load request until we've collected the initial set of columns
            // This is so we can apply default sort order (or any future per-column options) before loading data
            // We use EventCallbackSubscriber to safely hook this async operation into the synchronous rendering flow
            EventCallbackSubscriber<object?>? columnsFirstCollectedSubscriber = new(
            EventCallback.Factory.Create<object?>(this, RefreshDataCoreAsync));
            columnsFirstCollectedSubscriber.SubscribeOrMove(_internalGridContext.ColumnsFirstCollected);
        }

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            KeyCodeService.RegisterListener(OnKeyDownAsync);
        }

        #endregion

        #region Functions

        protected override Task OnParametersSetAsync()
        {
            if (_virtualize != Virtualize && Virtualize)
            {
                _virtualize = Virtualize;
                DisplayMode = DataGridDisplayMode.Table;
            }

            if (GridTemplateColumns is not null)
            {
                _internalGridTemplateColumns = GridTemplateColumns;
            }

            // The associated pagination state may have been added/removed/replaced
            _currentPageItemsChanged.SubscribeOrMove(Pagination?.GetType().GetProperty("CurrentPageItemsChanged")?.GetValue(Pagination) as EventCallbackSubscribable<PaginationState>);

            if (Items is not null && ItemsProvider is not null)
            {
                throw new InvalidOperationException($"SayehDataGrid requires one of {nameof(Items)} or {nameof(ItemsProvider)}, but both were specified.");
            }
            if (_oldItems != Items)
            {
                if (_oldItems is not null && _oldItems.IsA<INotifyCollectionChanged>())
                    _oldItems.As<INotifyCollectionChanged>().CollectionChanged -= OnItemsChanged;

                if (Items is not null && Items.IsA<INotifyCollectionChanged>())
                {
                    _oldItems = Items;
                    Items.As<INotifyCollectionChanged>().CollectionChanged += OnItemsChanged;
                    _isFixed = false;
                }
                else
                    _isFixed = true;
            }
            // Perform a re-query only if the data source or something else has changed
            var dataSourceHasChanged = !Equals(ItemsProvider, _lastAssignedItemsProvider) || !ReferenceEquals(Items, _lastAssignedItems);
            if (dataSourceHasChanged)
            {
                _scope?.Dispose();
                _scope = ScopeFactory.CreateAsyncScope();
                _lastAssignedItemsProvider = ItemsProvider;
                _lastAssignedItems = Items;
            }

            var paginationStateHasChanged =
                Pagination?.ItemsPerPage != _lastRefreshedPaginationState?.ItemsPerPage
                || Pagination?.CurrentPageIndex != _lastRefreshedPaginationState?.CurrentPageIndex;

            var mustRefreshData = dataSourceHasChanged || paginationStateHasChanged ;

            // We don't want to trigger the first data load until we've collected the initial set of columns,
            // because they might perform some action like setting the default sort order, so it would be wasteful
            // to have to re-query immediately
            return (_columns.Count > 0 && mustRefreshData) ? RefreshDataCoreAsync() : Task.CompletedTask;
        }

        private async void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => await RefreshDataCoreAsync();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && _gridReference is not null)
            {
                Module ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", JAVASCRIPT_FILE);
                try
                {
                    _jsEventDisposable = await Module.InvokeAsync<IJSObjectReference>("init", _gridReference);
                }
                catch (JSException ex)
                {
                    Console.WriteLine("[SayehDataGrid] " + ex.Message);
                }
            }
            if (_checkColumnOptionsPosition && _displayOptionsForColumn is not null)
            {
                _checkColumnOptionsPosition = false;
                _ = Module?.InvokeVoidAsync("checkColumnOptionsPosition", _gridReference, ".col-options").AsTask();
            }
            if (_checkColumnResizePosition && _displayResizeForColumn is not null)
            {
                _checkColumnResizePosition = false;
                _ = Module?.InvokeVoidAsync("checkColumnPopupPosition", _gridReference, ".col-resize").AsTask();
            }

            if (AutoFit && _gridReference is not null)
            {
                _ = Module?.InvokeVoidAsync("autoFitGridColumns", _gridReference, _columns.Count).AsTask();
            }
        }

        private void StartCollectingColumns()
        {
            _columns.Clear();
            _sortableColumns.Clear();
            _filterableColumns.Clear();
            _collectingColumns = true;
            _internalGridContext.SelectColumn = null;
            if (_rowHeader is not null)
                AddColumn(_rowHeader);
        }

        private void FinishCollectingColumns()
        {
            _collectingColumns = false;
            _manualGrid = _columns.Count == 0;
            if (!string.IsNullOrWhiteSpace(GridTemplateColumns) && _columns.Any(x => !string.IsNullOrWhiteSpace(x.Width)))
            {
                throw new Exception("You can use either the 'GridTemplateColumns' parameter on the grid or the 'Width' property at the column level, not both.");
            }

            if (string.IsNullOrWhiteSpace(_internalGridTemplateColumns) && _columns.Any(x => !string.IsNullOrWhiteSpace(x.Width)))
            {
                _internalGridTemplateColumns = string.Join(" ", _columns.Select(x => x.Width ?? "1fr"));
            }

            if (ResizableColumns)
            {
                _ = Module?.InvokeVoidAsync("enableColumnResizing", _gridReference).AsTask();
            }
            // Always re-evaluate after collecting columns when using displaymode grid. A column might be added or hidden and the _internalGridTemplateColumns needs to reflect that.
            if (DisplayMode == DataGridDisplayMode.Grid)
            {
                if (!AutoFit)
                {
                    _internalGridTemplateColumns = GridTemplateColumns ?? string.Join(" ", Enumerable.Repeat("1fr", _columns.Count));
                }

                if (_columns.Any(x => !string.IsNullOrWhiteSpace(x.Width)))
                {
                    _internalGridTemplateColumns = GridTemplateColumns ?? string.Join(" ", _columns.Select(x => x.Width ?? "auto"));
                }
            }

            if (ResizableColumns)
            {
                _ = Module?.InvokeVoidAsync("enableColumnResizing", _gridReference).AsTask();
            }
        }

        // Invoked by descendant columns at a special time during rendering
        internal void AddColumn(SayehColumnBase<TItem> column)
        {
            if (_collectingColumns)
            {
                _columns.Add(column);
                if (column.SortingIsEnable())
                {
                    _sortableColumns.Add(column);
                    if (column.DefaultSort.HasValue && !column.DefaultSortApplied)
                    {
                        column.DefaultSortApplied = true;
                        ISortableColumn<TItem> ccol = (column as ISortableColumn<TItem>)!;
                        if (!ccol.SortDirection.HasValue)
                        {
                            ccol.SortOrder = _sortableColumns.Any() ? (short)(_sortableColumns.Max(m => ((ISortableColumn<TItem>)m).SortOrder) + 1) : (short)1;
                            ccol.SortDirection = column.DefaultSort.Value;
                        }
                    }
                }
                if (column.FilterIsEnable())
                    _filterableColumns.Add(column);
                if (column is SayehSelectColumn<TItem>)
                {
                    if (_internalGridContext.SelectColumn is not null)
                        throw new Exception("each grid instance can have hust one select column");
                    else
                        _internalGridContext.SelectColumn = column as SayehSelectColumn<TItem>;
                }
            }
        }

        #region Column Optopns

        /// <summary>
        /// Displays the <see cref="ColumnBase{TGridItem}.ColumnOptions"/> UI for the specified column, closing any other column
        /// options UI that was previously displayed.
        /// </summary>
        /// <param name="column">The column whose options are to be displayed, if any are available.</param>
        public Task<bool> ShowColumnOptionsAsync(SayehColumnBase<TItem> column)
        {
            if (_currentRow is not null && _currentRow.Mode == DataGridItemMode.Edit)
                return Task.FromResult(false);
            _displayOptionsForColumn = column;
            _checkColumnOptionsPosition = true; // Triggers a call to JSRuntime to position the options element, apply autofocus, and any other setup
            return Task.FromResult(true);
        }

        /// <summary>
        /// Displays the <see cref="ColumnBase{TGridItem}.ColumnOptions"/> UI for the specified column <paramref name="title"/> found first,
        /// closing any other column options UI that was previously displayed. If the title is not found, nothing happens.
        /// </summary>
        /// <param name="title">The column title whose options UI is to be displayed.</param>
        /// <returns>A <see cref="Task"/> representing the completion of the operation.</returns>
        public Task ShowColumnOptionsAsync(string title)
        {
            var column = _columns.FirstOrDefault(c => c.Title?.Equals(title, StringComparison.InvariantCultureIgnoreCase) ?? false);
            return (column is not null) ? ShowColumnOptionsAsync(column) : Task.CompletedTask;
        }

        /// <summary>
        /// Displays the <see cref="ColumnBase{TGridItem}.ColumnOptions"/> UI for the specified column <paramref name="index"/>,
        /// closing any other column options UI that was previously displayed. If the index is out of range, nothing happens.
        /// </summary>
        /// <param name="index">The column index whose options UI is to be displayed.</param>
        /// <returns>A <see cref="Task"/> representing the completion of the operation.</returns>
        public Task ShowColumnOptionsAsync(int index)
        {
            return (index >= 0 && index < _columns.Count) ? ShowColumnOptionsAsync(_columns[index]) : Task.CompletedTask;
        }

        internal void CloseColumnOptions()
        {
            if (_displayOptionsForColumn is not null)
            {
                _displayOptionsForColumn.CloseFilter();
                _displayOptionsForColumn = null;
            }
        }

        #endregion

        #region Column Resize

        /// <summary>
        /// Displays the column resize UI for the specified column, closing any other column
        /// resize UI that was previously displayed.
        /// </summary>
        /// <param name="column">The column whose resize UI is to be displayed.</param>
        /// <returns>A <see cref="Task"/> representing the completion of the operation.</returns>
        public Task ShowColumnResizeAsync(SayehColumnBase<TItem> column)
        {
            _displayResizeForColumn = column;
            _checkColumnResizePosition = true; // Triggers a call to JSRuntime to position the options element, apply autofocus, and any other setup
            StateHasChanged();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Displays the column resize UI for the specified column, closing any other column
        /// resize UI that was previously displayed.
        /// </summary>
        /// <param name="title">The column title whose resize UI is to be displayed.</param>
        /// <returns>A <see cref="Task"/> representing the completion of the operation.</returns>
        public Task ShowColumnResizeAsync(string title)
        {
            var column = _columns.FirstOrDefault(c => c.Title?.Equals(title, StringComparison.InvariantCultureIgnoreCase) ?? false);
            return (column is not null) ? ShowColumnResizeAsync(column) : Task.CompletedTask;
        }

        /// <summary>
        /// Displays the column resize UI for the specified column, closing any other column
        /// resize UI that was previously displayed.
        /// </summary>
        /// <param name="index">The column index whose resize UI is to be displayed.</param>
        /// <returns>A <see cref="Task"/> representing the completion of the operation.</returns>
        public Task ShowColumnResizeAsync(int index)
        {
            return (index >= 0 && index < _columns.Count) ? ShowColumnResizeAsync(_columns[index]) : Task.CompletedTask;
        }

        #endregion

        public void SetLoadingState(bool loading)
        {
            Loading = loading;
        }

        #region Data source

        /// <summary>
        /// Instructs the grid to re-fetch and render the current data from the supplied data source
        /// (either <see cref="Items"/> or <see cref="ItemsProvider"/>).
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the completion of the operation.</returns>
        public async Task RefreshDataAsync()
        {
            await RefreshDataCoreAsync();
        }

        // Same as RefreshDataAsync, except without forcing a re-render. We use this from OnParametersSetAsync
        // because in that case there's going to be a re-render anyway.
        private async Task RefreshDataCoreAsync()
        {
            // Move into a "loading" state, cancelling any earlier-but-still-pending load
            _pendingDataLoadCancellationTokenSource?.Cancel();
            var thisLoadCts = _pendingDataLoadCancellationTokenSource = new CancellationTokenSource();
            if (_virtualizeComponent is not null)
            {
                // If we're using Virtualize, we have to go through its RefreshDataAsync API otherwise:
                // (1) It won't know to update its own internal state if the provider output has changed
                // (2) We won't know what slice of data to query for
                await _virtualizeComponent.RefreshDataAsync();
                _pendingDataLoadCancellationTokenSource = null;
            }

            // If we're not using Virtualize, we build and execute a request against the items provider directly
            var startIndex = Pagination is null ? 0 : (Pagination.CurrentPageIndex * Pagination.ItemsPerPage);
            GridItemsProviderRequest<TItem> request = new (startIndex, Pagination?.ItemsPerPage, _sortableColumns, _filterableColumns, thisLoadCts.Token);
            _lastRefreshedPaginationState = Pagination;
            var result = await ResolveItemsRequestAsync(request);
            if (!thisLoadCts.IsCancellationRequested)
            {
                _internalGridContext.Items = result.Items;
                _internalGridContext.TotalItemCount = result.TotalItemCount;
                Pagination?.SetTotalItemCountAsync(_internalGridContext.TotalItemCount);
                _pendingDataLoadCancellationTokenSource = null;
            }
            _internalGridContext.ResetRowIndexes(startIndex);
            //await InvokeAsync(StateHasChanged);
            //RowsPart?.ReRender();
        }

        async ValueTask<ItemsProviderResult<(int, TItem)>> ProvideVirtualizedItemsAsync(ItemsProviderRequest request)
        {
            _lastRefreshedPaginationState = Pagination;
            await Task.Delay(100);
            if (request.CancellationToken.IsCancellationRequested)
            {
                return default;
            }

            // Combine the query parameters from Virtualize with the ones from PaginationState
            var startIndex = request.StartIndex;
            var count = request.Count;
            if (Pagination is not null)
            {
                startIndex += Pagination.CurrentPageIndex * Pagination.ItemsPerPage;
                count = Math.Min(request.Count, Pagination.ItemsPerPage - request.StartIndex);
            }

            var providerRequest = new GridItemsProviderRequest<TItem>(startIndex, count, _sortableColumns, _filterableColumns, request.CancellationToken);
            var providerResult = await ResolveItemsRequestAsync(providerRequest);
            if (!request.CancellationToken.IsCancellationRequested)
            {
                {
                    // ARIA's rowcount is part of the UI, so it should reflect what the human user regards as the number of rows in the table,
                    // not the number of physical <tr> elements. For virtualization this means what's in the entire scrollable range, not just
                    // the current viewport. In the case where you're also paginating then it means what's conceptually on the current page.
                    // TODO: This currently assumes we always want to expand the last page to have ItemsPerPage rows, but the experience might
                    //       be better if we let the last page only be as big as its number of actual rows.
                    _internalGridContext.TotalItemCount = providerResult.TotalItemCount;
                    _internalGridContext.TotalViewItemCount = Pagination?.ItemsPerPage ?? providerResult.TotalItemCount;
                    Pagination?.SetTotalItemCountAsync(providerResult.TotalItemCount);

                    if ((_internalGridContext.TotalItemCount > 0 && Loading != false))
                    {
                        Loading = false;
                        //this.RowsPart?.ReRender();
                    }

                    // We're supplying the row _index along with each row's data because we need it for aria-rowindex, and we have to account for
                    // the virtualized start _index. It might be more performant just to have some _latestQueryRowStartIndex field, but we'd have
                    // to make sure it doesn't get out of sync with the rows being rendered.
                    return new ItemsProviderResult<(int, TItem)>(
                         items: providerResult.Items.Select((x, i) => ValueTuple.Create(i + request.StartIndex + 2, x)),
                         totalItemCount: _internalGridContext.TotalViewItemCount);

                }
            }
            return default;
        }

        // Normalizes all the different ways of configuring a data source so they have common GridItemsProvider-shaped API
        async ValueTask<GridItemsProviderResult<TItem>> ResolveItemsRequestAsync(GridItemsProviderRequest<TItem> request)
        {
            if (ItemsProvider is not null)
            {
                var gipr = await ItemsProvider(request);
                if (gipr.Items is not null)
                {
                    Loading = false;
                }
                return gipr;
            }
            else if (Items is not null)
            {
                var totalItemCount = Items.Count();
                _internalGridContext.TotalItemCount = totalItemCount;
                var result = request.ApplyFilterAndSorting(Items)?.Skip(request.StartIndex);
                return GridItemsProviderResult.From([.. result], totalItemCount);
            }
            else
            {
                return GridItemsProviderResult.From(Array.Empty<TItem>(), 0);
            }
        }

        #endregion

        #region Column Resize

        /// <summary>
        /// Resizes the column width by a discrete amount.
        /// </summary>
        /// <param name="columnIndex">The column to be resized</param>
        /// <param name="widthChange">The amount of pixels to change width with</param>
        /// <returns></returns>
        public async Task SetColumnWidthDiscreteAsync(int? columnIndex, float widthChange)
        {
            if (_gridReference is not null && Module is not null)
            {
                await Module.InvokeVoidAsync("resizeColumnDiscrete", _gridReference, columnIndex, widthChange);
            }
        }

        /// <summary>
        /// Resizes the column width to the exact width specified (in pixels).
        /// </summary>
        /// <param name="columnIndex">The column to be resized</param>
        /// <param name="width">The new width in pixels</param>
        /// <returns></returns>
        public async Task SetColumnWidthExactAsync(int columnIndex, int width)
        {
            if (_gridReference is not null && Module is not null)
            {
                await Module.InvokeVoidAsync("resizeColumnExact", _gridReference, columnIndex, width);
            }
        }

        #endregion


        /// <summary>
        /// apply sort to the specified <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column that defines the new sort order.</param>
        /// <returns>A <see cref="Task"/> representing the completion of the operation.</returns>
        internal Task ApplySort(ISortableColumn<TItem> column)
        {
            if (_currentRow is not null && _currentRow.Mode == DataGridItemMode.Edit)
                return Task.CompletedTask;
            var affectedColumns = _sortableColumns.Cast<ISortableColumn<TItem>>().Where(w => w.SortDirection.HasValue);
            multiColumnSorted = affectedColumns.Count() > 1;
            if (column.SortDirection.HasValue)
            {
                if (multiColumnSorted)
                {
                    column.SortOrder = (short)(affectedColumns.Count() + 1);
                    rearrangeSortableColumns(affectedColumns);
                }
                else
                    column.SortOrder = 1;
            }
            else
                rearrangeSortableColumns(affectedColumns);
            return RefreshDataAsync();
        }

        /// <summary>
        /// recalculate filters
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        internal Task ApplyFilter(IFilterableColumn<TItem> column)
        {
            return RefreshDataAsync();
        }

        private void rearrangeSortableColumns(IEnumerable<ISortableColumn<TItem>> affectedColumns)
        {
            short order = 1;
            foreach (var col in affectedColumns.OrderBy(o => o.SortOrder).ToList())
            {
                col.SortOrder = order;
                order++;
            }
        }

        private string AriaSortValue(SayehColumnBase<TItem> column)
        {
            ListSortDirection? sortDirection = null;
            if (_sortableColumns.Any(a => a == column && ((ISortableColumn<TItem>)a).SortDirection.HasValue))
            {
                sortDirection = ((ISortableColumn<TItem>)column).SortDirection!.Value;
            }
            return sortDirection.HasValue ? (sortDirection.Value == ListSortDirection.Ascending ? "ascending" : "descending") : "none";
        }

        private string? StyleValue => new StyleBuilder(Style)
       .AddStyle("grid-template-columns", _internalGridTemplateColumns, !string.IsNullOrWhiteSpace(_internalGridTemplateColumns) && DisplayMode == DataGridDisplayMode.Grid)
       //.AddStyle("grid-template-rows", "auto 1fr", (_internalGridContext.TotalItemCount == 0 || Items is null) && DisplayMode == DataGridDisplayMode.Grid)
       //.AddStyle("height", "100%", _internalGridContext.TotalItemCount == 0)
       .AddStyle("border-collapse", "separate", GenerateHeader == GenerateHeaderOption.Sticky)
       .AddStyle("border-spacing", "0", GenerateHeader == GenerateHeaderOption.Sticky)
       .AddStyle("width", "100%", DisplayMode == DataGridDisplayMode.Table)
       .Build();


        //private string? columnheaderclass(sayehcolumnbase<titem> column)
        //{
        //    listsortdirection? sortdirection = null;
        //    if (_sortablecolumns.any(a => a == column && ((isortablecolumn<titem>)a).sortdirection.hasvalue))
        //    {
        //        sortdirection = ((isortablecolumn<titem>)column).sortdirection!.value;
        //    }
        //    return sortdirection.hasvalue
        //   ? $"{columnclass(column)} {(sortdirection.value == listsortdirection.ascending ? "col-sort-asc" : "col-sort-desc")}"
        //   : columnclass(column);
        //}

        private string? ColumnHeaderClass(SayehColumnBase<TItem> column)
        {
            ListSortDirection? sortDirection = null;
            if (_sortableColumns.Any(a => a == column && ((ISortableColumn<TItem>)a).SortDirection.HasValue))
            {
                sortDirection = ((ISortableColumn<TItem>)column).SortDirection!.Value;
            }
            return new CssBuilder(column.Class)
               .AddClass(ColumnJustifyClass(column))
               .AddClass("col-sort-asc", sortDirection.HasValue && sortDirection.Value == ListSortDirection.Ascending)
               .AddClass("col-sort-desc", sortDirection.HasValue && sortDirection.Value == ListSortDirection.Descending)
               .Build();
        }

        private static string? ColumnJustifyClass(SayehColumnBase<TItem> column)
        {
            return new CssBuilder(column.Class)
                .AddClass("col-justify-start", column.Align == Align.Start)
                .AddClass("col-justify-center", column.Align == Align.Center)
                .AddClass("col-justify-end", column.Align == Align.End)
                .Build();
        }


        private string? GridClass()
        {
            return new CssBuilder(Class)
                .AddClass("fluent-data-grid")
                .AddClass("grid", DisplayMode == DataGridDisplayMode.Grid)
                .AddClass("auto-fit", AutoFit)
                .AddClass("loading", _pendingDataLoadCancellationTokenSource is not null)
                .Build();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            _currentPageItemsChanged.Dispose();

            try
            {
                if (_jsEventDisposable is not null)
                {
                    await _jsEventDisposable.InvokeVoidAsync("stop");
                    await _jsEventDisposable.DisposeAsync();
                }

                if (Module is not null)
                {
                    await Module.DisposeAsync();
                }
            }
            catch (Exception ex) when (ex is JSDisconnectedException ||
                                  ex is OperationCanceledException)
            {
                // The JSRuntime side may routinely be gone already if the reason we're disposing is that
                // the client disconnected. This is not an error.
            }
        }

        internal async Task OnRowFocusAsync(SayehDataGridRow<TItem> row)
        {
            await OnRowFocus.InvokeAsync(row);
            if (_currentRow is null || IsReadonly)
            {
                SetCurrentRow(row);
                return;
            }
            if (_currentRow.RowId.Equals(row.RowId))
                return;
            if (_currentRow.Mode == DataGridItemMode.Edit)
            {
                var canCommit = await EndEdit(_currentRow, EditActionEnum.Commit);
                if (canCommit)
                    SetCurrentRow(row);
            }
            else
                SetCurrentRow(row);
        }

        private async void SetCurrentRow(SayehDataGridRow<TItem> row)
        {
            if (row.Item is not null)
                SelectedItem = row.Item;
            else
                SelectedItem = null;
            if (_currentRow is not null)
                _currentRow.ReRenderHeaderRow();
            _currentRow = row;
            _currentRow.ReRenderHeaderRow();
            await OnRowFocus.InvokeAsync(row);
        }

        public async Task OnKeyDownAsync(FluentKeyCodeEventArgs args)
        {
            if (args.ShiftKey == true && args.Key == KeyCode.KeyR)
            {
                await ResetColumnWidthsAsync();
            }

            if (args.Value == "-")
            {
                await SetColumnWidthAsync(-10);
            }
            if (args.Value == "+")
            {
                //  Resize column up
                await SetColumnWidthAsync(10);
            }
            OnKeyPress(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs() { Key = args.Value });
            //return Task.CompletedTask;
        }

        private async Task SetColumnWidthAsync(float widthChange)
        {
            if (_gridReference is not null && Module is not null)
            {
                await Module.InvokeVoidAsync("resizeColumn", _gridReference, widthChange);
            }
        }

        internal async Task ResetColumnWidthsAsync()
        {
            if (_gridReference is not null && Module is not null)
            {
                await Module.InvokeVoidAsync("resetColumnWidths", _gridReference);
            }
        }

        Task IHandleEvent.HandleEventAsync(
         EventCallbackWorkItem callback, object? arg) => callback.InvokeAsync(arg);

        #endregion

    }
}
