namespace Ensuranx.Common.PaginationResponse
{
    public class BasicFilter
    {
        public string SortBy { get; set; } = "Id";
        public bool IsAsc { get; set; } = true;
        public string sSearch { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
      
        public BasicFilter(int pageNumber, int pageSize)
        {
            this.PageNumber = pageNumber < 1 ? 1 : pageNumber;
            this.PageSize = pageSize;
        }
    }
}
