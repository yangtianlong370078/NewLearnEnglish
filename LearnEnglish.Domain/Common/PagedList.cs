namespace LearnEnglish.Domain.Common
{
    /// <summary>
    /// 分页结果封装（纯数据，不含 HTML 逻辑）
    /// </summary>
    /// <typeparam name="T">数据项类型</typeparam>
    public class PagedList<T>
    {
        /// <summary>
        /// 当前页数据
        /// </summary>
        public IReadOnlyList<T> Items { get; }

        /// <summary>
        /// 当前页码（从1开始）
        /// </summary>
        public int PageIndex { get; }

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCount => (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPrevious => PageIndex > 1;

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNext => PageIndex < PageCount;

        public PagedList(IEnumerable<T> items, int pageIndex, int pageSize, int totalCount)
        {
            Items = items.ToList().AsReadOnly();
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
    }
}
