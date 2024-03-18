
namespace Ensuranx.Application.Requests.Branch
{
    public class UpdateBranch
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string Profile { get; set; }
        public long ProfileId { get; set; }
        public string Cover { get; set; }
        public long CoverId { get; set; }
        public long BusinessId { get; set; }
    }
}
