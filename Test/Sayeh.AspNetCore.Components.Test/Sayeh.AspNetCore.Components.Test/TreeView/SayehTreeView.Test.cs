using Microsoft.AspNetCore.Components.Web.Virtualization;
using Sample.Client.Model;
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
namespace Sayeh.AspNetCore.Components.Test;

[TestClass]
public class SayehTreeViewTest : TestBase
{

    #region Initialize state of control

    [TestMethod("Initialize : set selectedItem (via search in Data)")]
    [Description("Peek an Item from source of control, then set it as SelectedItem, then ensure the control can find its node and make it Selected and all its parent should be expanded")]
    [DataRow(false)]
    [DataRow(true)]
    public async Task SetSelectedItemOnInitializeTest(bool virtualize)
    {
        var items = HierarchycalItem.GetHierarchycalItems(5, 4);

        var firstLevelItem = items.Skip(Random.Shared.Next(0, items.Count() - 1)).First().Children;
        var secondLevelItem = firstLevelItem.Skip(Random.Shared.Next(0, firstLevelItem.Count() - 1)).First().Children;
        var thirdLevelItem = secondLevelItem.Skip(Random.Shared.Next(0, secondLevelItem.Count() - 1)).First().Children;
        var forthLevelItem = thirdLevelItem.Skip(Random.Shared.Next(0, thirdLevelItem.Count() - 1)).First();

        Assert.IsNotNull(items);

        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
        .Add(p => p.Items, items)
        .Add(p => p.Children, x => x.Children)
        .Add(p => p.Parent, x => x.Parent)
        .Add(p => p.DisplayMember, x => x.Name)
        .Add(p => p.SelectedItem, forthLevelItem)
        .Add(p => p.Virtualize, virtualize)
        );

        var pov = new PrivateObject(cut.Instance);
        int waitCount = 0;
        while (pov.GetField("_selectedNode") == null && waitCount < 6)
        {
            waitCount++;
            await Task.Delay(100);
        }

