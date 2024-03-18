using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Response.Tournament
{
    public class RequestedTeamsInTournament
    {
        public string Name { get; set; } = string.Empty;
        public long Id { get; set; }
        public string Color { get; set; }   
    }

}
