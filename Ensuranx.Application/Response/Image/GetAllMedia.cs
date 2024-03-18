namespace Ensuranx.Application.Response.Image
{
    public class GetAllMedia
    {
        public long Id { get; set; }
        public long TableId { get; set; }
        public string Path { get; set; } = default!;
    }
}
