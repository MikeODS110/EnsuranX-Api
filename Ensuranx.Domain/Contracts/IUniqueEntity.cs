namespace Ensuranx.Domain.Contracts
{
    public interface IUniqueEntity<TId>
    {
        TId Id { get; set; }
    }
}
