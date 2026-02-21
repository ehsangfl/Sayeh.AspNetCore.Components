using Microsoft.AspNetCore.Components.Web.Virtualization;
using Sample.Client.Model;
using System.Collections;
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
        .Add(p => p.ParentItem, x => x.Parent)
        .Add(p => p.Text, x => x.Name)
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
        .Add(p => p.Text, x => x.Name)
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
            var item = (WeatherForecast)context;
            __builder.OpenComponent<SayehTreeViewItem<object>>(0);
            __builder.AddComponentParameter(1, nameof(SayehTreeViewItem<object>.Item), item);
            __builder.AddComponentParameter(2, nameof(SayehTreeViewItem<object>.ParentItem), (Func<object, object?>)(f => ((WeatherForecast)f).Group));
            __builder.AddComponentParameter(3, nameof(SayehTreeViewItem<object>.InitiallySelected), item == secondLevelItem);
            __builder.AddComponentParameter(4, nameof(SayehTreeViewItem<object>.ItemTemplate), secondLevelItemtemplate);
            __builder.CloseComponent();
        };
        RenderFragment<object> treeItemtemplate = context => __builder =>
        {
            var item = (WeatherForecastGroup)context;
            __builder.OpenComponent<SayehTreeViewItem<object>>(0);
            __builder.AddComponentParameter(1, nameof(SayehTreeViewItem<object>.Item), item);
            __builder.AddComponentParameter(2, nameof(SayehTreeViewItem<object>.Children), (Func<object, IEnumerable<object>>)(f => ((WeatherForecastGroup)f).Items.Cast<object>()));
            __builder.AddComponentParameter(3, nameof(SayehTreeViewItem<object>.Text), (Func<object, string>)(f => ((WeatherForecastGroup)f).Name));
            __builder.AddComponentParameter(4, nameof(SayehTreeViewItem<object>.ItemTemplate), firstLevelItemtemplate);
            __builder.AddComponentParameter(5, nameof(SayehTreeViewItem<object>.ChildrenTemplate), childrenItemtemplate);
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
        .Add(p => p.ParentItem, x => x.Parent)
        .Add(p => p.Text, x => x.Name)
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
        .Add(p => p.ParentItem, x => x.Parent)
        .Add(p => p.Text, x => x.Name)
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
        .Add(p => p.ParentItem, x => x.Parent)
        .Add(p => p.Text, x => x.Name)
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
        .Add(p => p.ParentItem, x => x.Parent)
        .Add(p => p.Text, x => x.Name)
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

    #region Multi Select

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
        .Add(p => p.ParentItem, x => x.Parent)
        .Add(p => p.Text, x => x.Name)
        .Add(p => p.Virtualize, virtualize)
        .Add(p => p.SelectProperty, s => s.IsSelected)
        .Add(p => p.IDProperty, s => s.ID)
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
        .Add(p => p.ParentItem, x => x.Parent)
        .Add(p => p.Text, x => x.Name)
        .Add(p => p.Virtualize, virtualize)
        .Add(p => p.SelectProperty, s => s.IsSelected)
        .Add(p => p.IDProperty, s => s.ID)
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
        .Add(p => p.ParentItem, x => x.Parent)
        .Add(p => p.Text, x => x.Name)
        .Add(p => p.Virtualize, virtualize)
        .Add(p => p.SelectProperty, s => s.IsSelected)
        .Add(p => p.IDProperty, s => s.ID)
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
        .Add(p => p.ParentItem, x => x.Parent)
        .Add(p => p.Text, x => x.Name)
        .Add(p => p.Virtualize, virtualize)
        .Add(p => p.SelectProperty, s => s.IsSelected)
        .Add(p => p.IDProperty, s => s.ID)
        );

        var pov = new PrivateObject(cut.Instance);
        var firstLevelNode = cut.Instance.GetNode(first2ndLevelItem) as SayehTreeViewCheckboxItem<HierarchycalItem>;
        Assert.IsNotNull(firstLevelNode);
       
        Assert.IsNull(firstLevelNode.CheckState);
        forthLevelItem.IsSelected = false;
        
        Assert.IsFalse(firstLevelNode.CheckState);


    }


    #endregion
}
