namespace Ensuranx.Application.Response.Branch
{
    public class AllBranchesResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ProfileImageUrl { get; set; }
        public string CoverImageUrl { get; set; }
        public List<StatkeeperImagesDetails> StatkeeperDetails{ get; set; }= new List<StatkeeperImagesDetails>();
    }
    public class StatkeeperImagesDetails
    {
        public long BranchId { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }    
        public string ProfileImage { get; set; }
    }
}
