using Sample.Client.Model;

namespace Sample.Client
{
    public static class Extensions
    {

        public static IEnumerable<HierarchycalItem> Flatten(this IEnumerable<HierarchycalItem> Items)
        {
            return Items.SelectMany(s => s.Children.Flatten()).Concat(Items);
        }


    }
}
