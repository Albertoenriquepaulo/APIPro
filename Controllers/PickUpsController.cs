using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vantex.Technician.Service.Entities;
using Vantex.Technician.Service.Services;

namespace Vantex.Technician.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PickUpsController : ControllerBase
    {
        private readonly ScheduleService _scheduleService;
        private readonly IUserService _userService;

        public PickUpsController(ScheduleService scheduleService, IUserService userService)
        {
            _scheduleService = scheduleService;
            _userService = userService;
        }

        // GET: api/PickUps
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PickUp>>> Get()
        {
            int? baseId = _userService.GetBaseIdClaim(HttpContext.User.Identity as ClaimsIdentity);

            if (baseId == null || !await _scheduleService.BaseIdExist(baseId.Value))
            {
                return NotFound();
            }
            return Ok(await _scheduleService.GetAllPickUps(baseId.Value));
        }


        // POST: api/PickUps
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/PickUps/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
