using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IPersistence _persistence;

        public AvailabilityService(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public async Task<Pagination<AvailabilityModel.Response>> GetAll(int pageSize, int pageIndex, Guid? ruleId = null)
        {
            var list = await _persistence.Paginate<AvailabilitySlot, string>(
                pageSize,
                pageIndex,
                a => !ruleId.HasValue || a.AvailabilityRuleId == ruleId.Value,
                x => x.SlotDate.ToString()
            );

            return list.Map(a => new AvailabilityModel.Response(a.Id, a.AvailabilityRuleId, a.SlotDate, a.StartTime, a.EndTime, a.Status));
        }

        public async Task<AvailabilityModel.Response> Create(AvailabilityModel.Request request)
        {
          
            var slot = new AvailabilitySlot(request.AvailabilityRuleId, request.SlotDate, request.StartTime, request.EndTime, Domain.Enums.SlotStatus.AVAILABLE, Guid.NewGuid());

            await _persistence.Add(slot);

            return new AvailabilityModel.Response(slot.Id, slot.AvailabilityRuleId, slot.SlotDate, slot.StartTime, slot.EndTime, slot.Status);
        }

        public async Task Delete(Guid id)
        {
            var slot = await _persistence.GetById<AvailabilitySlot>(id);
            if (slot != null)
            {
                await _persistence.Delete(slot);
            }
        }
    }
}
