using Ensuranx.Application.Requests.Branch;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Infrastructure.Mappers
{
    public class BranchSchedulingMapper
    {
        public List<BranchScheduling> MapBranchScheduling(string email, AddBranch addBranch,Branch branch)
        {
            List<BranchScheduling> branchSchedulingList = new List<BranchScheduling>();

            foreach (var item in addBranch.BranchSchedulingDetailList)
            {
                BranchScheduling branchScheduling = new BranchScheduling();
                TimeSpan ts = new TimeSpan(10, 30, 0);

                branchScheduling.StartTime = TimeSpan.Parse(item.StartTime);
                branchScheduling.EndTime = TimeSpan.Parse(item.EndTime);

                branchScheduling.WeekDay =  item.WeekDays.ToLower().Trim() == Domain.Enums.Enum.WeekDays.Monday.ToString().ToLower().Trim() ? Domain.Enums.Enum.WeekDays.Monday :
                                            item.WeekDays.ToLower().Trim() == Domain.Enums.Enum.WeekDays.Tuesday.ToString().ToLower().Trim() ? Domain.Enums.Enum.WeekDays.Tuesday :
                                            item.WeekDays.ToLower().Trim() == Domain.Enums.Enum.WeekDays.Wednesday.ToString().ToLower().Trim() ? Domain.Enums.Enum.WeekDays.Wednesday :
                                            item.WeekDays.ToLower().Trim() == Domain.Enums.Enum.WeekDays.Thursday.ToString().ToLower().Trim() ? Domain.Enums.Enum.WeekDays.Thursday :
                                            item.WeekDays.ToLower().Trim() == Domain.Enums.Enum.WeekDays.Friday.ToString().ToLower().Trim() ? Domain.Enums.Enum.WeekDays.Friday :
                                            item.WeekDays.ToLower().Trim() == Domain.Enums.Enum.WeekDays.Saturday.ToString().ToLower().Trim() ? Domain.Enums.Enum.WeekDays.Saturday :
                                            item.WeekDays.ToLower().Trim() == Domain.Enums.Enum.WeekDays.Sunday.ToString().ToLower().Trim() ? Domain.Enums.Enum.WeekDays.Sunday :
                                            Domain.Enums.Enum.WeekDays.Monday;
                branchScheduling.CreatedBy = email;
                branchScheduling.LastModifiedBy = email;
                branchScheduling.CreatedDateTime = DateTime.UtcNow;
                branchScheduling.LastModifiedDateTime = DateTime.UtcNow;
                branchScheduling.BranchId = branch.Id;

                branchSchedulingList.Add(branchScheduling);
            }

            
            return branchSchedulingList;
        }
    }
}
