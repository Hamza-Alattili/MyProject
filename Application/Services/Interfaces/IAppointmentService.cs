using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentDto> BookAppointmentAsync(AppointmentDto appointmentDto);
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientIdAsync(int patientId);
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByDoctorIdAsync(int doctorId);
        Task<AppointmentDto> UpdateAppointmentStatusAsync(int appointmentId, string status);
        Task<bool> CancelAppointmentAsync(int appointmentId);
    }
}
