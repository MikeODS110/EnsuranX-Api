namespace Ensuranx.Application.Response.BusinessPackage;

public class GetBusinessType
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
}
