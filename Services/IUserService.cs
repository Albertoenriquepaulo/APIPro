using System.Collections.Generic;
using System.Security.Claims;
using Vantex.Technician.Service.Entities;

namespace Vantex.Technician.Service.Services
{
    public interface IUserService
    {
        User Authenticate(string probeId, string userPass);
        int? GetBaseIdClaim(ClaimsIdentity identity);
    }
}
