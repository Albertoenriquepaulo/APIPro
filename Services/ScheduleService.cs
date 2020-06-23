using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Vantex.Technician.Service.Entities;
using Vantex.Technician.Service.Repositories;

namespace Vantex.Technician.Service.Services
{
    public class ScheduleService
    {
        private readonly ScheduleRepository _scheduleRepository;

        public ScheduleService(ScheduleRepository scheduleRepository)
        {
            _scheduleRepository = scheduleRepository ?? throw new ArgumentNullException(nameof(scheduleRepository));
        }

        public async Task<IEnumerable<object>> GetAllTerminals(int baseId)
        {
            return await _scheduleRepository.GetAllTerminals(baseId);
        }

        public async Task<IEnumerable<Delivery>> GetAllDeliveries(int baseId)
        {
            return await _scheduleRepository.GetAllDeliveries(baseId);
        }

        public async Task<IEnumerable<PickUp>> GetAllPickUps(int baseId)
        {
            return await _scheduleRepository.GetAllPickUps(baseId);
        }

        public async Task<IEnumerable<ServiceEntity>> GetAllServices(int baseId)
        {
            return await _scheduleRepository.GetAllServices(baseId);
        }
        public async Task<bool> BaseIdExist(int baseId)
        {
            return await _scheduleRepository.BaseIdExist(baseId);
        }


    }
}

