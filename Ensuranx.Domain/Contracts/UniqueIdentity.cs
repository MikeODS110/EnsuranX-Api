namespace Ensuranx.Domain.Contracts
{
    public abstract class UniqueIdentity<TId> : IUniqueEntity<TId>
    {
        public TId Id { get; set; }
    }
}