        Assert.AreEqual(forthLevelItem, cut.Instance.SelectedItem);
        var selectedNode = pov.GetField("_selectedNode");
        Assert.IsNotNull(selectedNode);
        Assert.IsTrue(((SayehTreeViewItem<HierarchycalItem>)selectedNode).Selected);
        Assert.AreEqual(forthLevelItem, ((SayehTreeViewItem<HierarchycalItem>)selectedNode).Item);

    }

    [TestMethod("Initialize : set selectedItem (via search in Nodes)")]
    [Description("Peek an Item from source of control, then set it as SelectedItem, then ensure the control can find its node via nodes (without ParentItem property) and make it Selected and all its parent should be expanded")]
    [DataRow(false)]
    //[DataRow(true)]
    public async Task SetSelectedItemOnInitializeByNodesTest(bool virtualize)
    {
        var items = HierarchycalItem.GetHierarchycalItems(5, 4);

        var firstLevelItem = items.Skip(Random.Shared.Next(0, items.Count() - 1)).First().Children;
        var secondLevelItem = firstLevelItem.Skip(Random.Shared.Next(0, firstLevelItem.Count() - 1)).First().Children;
        var thirdLevelItem = secondLevelItem.Skip(Random.Shared.Next(0, secondLevelItem.Count() - 1)).First().Children;
        var forthLevelItem = thirdLevelItem.Skip(Random.Shared.Next(0, thirdLevelItem.Count() - 1)).First();

        Assert.IsNotNull(items);

        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
        .Add(p => p.Items, items)
        .Add(p => p.Children, x => x.Children)
        .Add(p => p.DisplayMember, x => x.Name)
        .Add(p => p.SelectedItem, forthLevelItem)
        .Add(p => p.Virtualize, virtualize)
        );

        var pov = new PrivateObject(cut.Instance);
        int waitCount = 0;
        while (pov.GetField("_selectedNode") == null && waitCount < 6)
        {
            waitCount++;
            await Task.Delay(100);
        }

        Assert.AreEqual(forthLevelItem, cut.Instance.SelectedItem);
        var selectedNode = pov.GetField("_selectedNode");
        Assert.IsNotNull(selectedNode);
        Assert.IsTrue(((SayehTreeViewItem<HierarchycalItem>)selectedNode).Selected);
        Assert.AreEqual(forthLevelItem, ((SayehTreeViewItem<HierarchycalItem>)selectedNode).Item);

    }

    [TestMethod("Initialize : set selectedItem (via search in Nodes) when ItemTemplate is setted")]
    [Description("Peek an Item from source of control, then set it as SelectedItem, then ensure the control can find its node via nodes (without ParentItem property) and make it Selected and all its parent should be expanded")]
    [DataRow(false)]
    //[DataRow(true)]
    public async Task SetSelectedItemOnInitializeByNodes_ItemTempateTest(bool virtualize)
    {
        var items = WeatherForecast.GenerateGroupedItems();

        var firstLevelItem = items.Skip(Random.Shared.Next(0, items.Count() - 1)).First().Items;
        var secondLevelItem = firstLevelItem.Skip(Random.Shared.Next(0, firstLevelItem.Count() - 1)).First();

        Assert.IsNotNull(items);

        RenderFragment<object> firstLevelItemtemplate = context => __builder =>
        {
            var item = (WeatherForecastGroup)context;
            __builder.AddContent(0, new MarkupString($"<span class=\"treeitem-text\">@{item!.Name} - ({item.Items.Count()})</span>"));
        };
        RenderFragment<object> secondLevelItemtemplate = context => __builder =>
        {
            var item = (WeatherForecast)context;
            __builder.AddContent(0, new MarkupString($"<span class=\"treeitem-text\">{item!.Date} - {item!.Tempreture}</span>"));
        };
        RenderFragment<object> childrenItemtemplate = context => __builder =>
        {
            var group = (WeatherForecastGroup)context;
            var index = 0;
            foreach (var item in group.Items)
            {
                __builder.OpenComponent<SayehTreeViewItem<object>>(index++);
                __builder.AddComponentParameter(index++, nameof(SayehTreeViewItem<object>.Item), item);
                __builder.AddComponentParameter(index++, nameof(SayehTreeViewItem<object>.Parent), (Func<object, object?>)(f => ((WeatherForecast)f).Group));
                __builder.AddComponentParameter(index++, nameof(SayehTreeViewItem<object>.InitiallySelected), item == secondLevelItem);
                __builder.AddComponentParameter(index++, nameof(SayehTreeViewItem<object>.Template), secondLevelItemtemplate);
                __builder.CloseComponent();
            }
            
        };
        RenderFragment<object> treeItemtemplate = context => __builder =>
        {
            var item = (WeatherForecastGroup)context;
            __builder.OpenComponent<SayehTreeViewItem<object>>(0);
            __builder.AddComponentParameter(1, nameof(SayehTreeViewItem<object>.Item), item);
            __builder.AddComponentParameter(2, nameof(SayehTreeViewItem<object>.Children), (Func<object, IEnumerable<object>>)(f => ((WeatherForecastGroup)f).Items.Cast<object>()));
            __builder.AddComponentParameter(4, nameof(SayehTreeViewItem<object>.Template), firstLevelItemtemplate);
            __builder.AddComponentParameter(5, nameof(SayehTreeViewItem<object>.ChildContent), childrenItemtemplate);
            __builder.CloseComponent();
        };

        var cut = Render<SayehTreeView<object>>(parameters => parameters
        .Add(p => p.Items, items)
        .Add(p => p.SelectedItem, secondLevelItem)
        .Add(p => p.Virtualize, virtualize)
        .Add(p => p.ItemTemplate, treeItemtemplate)
        );

        var pov = new PrivateObject(cut.Instance);
        int waitCount = 0;
        while (pov.GetField("_selectedNode") == null && waitCount < 6)
        {
            waitCount++;
            await Task.Delay(100);
        }

        Assert.AreEqual(secondLevelItem, cut.Instance.SelectedItem);
        var selectedNode = pov.GetField("_selectedNode");
        Assert.IsNotNull(selectedNode);
        Assert.IsTrue(((SayehTreeViewItem<object>)selectedNode).Selected);
        Assert.AreEqual(secondLevelItem, ((SayehTreeViewItem<object>)selectedNode).Item);

    }

    #endregion

    #region Item Template

    [TestMethod("Template : renders Template instead of Item.ToString() when no DisplayMember is set")]
    public void TreeViewItem_RendersTemplate_NotToString()
    {
        var items = HierarchycalItem.GetHierarchycalItems(3, 1);
        var firstItem = items.First();

        RenderFragment<HierarchycalItem> itemTemplate = item => builder =>
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", "custom-template");
            builder.AddContent(2, $"TPL:{item.Name}");
            builder.CloseElement();
        };

        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
        .Add(p => p.Items, items)
        .Add(p => p.Children, x => x.Children)
        .Add(p => p.Parent, x => x.Parent)
        .Add(p => p.ItemTemplate, itemTemplate)
        );

        Assert.IsTrue(cut.Markup.Contains($"TPL:{firstItem.Name}"), "Template content should be rendered");
        Assert.IsFalse(cut.Markup.Contains(firstItem.ID.ToString()), "Item.ToString() should not be rendered when Template is set");
    }

    [TestMethod("Template : renders Template instead of DisplayMember when both are set")]
    public void TreeViewItem_RendersTemplate_NotDisplayMember()
    {
        var items = HierarchycalItem.GetHierarchycalItems(3, 1);
        var firstItem = items.First();

        RenderFragment<HierarchycalItem> itemTemplate = item => builder =>
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", "custom-template");
            builder.AddContent(2, $"TPL:{item.Name}");
            builder.CloseElement();
        };

        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
        .Add(p => p.Items, items)
        .Add(p => p.Children, x => x.Children)
        .Add(p => p.Parent, x => x.Parent)
        .Add(p => p.DisplayMember, x => $"DISPLAYMEMBER:{x.Name}")
        .Add(p => p.ItemTemplate, itemTemplate)
        );

        Assert.IsTrue(cut.Markup.Contains($"TPL:{firstItem.Name}"), "Template content should be rendered");
        Assert.IsFalse(cut.Markup.Contains($"DISPLAYMEMBER:{firstItem.Name}"), "DisplayMember should not be rendered when Template is set");
    }

    [TestMethod("Template : checkbox item renders Template instead of Item.ToString() when no DisplayMember is set")]
    public void TreeViewCheckboxItem_RendersTemplate_NotToString()
    {
        var items = HierarchycalItem.GetHierarchycalItems(3, 1);
        var firstItem = items.First();

        RenderFragment<HierarchycalItem> itemTemplate = item => builder =>
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", "custom-template");
            builder.AddContent(2, $"TPL:{item.Name}");
            builder.CloseElement();
        };

        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
        .Add(p => p.Items, items)
        .Add(p => p.Children, x => x.Children)
        .Add(p => p.Parent, x => x.Parent)
        .Add(p => p.SelectProperty, s => s.IsSelected)
        .Add(p => p.ItemTemplate, itemTemplate)
        );

        Assert.IsTrue(cut.Markup.Contains($"TPL:{firstItem.Name}"), "Template content should be rendered");
        Assert.IsFalse(cut.Markup.Contains(firstItem.ID.ToString()), "Item.ToString() should not be rendered when Template is set");
    }

    [TestMethod("Template : checkbox item renders Template instead of DisplayMember when both are set")]
    public void TreeViewCheckboxItem_RendersTemplate_NotDisplayMember()
    {
        var items = HierarchycalItem.GetHierarchycalItems(3, 1);
        var firstItem = items.First();

        RenderFragment<HierarchycalItem> itemTemplate = item => builder =>
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", "custom-template");
            builder.AddContent(2, $"TPL:{item.Name}");
            builder.CloseElement();
        };

        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
        .Add(p => p.Items, items)
        .Add(p => p.Children, x => x.Children)
        .Add(p => p.Parent, x => x.Parent)
        .Add(p => p.SelectProperty, s => s.IsSelected)
        .Add(p => p.DisplayMember, x => $"DISPLAYMEMBER:{x.Name}")
        .Add(p => p.ItemTemplate, itemTemplate)
        );

        Assert.IsTrue(cut.Markup.Contains($"TPL:{firstItem.Name}"), "Template content should be rendered");
        Assert.IsFalse(cut.Markup.Contains($"DISPLAYMEMBER:{firstItem.Name}"), "DisplayMember should not be rendered when Template is set");
    }

    [TestMethod("Template : checkbox item renders Template instead of Item.ToString() for every node, including leaves, with a conditional/nested-component template")]
    public void TreeViewCheckboxItem_RendersTemplate_NotToString_AllLevels()
    {
        var items = HierarchycalItem.GetHierarchycalItems(3, 2);
        var allItems = Flatten(items).ToList();

        RenderFragment<HierarchycalItem> itemTemplate = item => builder =>
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", "custom-template");
            builder.AddContent(2, $"TPL:{item.Name}");
            builder.CloseElement();
            if (item.Children.Count == 0)
            {
                builder.OpenComponent<SayehButton>(3);
                builder.AddAttribute(4, "ChildContent", (RenderFragment)(b => b.AddContent(0, "leaf-action")));
                builder.CloseComponent();
            }
        };

        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
        .Add(p => p.Items, items)
        .Add(p => p.Children, x => x.Children)
        .Add(p => p.Parent, x => x.Parent)
        .Add(p => p.SelectProperty, s => s.IsSelected)
        .Add(p => p.ItemTemplate, itemTemplate)
        );

        foreach (var item in allItems)
        {
            Assert.IsTrue(cut.Markup.Contains($"TPL:{item.Name}"), $"Template content should be rendered for '{item.Name}'");
            Assert.IsFalse(cut.Markup.Contains(item.ID.ToString()), $"Item.ToString() should not be rendered for '{item.Name}' when Template is set");
        }
    }

    static IEnumerable<HierarchycalItem> Flatten(IEnumerable<HierarchycalItem> items)
    {
        foreach (var item in items)
        {
            yield return item;
            foreach (var child in Flatten(item.Children))
                yield return child;
        }
    }

    #endregion

    #region Search

    [TestMethod("Search : set selectedItem")]
    [Description("Peek an Item from source of control, do search for peeked item, then check the SelectedItem and _selectedNode properties setted")]
    [DataRow(false)]
    [DataRow(true)]
    public async Task Search_SetSelectedItemAfterSearchTest(bool virtualize)
    {
        var items = HierarchycalItem.GetHierarchycalItems(5, 3);
        Assert.IsNotNull(items);

        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
        .Add(p => p.Items, items)
        .Add(p => p.Children, x => x.Children)
        .Add(p => p.Parent, x => x.Parent)
        .Add(p => p.DisplayMember, x => x.Name)
        .Add(p => p.Filter, (x, txt) => x.Name.Contains(txt))
        .Add(p => p.Virtualize, virtualize)
        );

        var firstLevelItem = items.Skip(Random.Shared.Next(0, items.Count() - 1)).First().Children;
        var secondLevelItem = firstLevelItem.Skip(Random.Shared.Next(0, firstLevelItem.Count() - 1)).First().Children;
        var thirdLevelItem = secondLevelItem.Skip(Random.Shared.Next(0, secondLevelItem.Count() - 1)).First();

        var pov = new PrivateObject(cut.Instance);
        await pov.InvokeAsync("DoSearch", thirdLevelItem.Name);

        Assert.AreEqual(thirdLevelItem, cut.Instance.SelectedItem);

        var selectedNode = pov.GetField("_selectedNode");
        Assert.IsNotNull(selectedNode);
        Assert.IsTrue(((SayehTreeViewItem<HierarchycalItem>)selectedNode).Selected);
        Assert.AreEqual(thirdLevelItem, ((SayehTreeViewItem<HierarchycalItem>)selectedNode).Item);

    }

    [TestMethod("Search : selectNext,selectPrevoious functions for single item")]
    [Description("Peek an Item from source of control, do search for peeked item, then Ensure selectPrevoious,selectNext functions should select finded item as SelectedItem")]
    [DataRow("selectNext", true)]
    [DataRow("selectNext", false)]
    [DataRow("selectPrevious", true)]
    [DataRow("selectPrevious", false)]
    public async Task Search_SelectNext_SelectPrevoiousTest(string functionName, bool virtualize)
    {
        var items = HierarchycalItem.GetHierarchycalItems(5, 3);
        Assert.IsNotNull(items);

        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
        .Add(p => p.Items, items)
        .Add(p => p.Children, x => x.Children)
        .Add(p => p.Parent, x => x.Parent)
        .Add(p => p.DisplayMember, x => x.Name)
        .Add(p => p.Filter, (x, txt) => x.Name.Contains(txt))
        .Add(p => p.Virtualize, virtualize)
        );

        var firstLevelItem = items.Skip(Random.Shared.Next(0, items.Count() - 1)).First().Children;
        var secondLevelItem = firstLevelItem.Skip(Random.Shared.Next(0, firstLevelItem.Count() - 1)).First().Children;
        var thirdLevelItem = secondLevelItem.Skip(Random.Shared.Next(0, secondLevelItem.Count() - 1)).First();

        var pov = new PrivateObject(cut.Instance);
        await pov.InvokeAsync("DoSearch", thirdLevelItem.Name);

        Assert.AreEqual(1, ((IList<HierarchycalItem>)pov.GetField("_findedItems")).Count, "filter should find just one item");
        pov.Invoke(functionName);
        Assert.AreEqual(thirdLevelItem, cut.Instance.SelectedItem);

    }

    [TestMethod("Search : selectNext functions for multiple search item")]
    [Description("Peek two Items from source of control, rename those to have a shared part. do search for those items, then Ensure selectNext functions set SelectedItem correctly")]
    [DataRow(true)]
    [DataRow(false)]
    public void Search_SelectNext_ForMultipleSearchResultTest(bool virtualize)
    {
        var items = HierarchycalItem.GetHierarchycalItems(5, 4);
        Assert.IsNotNull(items);

        var firstLevelItem = items.Skip(Random.Shared.Next(0, (items.Count() / 2) - 1)).First().Children;
        var secondLevelItem = firstLevelItem.Skip(Random.Shared.Next(0, (firstLevelItem.Count() / 2) - 1)).First().Children;
        var thirdLevelItem = secondLevelItem.Skip(Random.Shared.Next(0, (secondLevelItem.Count() / 2) - 1)).First().Children;
        var forthLevelItem = thirdLevelItem.Skip(Random.Shared.Next(0, thirdLevelItem.Count() - 1)).First();

        var firstItem = forthLevelItem;

        firstLevelItem = items.Skip(Random.Shared.Next((items.Count() / 2), items.Count() - 1)).First().Children;
        secondLevelItem = firstLevelItem.Skip(Random.Shared.Next((firstLevelItem.Count() / 2), firstLevelItem.Count() - 1)).First().Children;
        thirdLevelItem = secondLevelItem.Skip(Random.Shared.Next((secondLevelItem.Count() / 2), secondLevelItem.Count() - 1)).First().Children;
        forthLevelItem = thirdLevelItem.Skip(Random.Shared.Next(0, thirdLevelItem.Count() - 1)).First();

        var secondItem = forthLevelItem;

        var guid = Guid.NewGuid().ToString();
        firstItem.Name = guid + Random.Shared.Next(0, 10000).ToString();
        secondItem.Name = guid + Random.Shared.Next(10000, 100000000).ToString();

        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
        .Add(p => p.Items, items)
        .Add(p => p.Children, x => x.Children)
        .Add(p => p.Parent, x => x.Parent)
        .Add(p => p.DisplayMember, x => x.Name)
        .Add(p => p.Filter, (x, txt) => x.Name.Contains(txt))
        .Add(p => p.Virtualize, virtualize)
        );

        var pov = new PrivateObject(cut.Instance);
        pov.Invoke("DoSearch", guid);

        Assert.AreEqual(2, ((IList<HierarchycalItem>)pov.GetField("_findedItems")).Count, "filter should find just one item");
        Assert.AreEqual(firstItem, cut.Instance.SelectedItem);
        pov.Invoke("selectNext");
        Assert.AreEqual(secondItem, cut.Instance.SelectedItem);
    }

    [TestMethod("Search : selectPrevious functions for multiple search item")]
    [Description("Peek two Items from source of control, rename those to have a shared part. do search for those items, then Ensure selectPrevious functions set SelectedItem correctly")]
    [DataRow(true)]
    [DataRow(false)]
    public void Search_SelectPrevious_ForMultipleSearchResultTest(bool virtualize)
    {
        var items = HierarchycalItem.GetHierarchycalItems(7, 3);
        Assert.IsNotNull(items);

        var firstLevelItem = items.Skip(Random.Shared.Next(0, (items.Count() / 2) - 1)).First().Children;
        var secondLevelItem = firstLevelItem.Skip(Random.Shared.Next(0, (firstLevelItem.Count() / 2) - 1)).First().Children;
        var thirdLevelItem = secondLevelItem.Skip(Random.Shared.Next(0, (secondLevelItem.Count() / 2) - 1)).First();

        var firstItem = thirdLevelItem;

        firstLevelItem = items.Skip(Random.Shared.Next((items.Count() / 2), items.Count() - 1)).First().Children;
        secondLevelItem = firstLevelItem.Skip(Random.Shared.Next((firstLevelItem.Count() / 2), firstLevelItem.Count() - 1)).First().Children;
        thirdLevelItem = secondLevelItem.Skip(Random.Shared.Next((secondLevelItem.Count() / 2), secondLevelItem.Count() - 1)).First();

        var secondItem = thirdLevelItem;

        var guid = Guid.NewGuid().ToString();
        firstItem.Name = guid + Random.Shared.Next(0, 10000).ToString();
        secondItem.Name = guid + Random.Shared.Next(10000, 100000000).ToString();

        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
        .Add(p => p.Items, items)
        .Add(p => p.Children, x => x.Children)
        .Add(p => p.Parent, x => x.Parent)
        .Add(p => p.DisplayMember, x => x.Name)
        .Add(p => p.Filter, (x, txt) => x.Name.Contains(txt))
        .Add(p => p.Virtualize, virtualize)
        );

        var pov = new PrivateObject(cut.Instance);
        pov.Invoke("DoSearch", guid);

        Assert.AreEqual(2, ((IList<HierarchycalItem>)pov.GetField("_findedItems")).Count, "filter should find just one item");
        Assert.AreEqual(firstItem, cut.Instance.SelectedItem);
        pov.Invoke("selectNext");
        Assert.AreEqual(secondItem, cut.Instance.SelectedItem);
        pov.Invoke("selectPrevious");
        Assert.AreEqual(firstItem, cut.Instance.SelectedItem);

    }

    #endregion

    #region Collection changes

    static ObservableCollection<HierarchycalItem> GetObservableItems(int count, int childCount)
    {
        var roots = new ObservableCollection<HierarchycalItem>();
        for (var i = 0; i < count; i++)
        {
            var root = new HierarchycalItem($"root-{i}", null) { Children = new ObservableCollection<HierarchycalItem>() };
            for (var j = 0; j < childCount; j++)
                root.Children.Add(new HierarchycalItem($"child-{i}-{j}", root) { Children = new ObservableCollection<HierarchycalItem>() });
            roots.Add(root);
        }
        return roots;
    }

    [TestMethod("Collection : adding/removing a root item re-renders the tree")]
    [DataRow(false)]
    [DataRow(true)]
    public void Collection_RootAddRemove_Rerenders(bool checkbox)
    {
        var items = GetObservableItems(3, 2);
        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters =>
        {
            parameters
                .Add(p => p.Items, items)
                .Add(p => p.Children, x => x.Children)
                .Add(p => p.Parent, x => x.Parent)
                .Add(p => p.DisplayMember, x => x.Name);
            if (checkbox)
                parameters.Add(p => p.SelectProperty, s => s.IsSelected);
        });

        var added = new HierarchycalItem("new-root", null) { Children = new ObservableCollection<HierarchycalItem>() };
        cut.InvokeAsync(() => items.Add(added));
        cut.WaitForAssertion(() => Assert.IsTrue(cut.Markup.Contains("new-root")));
        Assert.IsNotNull(cut.Instance.GetNode(added));

        var removed = items[0];
        cut.InvokeAsync(() => items.Remove(removed));
        cut.WaitForAssertion(() => Assert.IsFalse(cut.Markup.Contains(removed.Name)));
        Assert.IsNull(cut.Instance.GetNode(removed));
    }

    [TestMethod("Collection : adding/removing a child item re-renders its parent node")]
    [DataRow(false)]
    [DataRow(true)]
    public void Collection_ChildAddRemove_Rerenders(bool checkbox)
    {
        var items = GetObservableItems(2, 2);
        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters =>
        {
            parameters
                .Add(p => p.Items, items)
                .Add(p => p.Children, x => x.Children)
                .Add(p => p.Parent, x => x.Parent)
                .Add(p => p.DisplayMember, x => x.Name);
            if (checkbox)
                parameters.Add(p => p.SelectProperty, s => s.IsSelected);
        });

        var parent = items[1];
        var leaf = parent.Children[0];
        var added = new HierarchycalItem("new-grandchild", leaf) { Children = new ObservableCollection<HierarchycalItem>() };
        cut.InvokeAsync(() => leaf.Children.Add(added));
        cut.WaitForAssertion(() => Assert.IsTrue(cut.Markup.Contains("new-grandchild")));
        Assert.IsNotNull(cut.Instance.GetNode(leaf)!.GetNode(added));

        var removed = parent.Children[1];
        cut.InvokeAsync(() => parent.Children.Remove(removed));
        cut.WaitForAssertion(() => Assert.IsFalse(cut.Markup.Contains(removed.Name)));
        Assert.IsNull(cut.Instance.GetNode(parent)!.GetNode(removed));
    }

    [TestMethod("Collection : removing the selected item clears SelectedItem")]
    public async Task Collection_RemoveSelected_ClearsSelection()
    {
        var items = GetObservableItems(2, 2);
        var selected = items[0].Children[1];
        HierarchycalItem? reported = selected;
        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.Children, x => x.Children)
            .Add(p => p.Parent, x => x.Parent)
            .Add(p => p.DisplayMember, x => x.Name)
            .Add(p => p.SelectedItem, selected)
            .Add(p => p.SelectedItemChanged, (HierarchycalItem? i) => reported = i));

        var pov = new PrivateObject(cut.Instance);
        int waitCount = 0;
        while (pov.GetField("_selectedNode") == null && waitCount < 6)
        {
            waitCount++;
            await Task.Delay(100);
        }
        Assert.IsNotNull(pov.GetField("_selectedNode"));

        await cut.InvokeAsync(() => items[0].Children.Remove(selected));
        cut.WaitForAssertion(() => Assert.IsNull(cut.Instance.SelectedItem));
        Assert.IsNull(reported);
        Assert.IsNull(pov.GetField("_selectedNode"));
    }

    [TestMethod("Collection : adding an unchecked child to a fully checked parent makes it indeterminate")]
    public void Collection_CheckboxParentStateUpdatesOnChildAdd()
    {
        var items = GetObservableItems(1, 2);
        var parent = items[0];
        foreach (var c in parent.Children)
            c.IsSelected = true;

        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.Children, x => x.Children)
            .Add(p => p.Parent, x => x.Parent)
            .Add(p => p.DisplayMember, x => x.Name)
            .Add(p => p.SelectProperty, s => s.IsSelected));

        var node = (SayehTreeViewCheckboxItem<HierarchycalItem>)cut.Instance.GetNode(parent)!;
        cut.WaitForAssertion(() => Assert.IsTrue(node.CheckState));

        cut.InvokeAsync(() => parent.Children.Add(new HierarchycalItem("unchecked-child", parent) { Children = new ObservableCollection<HierarchycalItem>() }));
        cut.WaitForAssertion(() => Assert.IsNull(node.CheckState));
    }

    [TestMethod("Collection : non-observable source keeps default behavior")]
    public void Collection_NonObservable_NoRerender()
    {
        var items = HierarchycalItem.GetHierarchycalItems(3, 1).ToList();
        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.Children, x => x.Children)
            .Add(p => p.Parent, x => x.Parent)
            .Add(p => p.DisplayMember, x => x.Name));

        var added = new HierarchycalItem("not-observed", null);
        cut.InvokeAsync(() => items.Add(added));
        Assert.IsFalse(cut.Markup.Contains("not-observed"));

        cut.Render();
        Assert.IsTrue(cut.Markup.Contains("not-observed"));
    }

    #endregion

    #region Multi Select

    [TestMethod("Multi select : SelectProperty can be switched back to null")]
    public void SelectProperty_SetBackToNull_DoesNotThrow()
    {
        var items = HierarchycalItem.GetHierarchycalItems(3, 1);
        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.Children, x => x.Children)
            .Add(p => p.Parent, x => x.Parent)
            .Add(p => p.DisplayMember, x => x.Name)
            .Add(p => p.SelectProperty, s => s.IsSelected));

        Assert.IsInstanceOfType<SayehTreeViewCheckboxItem<HierarchycalItem>>(cut.Instance.GetNode(items.First()));

        cut.Render(parameters => parameters.Add(p => p.SelectProperty, (System.Linq.Expressions.Expression<Func<HierarchycalItem, bool>>)null!));

        Assert.IsNotInstanceOfType<SayehTreeViewCheckboxItem<HierarchycalItem>>(cut.Instance.GetNode(items.First()));
    }

    [TestMethod]
    [DataRow(false)]
    public void TreeViewCheckboxItem_GetIsChecked(bool virtualize)
    {
        var items = HierarchycalItem.GetHierarchycalItems(4, 4);

        var firstLevelItem = items.FirstOrDefault(i => i.Children != null && i.Children.Count() > 1)
                             ?? items.First();
        var secondLevelItem = firstLevelItem.Children.Skip(Random.Shared.Next(0, firstLevelItem.Children.Count() - 1)).First();
        var thirdLevelItem = secondLevelItem.Children.Skip(Random.Shared.Next(0, secondLevelItem.Children.Count() - 1)).First();
        var forthLevelItem = thirdLevelItem.Children.Skip(Random.Shared.Next(0, thirdLevelItem.Children.Count() - 1)).First();

        forthLevelItem.IsSelected = true;
        //forthLevelItem.Id = 1102794386;

        Assert.IsNotNull(items);

        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
        .Add(p => p.Items, items)
        .Add(p => p.Children, x => x.Children)
        .Add(p => p.Parent, x => x.Parent)
        .Add(p => p.DisplayMember, x => x.Name)
        .Add(p => p.Virtualize, virtualize)
        .Add(p => p.SelectProperty, s => s.IsSelected)
        );

        var pov = new PrivateObject(cut.Instance);
        var firstLevelNodes = pov.GetField("_allItems") as Dictionary<string, SayehTreeViewItem<HierarchycalItem>>;
        Assert.IsNotNull(firstLevelNodes);

        var firstLevelNode = firstLevelNodes.First(f => f.Value.Item == firstLevelItem);
        var firstLevelNodePov = new PrivateObject(firstLevelNode.Value);
        if ((firstLevelNodePov.GetFieldOrProperty("_children") as Dictionary<string, SayehTreeViewItem<HierarchycalItem>>)!.Count() > 1)
            Assert.IsNull(firstLevelNodePov.Invoke("GetIsChecked").As<bool?>());
        else
            Assert.IsTrue(firstLevelNodePov.Invoke("GetIsChecked").As<bool>());
    }

    [TestMethod]
    [DataRow(false)]
    public void TreeViewCheckboxItem_HasBothCheckedUncheckedChild(bool virtualize)
    {
        var items = HierarchycalItem.GetHierarchycalItems(4, 4);

        var firstLevelItem = items.FirstOrDefault(i => i.Children != null && i.Children.Count() > 1)
                             ?? items.First();
        var secondLevelItem = firstLevelItem.Children.Skip(Random.Shared.Next(0, firstLevelItem.Children.Count() - 1)).First();
        var thirdLevelItem = secondLevelItem.Children.Skip(Random.Shared.Next(0, secondLevelItem.Children.Count() - 1)).First();
        var forthLevelItem = thirdLevelItem.Children.Skip(Random.Shared.Next(0, thirdLevelItem.Children.Count() - 1)).First();

        forthLevelItem.IsSelected = true;
        //forthLevelItem.Id = 1102794386;

        Assert.IsNotNull(items);

        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
        .Add(p => p.Items, items)
        .Add(p => p.Children, x => x.Children)
        .Add(p => p.Parent, x => x.Parent)
        .Add(p => p.DisplayMember, x => x.Name)
        .Add(p => p.Virtualize, virtualize)
        .Add(p => p.SelectProperty, s => s.IsSelected)
        );

        var pov = new PrivateObject(cut.Instance);
        var firstLevelNode = cut.Instance.GetNode(firstLevelItem);
        Assert.IsNotNull(firstLevelNode);
        var secondLevelNode = firstLevelNode.GetNode(secondLevelItem);
        Assert.IsNotNull(secondLevelNode);
        var thirdLevelNode = secondLevelNode.GetNode(thirdLevelItem);
        Assert.IsNotNull(thirdLevelNode);
        if (thirdLevelNode._children.Count == 1)
            Assert.IsTrue(((SayehTreeViewCheckboxItem<HierarchycalItem>)thirdLevelNode!).CheckState);
        else
            Assert.IsNull(((SayehTreeViewCheckboxItem<HierarchycalItem>)thirdLevelNode!).CheckState);

        if (secondLevelNode._children.Count > 1 || ((SayehTreeViewCheckboxItem<HierarchycalItem>)thirdLevelNode!).CheckState is null)
            Assert.IsNull(((SayehTreeViewCheckboxItem<HierarchycalItem>)secondLevelNode!).CheckState);
        else
            Assert.IsTrue(((SayehTreeViewCheckboxItem<HierarchycalItem>)secondLevelNode!).CheckState);

        Assert.IsNull(((SayehTreeViewCheckboxItem<HierarchycalItem>)firstLevelNode!).CheckState);

    }

    [TestMethod]
    [DataRow(false)]
    public void TreeViewCheckboxItem_SetParentOfCheckedItemToNull(bool virtualize)
    {
        var items = HierarchycalItem.GetHierarchycalItems(4, 4);

        var firstLevelItem = items.FirstOrDefault(i => i.Children != null && i.Children.Count() > 1)
                             ?? items.First();
        var secondLevelItem = firstLevelItem.Children.Skip(Random.Shared.Next(0, firstLevelItem.Children.Count() - 1)).First();
        var thirdLevelItem = secondLevelItem.Children.Skip(Random.Shared.Next(0, secondLevelItem.Children.Count() - 1)).First();
        var forthLevelItem = thirdLevelItem.Children.Skip(Random.Shared.Next(0, thirdLevelItem.Children.Count() - 1)).First();

        forthLevelItem.IsSelected = true;
        //forthLevelItem.Id = 1102794386;

        Assert.IsNotNull(items);

        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
        .Add(p => p.Items, items)
        .Add(p => p.Children, x => x.Children)
        .Add(p => p.Parent, x => x.Parent)
        .Add(p => p.DisplayMember, x => x.Name)
        .Add(p => p.Virtualize, virtualize)
        .Add(p => p.SelectProperty, s => s.IsSelected)
        );

        var pov = new PrivateObject(cut.Instance);
        var firstLevelNode = cut.Instance.GetNode(firstLevelItem) as SayehTreeViewCheckboxItem<HierarchycalItem>;
        Assert.IsNotNull(firstLevelNode);
        var firstLevelNodePov = new PrivateObject(firstLevelNode);

        if ((firstLevelNodePov.GetFieldOrProperty("_children") as Dictionary<string, SayehTreeViewItem<HierarchycalItem>>)!.Count() > 1)
        {
            Assert.IsNull(firstLevelNodePov.GetFieldOrProperty("CheckState").As<bool?>());
            Assert.IsFalse(firstLevelNode._selectProperty.Invoke(firstLevelItem).As<bool>());
        }
        else
        {
            Assert.IsTrue(firstLevelNodePov.GetFieldOrProperty("CheckState").As<bool>());
            Assert.IsFalse(firstLevelNode._selectProperty.Invoke(firstLevelItem).As<bool>());
        }

        Assert.IsFalse(firstLevelNode.InitiallySelected);

        var secondLevelNode = firstLevelNode.GetNode(secondLevelItem) as SayehTreeViewCheckboxItem<HierarchycalItem>;
        Assert.IsNotNull(secondLevelNode);
        var secondLevelNodePov = new PrivateObject(secondLevelNode);

        if ((secondLevelNodePov.GetFieldOrProperty("_children") as Dictionary<string, SayehTreeViewItem<HierarchycalItem>>)!.Count() > 1)
        {
            Assert.IsNull(secondLevelNodePov.GetFieldOrProperty("CheckState").As<bool?>());
            Assert.IsFalse(secondLevelNode._selectProperty.Invoke(firstLevelItem).As<bool>());
        }
        else
        {
            Assert.IsTrue(secondLevelNodePov.GetFieldOrProperty("CheckState").As<bool?>());
            Assert.IsFalse(secondLevelNode._selectProperty.Invoke(firstLevelItem).As<bool>());
        }

        var thirdLevelNode = secondLevelNode.GetNode(thirdLevelItem) as SayehTreeViewCheckboxItem<HierarchycalItem>;
        Assert.IsNotNull(thirdLevelNode);
        var thirdLevelNodePov = new PrivateObject(thirdLevelNode);
        if ((thirdLevelNodePov.GetFieldOrProperty("_children") as Dictionary<string, SayehTreeViewItem<HierarchycalItem>>)!.Count() > 1)
        {
            Assert.IsNull(thirdLevelNodePov.GetFieldOrProperty("CheckState").As<bool?>());
            Assert.IsFalse(thirdLevelNode._selectProperty.Invoke(firstLevelItem).As<bool>());
        }
        else
        {
            Assert.IsTrue(thirdLevelNodePov.GetFieldOrProperty("CheckState").As<bool?>());
            Assert.IsFalse(thirdLevelNode._selectProperty.Invoke(firstLevelItem).As<bool>());
        }
    }

    //[TestMethod]
    //[DataRow(false)]
    public void TreeViewCheckboxItem_SetParentOfCheckedItemToFalse(bool virtualize)
    {
        var items = HierarchycalItem.GetHierarchycalItems(4, 4);

        var firstLevelItem = items.FirstOrDefault(i => i.Children != null && i.Children.Count() > 1)
                             ?? items.First();
        var first2ndLevelItem = items.FirstOrDefault(i => i.Children != null && i.Children.Count() > 1)
                             ?? items.Skip(1).First();
        var secondLevelItem = firstLevelItem.Children.Skip(Random.Shared.Next(0, firstLevelItem.Children.Count() - 1)).First();
        var thirdLevelItem = secondLevelItem.Children.Skip(Random.Shared.Next(0, secondLevelItem.Children.Count() - 1)).First();
        var forthLevelItem = thirdLevelItem.Children.Skip(Random.Shared.Next(0, thirdLevelItem.Children.Count() - 1)).First();

        forthLevelItem.IsSelected = true;
        //first2ndLevelItem.Id = 1102794386;

        Assert.IsNotNull(items);

        var cut = Render<SayehTreeView<HierarchycalItem>>(parameters => parameters
        .Add(p => p.Items, items)
        .Add(p => p.Children, x => x.Children)
        .Add(p => p.Parent, x => x.Parent)
        .Add(p => p.DisplayMember, x => x.Name)
        .Add(p => p.Virtualize, virtualize)
        .Add(p => p.SelectProperty, s => s.IsSelected)
        );

        var pov = new PrivateObject(cut.Instance);
        var firstLevelNode = cut.Instance.GetNode(first2ndLevelItem) as SayehTreeViewCheckboxItem<HierarchycalItem>;
        Assert.IsNotNull(firstLevelNode);
       
        Assert.IsNull(firstLevelNode.CheckState);
        forthLevelItem.IsSelected = false;
        
        Assert.IsFalse(firstLevelNode.CheckState);


    }


    #endregion

    #region Drag and drop

    (ObservableCollection<HierarchycalItem> roots, IRenderedComponent<SayehTreeView<HierarchycalItem>> cut, List<TreeViewItemDropEventArgs<HierarchycalItem>> drops)
        RenderDragDropTree(Func<TreeViewItemDropEventArgs<HierarchycalItem>, bool>? canDrop = null, System.Windows.Input.ICommand? droppedCommand = null)
        => RenderDragDropTree<SayehTreeView<HierarchycalItem>>(canDrop, droppedCommand);

    (ObservableCollection<HierarchycalItem> roots, IRenderedComponent<TTree> cut, List<TreeViewItemDropEventArgs<HierarchycalItem>> drops)
        RenderDragDropTree<TTree>(Func<TreeViewItemDropEventArgs<HierarchycalItem>, bool>? canDrop = null, System.Windows.Input.ICommand? droppedCommand = null)
        where TTree : SayehTreeView<HierarchycalItem>
    {
        // root0 { child0, child1, child2 }, root1 { }, root2 { }
        var roots = new ObservableCollection<HierarchycalItem>();
        for (var i = 0; i < 3; i++)
            roots.Add(new HierarchycalItem($"root{i}", null) { Children = new ObservableCollection<HierarchycalItem>() });
        for (var i = 0; i < 3; i++)
            roots[0].Children.Add(new HierarchycalItem($"child{i}", roots[0]) { Children = new ObservableCollection<HierarchycalItem>() });

        var drops = new List<TreeViewItemDropEventArgs<HierarchycalItem>>();
        var cut = Render<TTree>(parameters => parameters
            .Add(p => p.Items, roots)
            .Add(p => p.Children, x => x.Children)
            .Add(p => p.Parent, x => x.Parent)
            .Add(p => p.DisplayMember, x => x.Name)
            .Add(p => p.AllowDragDrop, true)
            .Add(p => p.CanDrag, x => x.Name != "root2")
            .Add(p => p.CanDrop, canDrop)
            .Add(p => p.ItemDroppedCommand, droppedCommand)
            .Add(p => p.ItemDropped, (TreeViewItemDropEventArgs<HierarchycalItem> e) =>
            {
                drops.Add(e);
                var source = e.OldParent?.Children ?? roots;
                var destination = e.NewParent?.Children ?? roots;
                source.Remove(e.Item);
                destination.Insert(e.Index, e.Item);
                e.Item.Parent = e.NewParent;
            }));
        return (roots, cut, drops);
    }

    static string NodeId<TTree>(IRenderedComponent<TTree> cut, HierarchycalItem item) where TTree : SayehTreeView<HierarchycalItem>
        => cut.Instance.GetNode(item)?.Id ?? cut.Instance.GetNode(item.Parent!).GetNode(item)!.Id!;

    [TestMethod("DragDrop : rows are draggable only when allowed")]
    public void DragDropDraggableAttributeTest()
    {
        var (roots, cut, _) = RenderDragDropTree();
        var rows = cut.FindAll("fluent-tree-view > fluent-tree-item > .treeitem-row");
        Assert.AreEqual(3, rows.Count);
        Assert.AreEqual("true", rows[0].GetAttribute("draggable"));
        Assert.IsNull(rows[2].GetAttribute("draggable"), "CanDrag returns false for root2");
    }

    [TestMethod("DragDrop : reorder siblings")]
    [DataRow("Before", 0, 2, new[] { "child2", "child0", "child1" })]
    [DataRow("After", 2, 0, new[] { "child1", "child2", "child0" })]
    [DataRow("After", 0, 1, new[] { "child0", "child1", "child2" })]
    public async Task DragDropReorderSiblingsTest(string position, int targetIndex, int sourceIndex, string[] expected)
    {
        var (roots, cut, drops) = RenderDragDropTree();
        var children = roots[0].Children;
        cut.Instance.GetNode(roots[0]).SetExpanded(true);
        cut.Render();

        await cut.InvokeAsync(() => cut.Instance.DropItemAsync(NodeId(cut, children[sourceIndex]), NodeId(cut, children[targetIndex]), position));

        Assert.AreEqual(1, drops.Count);
        Assert.AreSame(roots[0], drops[0].OldParent);
        Assert.AreSame(roots[0], drops[0].NewParent);
        CollectionAssert.AreEqual(expected, children.Select(c => c.Name).ToArray());
    }

    [TestMethod("DragDrop : move inside another item and keep the selection")]
    public async Task DragDropInsideKeepsSelectionTest()
    {
        var (roots, cut, drops) = RenderDragDropTree();
        var root0 = roots[0];
        var root1 = roots[1];
        HierarchycalItem? selectedChanged = root0;
        cut.Render(parameters => parameters
            .Add(p => p.SelectedItem, root0)
            .Add(p => p.SelectedItemChanged, (HierarchycalItem? i) => selectedChanged = i));
        await cut.InvokeAsync(() => cut.Instance.GetNode(root0).HandleRowClick());

        await cut.InvokeAsync(() => cut.Instance.DropItemAsync(NodeId(cut, root0), NodeId(cut, root1), "Inside"));

        Assert.AreEqual(1, drops.Count);
        Assert.IsNull(drops[0].OldParent);
        Assert.AreSame(root1, drops[0].NewParent);
        Assert.AreEqual(0, drops[0].Index);
        CollectionAssert.AreEqual(new[] { "root1", "root2" }, roots.Select(r => r.Name).ToArray());
        Assert.AreSame(root0, root1.Children.Single());
        Assert.AreSame(root0, selectedChanged, "moving the selected item must not clear the selection");
        Assert.AreSame(root0, cut.Instance.SelectedItem);
        Assert.IsTrue(cut.Instance.GetNode(root1).Expanded, "the drop target is expanded to reveal the moved item");
    }

    [TestMethod("DragDrop : refuse invalid drops")]
    public async Task DragDropRefusedTest()
    {
        var (roots, cut, drops) = RenderDragDropTree(e => e.Target.Name != "root1");
        cut.Instance.GetNode(roots[0]).SetExpanded(true);
        cut.Render();
        var root0 = NodeId(cut, roots[0]);

        // onto itself / its own descendant
        CollectionAssert.AreEqual(Array.Empty<string>(), await cut.Instance.GetAllowedDropPositionsAsync(root0, root0));
        CollectionAssert.AreEqual(Array.Empty<string>(), await cut.Instance.GetAllowedDropPositionsAsync(root0, NodeId(cut, roots[0].Children[1])));
        // CanDrag = false
        CollectionAssert.AreEqual(Array.Empty<string>(), await cut.Instance.GetAllowedDropPositionsAsync(NodeId(cut, roots[2]), root0));
        // CanDrop = false
        CollectionAssert.AreEqual(Array.Empty<string>(), await cut.Instance.GetAllowedDropPositionsAsync(root0, NodeId(cut, roots[1])));
        CollectionAssert.AreEqual(new[] { "Before", "Inside", "After" }, await cut.Instance.GetAllowedDropPositionsAsync(root0, NodeId(cut, roots[2])));

        await cut.InvokeAsync(() => cut.Instance.DropItemAsync(root0, NodeId(cut, roots[1]), "Inside"));
        Assert.AreEqual(0, drops.Count);
    }

    [TestMethod("DragDrop : ItemDroppedCommand.CanExecute refuses drops")]
    public async Task DragDropCommandCanExecuteTest()
    {
        // The command refuses dropping inside another item: only reordering is allowed.
        var command = new DropCommand(e => e.Position != TreeViewDropPosition.Inside);
        var (roots, cut, drops) = RenderDragDropTree(droppedCommand: command);
        var root0 = NodeId(cut, roots[0]);
        var root1 = NodeId(cut, roots[1]);

        CollectionAssert.AreEqual(new[] { "Before", "After" }, await cut.Instance.GetAllowedDropPositionsAsync(root0, root1));

        await cut.InvokeAsync(() => cut.Instance.DropItemAsync(root0, root1, "Inside"));
        Assert.AreEqual(0, command.Executed.Count, "a refused drop does not execute the command");
        Assert.AreEqual(0, drops.Count, "a refused drop does not raise ItemDropped");

        await cut.InvokeAsync(() => cut.Instance.DropItemAsync(root0, root1, "After"));
        Assert.AreEqual(1, command.Executed.Count);
        Assert.AreEqual(1, drops.Count);
        Assert.AreSame(command.Executed[0], drops[0], "the command and the callback receive the same args");
    }

    class DropCommand(Func<TreeViewItemDropEventArgs<HierarchycalItem>, bool> canExecute) : System.Windows.Input.ICommand
    {
        public List<TreeViewItemDropEventArgs<HierarchycalItem>> Executed { get; } = [];
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => canExecute((TreeViewItemDropEventArgs<HierarchycalItem>)parameter!);
        public void Execute(object? parameter) => Executed.Add((TreeViewItemDropEventArgs<HierarchycalItem>)parameter!);
    }

    [TestMethod("DragDrop : overridable OnDragEntering/OnDragEntered")]
    public async Task DragDropOverrideDragEnterTest()
    {
        var (roots, cut, _) = RenderDragDropTree<TrackingTreeView>();
        var tree = cut.Instance;
        var root0 = NodeId(cut, roots[0]);

        Assert.IsTrue(tree.OverridesDragEnter, "a derived tree overriding OnDragEnter* makes the JS module ask .NET");
        Assert.IsFalse(RenderDragDropTree().cut.Instance.OverridesDragEnter);

        // allowed: Entering then Entered, with the evaluated positions
        var allowed = await tree.GetAllowedDropPositionsAsync(root0, NodeId(cut, roots[1]));
        CollectionAssert.AreEqual(new[] { "Before", "Inside", "After" }, allowed);
        CollectionAssert.AreEqual(new[] { "Entering root1", "Entered root1 Before,Inside,After" }, tree.Log);

        // cancelled in OnDragEntering: no position, OnDragEntered not called
        tree.Log.Clear();
        tree.CancelEnterOn = "root2";
        CollectionAssert.AreEqual(Array.Empty<string>(), await tree.GetAllowedDropPositionsAsync(root0, NodeId(cut, roots[2])));
        CollectionAssert.AreEqual(new[] { "Entering root2" }, tree.Log);
    }

    [TestMethod("DragDrop : overridable OnDropping/OnDropped")]
    public async Task DragDropOverrideDropTest()
    {
        var command = new DropCommand(_ => true);
        var (roots, cut, drops) = RenderDragDropTree<TrackingTreeView>(droppedCommand: command);
        var tree = cut.Instance;
        var root0 = NodeId(cut, roots[0]);

        // cancelled in OnDropping: neither the command, ItemDropped nor OnDropped run
        tree.CancelDropOn = "root1";
        await cut.InvokeAsync(() => tree.DropItemAsync(root0, NodeId(cut, roots[1]), "Inside"));
        CollectionAssert.AreEqual(new[] { "Dropping root1" }, tree.Log);
        Assert.AreEqual(0, command.Executed.Count);
        Assert.AreEqual(0, drops.Count);

        // accepted: OnDropping -> command -> ItemDropped -> OnDropped
        tree.Log.Clear();
        tree.CancelDropOn = null;
        await cut.InvokeAsync(() => tree.DropItemAsync(root0, NodeId(cut, roots[2]), "After"));
        CollectionAssert.AreEqual(new[] { "Dropping root2", "Dropped root2 commands:1 callbacks:1" }, tree.Log);
        Assert.AreEqual(1, drops.Count);
    }

    [TestMethod("DragDrop : a derived tree moving the selected item in OnDropped keeps the selection")]
    public async Task DragDropMoveInOnDroppedKeepsSelectionTest()
    {
        var roots = new ObservableCollection<HierarchycalItem>();
        for (var i = 0; i < 2; i++)
            roots.Add(new HierarchycalItem($"root{i}", null) { Children = new ObservableCollection<HierarchycalItem>() });
        var root0 = roots[0];
        var root1 = roots[1];
        HierarchycalItem? selectedChanged = root0;
        var cut = Render<MovingTreeView>(parameters => parameters
            .Add(p => p.Items, roots)
            .Add(p => p.Children, x => x.Children)
            .Add(p => p.Parent, x => x.Parent)
            .Add(p => p.DisplayMember, x => x.Name)
            .Add(p => p.AllowDragDrop, true)
            .Add(p => p.SelectedItem, root0)
            .Add(p => p.SelectedItemChanged, (HierarchycalItem? i) => selectedChanged = i));
        cut.Instance.Roots = roots;
        await cut.InvokeAsync(() => cut.Instance.GetNode(root0).HandleRowClick());

        await cut.InvokeAsync(() => cut.Instance.DropItemAsync(NodeId(cut, root0), NodeId(cut, root1), "Inside"));

        Assert.AreSame(root0, root1.Children.Single());
        Assert.AreSame(root0, selectedChanged, "moving the selected item in OnDropped must not clear the selection");
        Assert.AreSame(root0, cut.Instance.SelectedItem);
    }

    /// <summary>Moves the dropped item itself, in OnDropped, like a tree bound to a ViewModel's hierarchy editor.</summary>
    class MovingTreeView : SayehTreeView<HierarchycalItem>
    {
        public ObservableCollection<HierarchycalItem> Roots { get; set; } = [];

        protected override Task OnDropped(TreeViewItemDropEventArgs<HierarchycalItem> args)
        {
            (args.OldParent?.Children ?? Roots).Remove(args.Item);
            (args.NewParent?.Children ?? Roots).Add(args.Item);
            args.Item.Parent = args.NewParent;
            return Task.CompletedTask;
        }
    }

    class TrackingTreeView : SayehTreeView<HierarchycalItem>
    {
        public List<string> Log { get; } = [];
        public string? CancelEnterOn { get; set; }
        public string? CancelDropOn { get; set; }
        // Counts what already ran when OnDropped is called, to check the order.
        int _commandCount => (ItemDroppedCommand as DropCommand)?.Executed.Count ?? 0;
        int _callbackCount;

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            var dropped = ItemDropped;
            ItemDropped = EventCallback.Factory.Create<TreeViewItemDropEventArgs<HierarchycalItem>>(this, async e =>
            {
                _callbackCount++;
                await dropped.InvokeAsync(e);
            });
        }

        protected override async Task OnDragEntering(TreeViewItemDragEnterEventArgs<HierarchycalItem> args)
        {
            await Task.Yield();
            Log.Add($"Entering {args.Target.Name}");
            args.Cancel = args.Target.Name == CancelEnterOn;
        }

        protected override Task OnDragEntered(TreeViewItemDragEnterEventArgs<HierarchycalItem> args)
        {
            Log.Add($"Entered {args.Target.Name} {string.Join(",", args.AllowedPositions)}");
            return Task.CompletedTask;
        }

        protected override async Task OnDropping(TreeViewItemDropEventArgs<HierarchycalItem> args)
        {
            await Task.Yield();
            Log.Add($"Dropping {args.Target.Name}");
            args.Cancel = args.Target.Name == CancelDropOn;
        }

        protected override Task OnDropped(TreeViewItemDropEventArgs<HierarchycalItem> args)
        {
            Log.Add($"Dropped {args.Target.Name} commands:{_commandCount} callbacks:{_callbackCount}");
            return Task.CompletedTask;
        }
    }

    #endregion
}
