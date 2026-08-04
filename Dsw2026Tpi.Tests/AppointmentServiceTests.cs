using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Enums;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Dsw2026Tpi.Tests;

public class AppointmentServiceTests
{
    private readonly IPersistence _persistence = Substitute.For<IPersistence>();
    private readonly ILogger<AppointmentService> _logger = Substitute.For<ILogger<AppointmentService>>();

    [Fact]
    public async Task Create_CuandoElTurnoEsEnElPasado_EntoncesLanzaExcepcion()
    {
        var doctorId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        var doctor = new Doctor("Dr. Test", "MED-001", Guid.NewGuid(), doctorId);
        var patient = new Patient(Guid.NewGuid(), "40123456", "Juan Test");

        var slotPasado = new AvailabilitySlot(Guid.NewGuid(), DateTime.Today.AddDays(-1),
            new TimeSpan(9, 0, 0), new TimeSpan(9, 30, 0), SlotStatus.AVAILABLE, slotId);

        _persistence.GetById<Doctor>(doctorId).Returns(doctor);
        _persistence.First<Patient>(Arg.Any<Expression<Func<Patient, bool>>>()).Returns(patient);
        _persistence.GetById<AvailabilitySlot>(slotId).Returns(slotPasado);

        var service = new AppointmentService(_persistence, _logger);
        var request = new AppointmentModel.Request(
            doctorId, slotId, new AppointmentModel.PatientDto(40123456), "Control de rutina");

        await Assert.ThrowsAnyAsync<Exception>(() => service.Create(request));
    }

    [Fact]
    public async Task Create_CuandoElSlotYaEstaReservado_EntoncesLanzaExcepcion()
    {
        var doctorId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        var doctor = new Doctor("Dr. Test", "MED-001", Guid.NewGuid(), doctorId);
        var patient = new Patient(Guid.NewGuid(), "40123456", "Juan Test");

        var slotOcupado = new AvailabilitySlot(Guid.NewGuid(), DateTime.Today.AddDays(5),
            new TimeSpan(9, 0, 0), new TimeSpan(9, 30, 0), SlotStatus.BOOKED, slotId);

        _persistence.GetById<Doctor>(doctorId).Returns(doctor);
        _persistence.First<Patient>(Arg.Any<Expression<Func<Patient, bool>>>()).Returns(patient);
        _persistence.GetById<AvailabilitySlot>(slotId).Returns(slotOcupado);

        var service = new AppointmentService(_persistence, _logger);
        var request = new AppointmentModel.Request(
            doctorId, slotId, new AppointmentModel.PatientDto(40123456), "Control de rutina");

        await Assert.ThrowsAnyAsync<Exception>(() => service.Create(request));
    }

    [Fact]
    public async Task Create_CuandoLosDatosSonValidos_EntoncesGuardaElTurno()
    {
        var doctorId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        var doctor = new Doctor("Dr. Test", "MED-001", Guid.NewGuid(), doctorId);
        var patient = new Patient(Guid.NewGuid(), "40123456", "Juan Test");

        var slotOk = new AvailabilitySlot(Guid.NewGuid(), DateTime.Today.AddDays(5),
            new TimeSpan(9, 0, 0), new TimeSpan(9, 30, 0), SlotStatus.AVAILABLE, slotId);

        _persistence.GetById<Doctor>(Arg.Any<Guid>()).ReturnsForAnyArgs(doctor);
        _persistence.First<Patient>(Arg.Any<Expression<Func<Patient, bool>>>()).ReturnsForAnyArgs(patient);
        _persistence.GetById<AvailabilitySlot>(Arg.Any<Guid>()).ReturnsForAnyArgs(slotOk);

        var service = new AppointmentService(_persistence, _logger);
        var request = new AppointmentModel.Request(
            doctorId, slotId, new AppointmentModel.PatientDto(40123456), "Control de rutina");

        await service.Create(request);

        await _persistence.Received().Add(Arg.Any<Appointment>());
    }

    [Fact]
    public async Task Create_CuandoElDoctorNoExiste_EntoncesLanzaExcepcion()
    {
        var doctorId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        Doctor? doctorInexistente = null;
        _persistence.GetById<Doctor>(doctorId).Returns(doctorInexistente);

        var service = new AppointmentService(_persistence, _logger);
        var request = new AppointmentModel.Request(
            doctorId, slotId, new AppointmentModel.PatientDto(40123456), "Control de rutina");

        await Assert.ThrowsAnyAsync<Exception>(() => service.Create(request));
    }
}