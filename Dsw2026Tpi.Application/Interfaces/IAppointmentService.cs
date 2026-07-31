using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task Create(AppointmentModel.Request request);
        Task Cancel(Guid id);
        Task<IEnumerable<AppointmentModel.PatientResponse>> GetByPatientDni(long dni);
        Task<IEnumerable<AppointmentModel.SearchResponse>> GetAppointmentsByDate(DateTime date);
        Task<Pagination<AppointmentModel.SearchResponse>> SearchAppointments(Guid? specialtyId, Guid? doctorId, long? dni, DateTime? date, int pageSize, int pageIndex);
    }
}