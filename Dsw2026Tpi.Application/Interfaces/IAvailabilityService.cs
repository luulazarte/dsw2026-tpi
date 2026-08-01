using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IAvailabilityService
    {

        Task<List<AvailabilityModel.Response>> Create(AvailabilityModel.Request request);
        Task<List<AvailabilityModel.Response>> Update(AvailabilityModel.Request request);
    }
}
