using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IAvailabilityService
    {
        Task<Pagination<AvailabilityModel.Response>> GetAll(int pageSize, int pageIndex, Guid? ruleId = null);
        Task<AvailabilityModel.Response> Create(AvailabilityModel.Request request);
        Task Delete(Guid id);
    }
}
