using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vantex.Technician.Service.Services;

namespace Vantex.Technician.Service.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TerminalsController : ControllerBase
    {

        private readonly ScheduleService _scheduleService;

        public IUserService _userService;

        public TerminalsController(ScheduleService scheduleService, IUserService userService)
        {
            _scheduleService = scheduleService;
            _userService = userService;
        }

        // GET api/values/5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> Get()
        {
            int? baseId = _userService.GetBaseIdClaim(HttpContext.User.Identity as ClaimsIdentity);
            if (baseId == null)
                return Unauthorized();
            if (!await _scheduleService.BaseIdExist(baseId.Value))
                return NotFound();
            return Ok(await _scheduleService.GetAllDeliveries(baseId.Value));
        }

    }
}