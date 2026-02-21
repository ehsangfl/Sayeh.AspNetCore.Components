namespace Sample.Client.Model
{
    public class HierarchycalItem
    {

        public static IEnumerable<HierarchycalItem> GetHierarchycalItems(int eachLevelCount, int depth)
        {
            List<HierarchycalItem> result = new();
            for (var i = 0; i < eachLevelCount; i++)
            {
                result.Add(generateItem(eachLevelCount,-1, depth));
            }
            return result;
        }

        private static HierarchycalItem generateItem(int eachLevelCount, int currentDepth, int maxDepth,HierarchycalItem? parent = null)
        {
            var random = new Random((int)DateTime.Now.Ticks + Random.Shared.Next());
            var id = random.Next();
            var item = new HierarchycalItem($"item {id}",parent);
            var cdepth = ++currentDepth;
            var elc = currentDepth ==0 ? eachLevelCount : random.Next(1, eachLevelCount);
            if (currentDepth < maxDepth)
                for (var i = 0; i < eachLevelCount; i++)
                {
                    item.Children.Add(generateItem(elc, cdepth, maxDepth, item));
                }
            return item;
        }

        public HierarchycalItem(string name, HierarchycalItem? parent)
        {
            ID = Guid.NewGuid();
            Name = name;
            Children = new List<HierarchycalItem>();
            Parent = parent;
        }

        public HierarchycalItem? Parent { get; set; }

        public Guid ID { get; set; }

        public string Name { get; set; }

        public IList<HierarchycalItem> Children { get; set; }

        public bool IsSelected { get; set; }

        public override string ToString()
        {
            return ID.ToString();
        }

    }
}
