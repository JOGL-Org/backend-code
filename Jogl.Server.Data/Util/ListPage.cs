namespace Jogl.Server.Data.Util
{
    public class ListPage<T>
    {
        public int Total { get; set; }
        public IEnumerable<T> Items { get; set; }

        public ListPage(IEnumerable<T> items)
        {
            Items = items;
        }

        public ListPage(IEnumerable<T> items, int total)
        {
            Items = items;
            Total = total;
        }
    }
}
