namespace Ensuranx.Common.PaginationResponse
{
    public class PagedResponse<T>
    {
      
        public int TotalRecords { get; set; }
 
        public T Data { get; set; }
       
        public PagedResponse(int totalRecords, T data, int pageNumber, int pageSize)
        {

            this.Data = data;
            this.TotalRecords = totalRecords;

        }
    }
}
