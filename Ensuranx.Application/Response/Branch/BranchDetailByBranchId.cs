using Ensuranx.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Response.Branch
{
    public class BranchDetailByBranchId
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ProfileImageUrl { get; set; }
        public string CoverImageUrl { get; set; }
        
       
        public List<BranchSchedulingDetailByBranchId> branchSchedulings { get; set; }   
        public List<ConnectionDetailsbyBranchId> connections { get; set; }
    }

    public class BranchSchedulingDetailByBranchId
    {
        public long BranchId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public long WeekDay { get; set; }
    }
    public class ConnectionDetailsbyBranchId
    {
        public string Name { get; set; }
        public string Value { get; set; }
        
    }
}
