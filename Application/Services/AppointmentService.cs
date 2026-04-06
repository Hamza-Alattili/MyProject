using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ProjectDbContext _context;

        public AppointmentService(ProjectDbContext context)
        {
            _context = context;
        }

        public async Task<AppointmentDto> BookAppointmentAsync(AppointmentDto appointmentDto)
        {
            // Check for doctor availability and conflicts
            var doctorScheduleConflict = await _context.Appointments
                .AnyAsync(a => a.DoctorId == appointmentDto.DoctorId &&
                               a.AppointmentDate == appointmentDto.AppointmentDate &&
                               ((a.StartTime < appointmentDto.EndTime && a.EndTime > appointmentDto.StartTime) ||
                                (appointmentDto.StartTime < a.EndTime && appointmentDto.EndTime > a.StartTime)));

            if (doctorScheduleConflict)
            {
                throw new InvalidOperationException("Doctor is not available at the requested time.");
            }

            var appointment = new Appointment
            {
                PatientId = appointmentDto.PatientId,
                DoctorId = appointmentDto.DoctorId,
                AppointmentDate = appointmentDto.AppointmentDate,
                StartTime = appointmentDto.StartTime,
                EndTime = appointmentDto.EndTime,
                Status = "Pending", // Default status
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Populate DTO with names for response
            var patient = await _context.Users.FindAsync(appointment.PatientId);
            var doctor = await _context.Doctors.FindAsync(appointment.DoctorId);

            appointmentDto.Id = appointment.Id;
            appointmentDto.PatientName = patient?.Name;
            appointmentDto.DoctorName = doctor?.Name;
            appointmentDto.Status = appointment.Status;
            appointmentDto.CreatedAt = appointment.CreatedAt;

            return appointmentDto;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientIdAsync(int patientId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .AsNoTracking()
                .ToListAsync();

            return appointments.Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientName = a.Patient?.Name,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor?.Name,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                CreatedAt = a.CreatedAt
            });
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByDoctorIdAsync(int doctorId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .AsNoTracking()
                .ToListAsync();

            return appointments.Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientName = a.Patient?.Name,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor?.Name,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                CreatedAt = a.CreatedAt
            });
        }

        public async Task<AppointmentDto> UpdateAppointmentStatusAsync(int appointmentId, string status)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return null;

            appointment.Status = status;
            await _context.SaveChangesAsync();

            var patient = await _context.Users.FindAsync(appointment.PatientId);
            var doctor = await _context.Doctors.FindAsync(appointment.DoctorId);

            return new AppointmentDto
            {
                Id = appointment.Id,
                PatientId = appointment.PatientId,
                PatientName = patient?.Name,
                DoctorId = appointment.DoctorId,
                DoctorName = doctor?.Name,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status,
                CreatedAt = appointment.CreatedAt
            };
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return false;

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

