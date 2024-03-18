using System.ComponentModel.DataAnnotations;

namespace Ensuranx.Domain.Contracts
{
    public interface IAuditableEntity<TId> : IUniqueEntity<TId>,IEntity<TId>
    {
        [DataType(DataType.Date), DisplayFormat(DataFormatString = @"{0:dd\/MM\/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        DateTime CreatedDateTime { get; set; }
        DateTime LastModifiedDateTime { get; set; }
        string CreatedBy { get; set; }
        string LastModifiedBy { get; set; }
        bool IsDeleted { get; set; }
        bool IsActive { get; set; }
    }
}
