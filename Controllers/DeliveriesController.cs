using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vantex.Technician.Service.Entities;
using Vantex.Technician.Service.Services;

namespace Vantex.Technician.Service.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveriesController : ControllerBase
    {

        private readonly ScheduleService _scheduleService;

        public IUserService _userService;

        public DeliveriesController(ScheduleService scheduleService, IUserService userService)
        {
            _scheduleService = scheduleService;
            _userService = userService;
        }

        // GET api/values/5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Delivery>>> Get()
        {
            int? baseId = _userService.GetBaseIdClaim(HttpContext.User.Identity as ClaimsIdentity);

            if (baseId == null || !await _scheduleService.BaseIdExist(baseId.Value))
            {
                return NotFound();
            }

            return Ok(await _scheduleService.GetAllDeliveries(baseId.Value));
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
            throw new NotImplementedException();
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
            throw new NotImplementedException();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
