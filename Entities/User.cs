using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vantex.Technician.Service.Entities
{
    public class User
    {
        public int Id { get; set; }

        public int BaseId { get; set; }

        public string BaseName { get; set; }

        public string ProbeId { get; set; }

        public string ProbeName { get; set; }

        public string Technician { get; set; }

        public string Password { get; set; }

        public string Token { get; set; }
    }
}
