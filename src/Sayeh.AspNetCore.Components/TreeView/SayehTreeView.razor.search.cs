using Microsoft.FluentUI.AspNetCore.Components;
using Sayeh.AspNetCore.Components.Infrastructure;
using System.Threading.Tasks;

namespace Sayeh.AspNetCore.Components;

partial class SayehTreeView<TItem>
{
    [Parameter]
    public Func<TItem, string, bool> Filter { get; set; }

    string _searchResult { get; set; }
    List<SayehTreeViewItem<TItem>> _findedNodes;
    List<TItem> _findedItems;
    int _filterPointer = 0;

    async Task DoSearch(string text)
    {
        _findedNodes = new List<SayehTreeViewItem<TItem>>();
        _findedItems = new List<TItem>();
        if (Filter is null)
            Filter = (f, txt) => f.ToString()?.Replace(" ", "").Trim().Contains(txt) ?? false;
        if (text.Trim().None())
        {
            _searchResult = string.Empty;
            return;
        }
        _searchResult = Resources.SearchingMessage;
        if (Children is null)
            await DoSearchByNodeAsync(text);
        else
            DoSearchByData(text);

        _filterPointer = 0;
        if (_findedItems.Count > 0)
        {
            _searchResult = string.Format(Resources.FoundedItemCount, _findedItems.Count);
            SelectedItem = _findedItems.First();
            await DisplaySelectedItem(SelectedItem);
            if (SelectedItemChanged.HasDelegate)
                await SelectedItemChanged.InvokeAsync(SelectedItem);
        }
        else if (_findedNodes.Count > 0)
        {
            _searchResult = string.Format(Resources.FoundedItemCount, _findedNodes.Count);
            SetSelectedNode(_findedNodes.First());
            if (SelectedItem != _findedItems.First()) { 
                SelectedItem = _findedItems.First();
                if (SelectedItemChanged.HasDelegate)
                    await SelectedItemChanged.InvokeAsync(SelectedItem);
            }
        }

    }

    #region Search by nodes

    async ValueTask DoSearchByNodeAsync(string text)
    {
        foreach (var node in _allItems.Where(w => w.Value.Parent is null).ToList())
        {
            await searchInNode(node.Value, text);
        }
    }

    async ValueTask searchInChildNodes(SayehTreeViewItem<TItem> node, string text)
    {
        foreach (var child in node._children)
        {
            await searchInNode(child.Value, text);
        }
    }

    async ValueTask searchInNode(SayehTreeViewItem<TItem> node, string text)
    {
        if (node.Item is null)
            return;
        await InvokeAsync(() => _searchResult = Resources.SearchingMessage + " - " + (node.Text is not null ? node.Text(node.Item) : node.Item.ToString()));
        if (Filter.Invoke(node.Item, text))
            _findedNodes.Add(node);
        var expanded = node.Expanded;
        //waiting to childs rendered
        if (Virtualize && !expanded)
        {
            await Task.Delay(100);
            node.SetExpanded(true);
        }
        await searchInChildNodes(node, text);
        node.SetExpanded(expanded);
    }

    #endregion

    #region Search by items

    void DoSearchByData(string text)
    {
        foreach (var node in _allItems.Where(w => w.Value.Parent is null).ToList())
        {
            if (node.Value.Item is null)
                continue;
            searchInItem(node.Value.Item, text);
        }
    }

    void searchInChildItems(TItem item, string text)
    {
        var children = Children is not null ? Children(item) : null;
        if (children is not null)
        {
            foreach (var child in children)
            {
                searchInItem(child, text);
            }
        }
    }

    void searchInItem(TItem item, string text)
    {
        InvokeAsync(() => _searchResult = Resources.SearchingMessage + " - " + (Text is not null ? Text(item) : item.ToString()));
        if (Filter.Invoke(item, text))
            _findedItems.Add(item);
        searchInChildItems(item, text);
    }

    #endregion

    #region Select next

    void selectNext()
    {
        if (_findedItems.Any())
            selectNextItem();
        else
            selectNextNode();
    }

    void selectNextNode()
    {
        if (_filterPointer < (_findedNodes.Count - 1))
        {
            _filterPointer++;
            var item = _findedNodes[_filterPointer];
            SetSelectedNode(item);
        }
    }

    void selectNextItem()
    {
        if (_filterPointer < (_findedItems.Count - 1))
        {
            _filterPointer++;
            var item = _findedItems[_filterPointer];
            SelectedItem = item;
        }
    }

    #endregion

    #region Select previous


    void selectPrevious()
    {
        if (_findedItems.Any())
            selectPreviousItem();
        else
            selectPreviousNode();
    }


    void selectPreviousNode()
    {
        if (_filterPointer > 0)
        {
            _filterPointer--;
            var item = _findedNodes[_filterPointer];
            SetSelectedNode(item);
        }
    }

    void selectPreviousItem()
    {
        if (_filterPointer > 0)
        {
            _filterPointer--;
            var item = _findedItems[_filterPointer];
            SelectedItem = item;
        }
    }

    #endregion

}
