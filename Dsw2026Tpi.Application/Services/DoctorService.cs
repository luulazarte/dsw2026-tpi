using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IPersistence _persistence;

    public DoctorService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<DoctorModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
    {
        if (!string.IsNullOrWhiteSpace(name) && (name.Length < 3 || name.Length > 100))
            throw new ValidationException("El nombre debe tener entre 3 y 100 caracteres.", nameof(ErrorCodes.VALIDATION_ERROR));

        var doctors = await _persistence.Paginate<Doctor, string>(
            pageSize,
            pageIndex,
            d => !d.Deleted && (string.IsNullOrWhiteSpace(name) || d.Name.Contains(name)),
            x => x.Name,
            nameof(Doctor.Speciality)
        );

        return doctors.Map(d => new DoctorModel.Response(d.Id, d.Name, d.LicenseNumber,
            new DoctorModel.SpecialityDto(d.Speciality?.Id, d.Speciality?.Name)));
    }

    public async Task<DoctorModel.Response> Create(DoctorModel.Request request)
    {
        ValidarDatos(request);

        
        var speciality = await _persistence.GetById<Speciality>(request.SpecialityId);
        if (speciality == null || speciality.Deleted)
            throw new ValidationException("La especialidad especificada no existe.", nameof(ErrorCodes.VALIDATION_ERROR));

        var doctor = new Doctor(request.Name, request.LicenseNumber, request.SpecialityId);
        await _persistence.Add(doctor);

        return new DoctorModel.Response(doctor.Id, doctor.Name, doctor.LicenseNumber,
            new DoctorModel.SpecialityDto(speciality.Id, speciality.Name));
    }

    public async Task<DoctorModel.Response> Update(Guid id, DoctorModel.Request request)
    {
        ValidarDatos(request);

        var doctor = await _persistence.GetById<Doctor>(id);
        if (doctor == null || doctor.Deleted)
            throw new EntityNotFoundException("Doctor");

        var speciality = await _persistence.GetById<Speciality>(request.SpecialityId);
        if (speciality == null || speciality.Deleted)
            throw new ValidationException("La especialidad especificada no existe.", nameof(ErrorCodes.VALIDATION_ERROR));

        
        doctor.Update(request.Name, request.LicenseNumber, request.SpecialityId);
        await _persistence.Update(doctor);

        return new DoctorModel.Response(doctor.Id, doctor.Name, doctor.LicenseNumber,
            new DoctorModel.SpecialityDto(speciality.Id, speciality.Name));
    }

    public async Task Delete(Guid id)
    {
        var doctor = await _persistence.GetById<Doctor>(id);
        if (doctor == null || doctor.Deleted)
            throw new EntityNotFoundException("Doctor");

        await _persistence.Delete(doctor); 
    }

    public async Task<List<AvailabilityModel.Response>> GetAvailabilities(Guid doctorId)
    {
        var doctor = await _persistence.GetById<Doctor>(doctorId);
        if (doctor == null || doctor.Deleted)
            throw new EntityNotFoundException("Doctor");

        var hoy = DateTime.Today;

        
        var slots = await _persistence.GetFiltered<AvailabilitySlot>(
            s => s.AvailabilityRule.DoctorId == doctorId
                 && s.SlotDate.Month == hoy.Month
                 && s.SlotDate.Year == hoy.Year,
            "AvailabilityRule"
        );

        if (slots == null || !slots.Any())
            return new List<AvailabilityModel.Response>(); 

        return slots.Select(s => new AvailabilityModel.Response(
            s.Id,
            s.AvailabilityRuleId,
            s.SlotDate,
            s.StartTime,
            s.EndTime,
            s.Status
        )).ToList();
    }

    
    private static void ValidarDatos(DoctorModel.Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3 || request.Name.Length > 100)
            throw new ValidationException("El nombre es obligatorio y debe tener entre 3 y 100 caracteres.", nameof(ErrorCodes.VALIDATION_ERROR));

        if (string.IsNullOrWhiteSpace(request.LicenseNumber))
            throw new ValidationException("El número de licencia es obligatorio.", nameof(ErrorCodes.VALIDATION_ERROR));
    }
}