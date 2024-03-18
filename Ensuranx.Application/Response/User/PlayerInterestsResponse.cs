namespace Ensuranx.Application.Response.User
{
    public class PlayerInterestsResponse
    {
        public List<ActivityListModel> ActivityCategoryIdList { get; set; }
        public int ZipCode { get; set; }
    }

    public class ActivityListModel
    {
        public long ActivityId { get; set; }
        public string ActivityName { get; set; }
    }
}
