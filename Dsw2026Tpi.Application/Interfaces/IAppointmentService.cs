using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<Pagination<AppointmentModel.Response>> GetAll(int pageSize, int pageIndex, Guid? patientId = null);
        Task<AppointmentModel.Response> Create(AppointmentModel.Request request);
        Task Cancel(Guid id);
    }
}
