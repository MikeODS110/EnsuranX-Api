namespace Ensuranx.Application.Response.Other
{
    public class PlayerDetails
    {
        public long Id { get; set; }
        public long PlayerId { get; set; }
        public string Name { get; set; } = default!;
        public string SequenceNumber { get; set; } = default!;
        public string Role { get; set; } = default!;
        public string Path { get; set; } = default!;
        public bool IsSubstituteOrDisqualify { get; set; } = false;
        public List<BasketballEnsuranx> BasketballEnsuranxList { get; set; } = default!;
    }
}
