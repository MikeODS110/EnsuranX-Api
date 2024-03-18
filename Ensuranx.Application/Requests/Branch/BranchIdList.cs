namespace Ensuranx.Application.Requests.Branch;

public class BranchIdList
{
    public List<long>? branchIdList { get; set; }
    public long statkeeperId { get; set; } = 0;   
}
