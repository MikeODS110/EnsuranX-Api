using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Requests.CMS.GOV
{
    public class HouseholdInfo
    {
        public int Income { get; set; }
        public List<PersonInfo> People { get; set; }
        public bool HasMarriedCouple { get; set; }
    }

    public class PersonInfo
    {
        public int Age { get; set; }
        public bool IsPregnant { get; set; }
        public bool IsParent { get; set; }
        public bool UsesTobacco { get; set; }
        public string Gender { get; set; }
    }
    public class PlaceInfo
    {
        public string CountyFips { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
    }
    public class PlanRequest
    {
        public HouseholdInfo Household { get; set; }
        public PlaceInfo Place { get; set; }
        public string Market { get; set; }
        public List<string> PlanIds { get; set; }
        public int Year { get; set; }
        public int AptcOverride { get; set; }
        public string CsrOverride { get; set; }
        public bool CatastrophicOverride { get; set; }
    }



}
