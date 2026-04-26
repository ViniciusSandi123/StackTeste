namespace StackTeste.Domain.Common
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0
            ? (int)Math.Ceiling(TotalCount / (double)PageSize)
            : 0;

        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;

        public PagedResult() { }

        public PagedResult(IEnumerable<T> items, int totalCount, int page, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }
    }
}
