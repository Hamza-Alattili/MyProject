using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoderZProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All appointment endpoints require authentication
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpPost("book")]
        [Authorize(Roles = "Patient")] // Only patients can book appointments
        public async Task<IActionResult> BookAppointment([FromBody] AppointmentDto appointmentDto)
        {
            // Get patient ID from token
            var patientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (patientIdClaim == null || !int.TryParse(patientIdClaim.Value, out int patientId))
            {
                return Unauthorized("Invalid patient ID in token.");
            }
            appointmentDto.PatientId = patientId;

            try
            {
                var newAppointment = await _appointmentService.BookAppointmentAsync(appointmentDto);
                return CreatedAtAction(nameof(GetPatientAppointments), new { patientId = newAppointment.PatientId }, newAppointment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "Patient,Admin")] // Patients can see their own, Admin can see all
        public async Task<IActionResult> GetPatientAppointments(int patientId)
        {
            // Ensure patient can only view their own appointments unless they are Admin
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
            {
                return Unauthorized("Invalid user ID in token.");
            }

            if (currentUserId != patientId && !User.IsInRole("Admin"))
            {
                return Forbid("You can only view your own appointments.");
            }

            var appointments = await _appointmentService.GetAppointmentsByPatientIdAsync(patientId);
            return Ok(appointments);
        }

        [HttpGet("doctor/{doctorId}")]
        [Authorize(Roles = "Doctor,Admin")] // Doctors can see their own, Admin can see all
        public async Task<IActionResult> GetDoctorAppointments(int doctorId)
        {
            // Ensure doctor can only view their own appointments unless they are Admin
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
            {
                return Unauthorized("Invalid user ID in token.");
            }

            // If user is Admin, they can see everything
            if (User.IsInRole("Admin"))
            {
                var adminAppointments = await _appointmentService.GetAppointmentsByDoctorIdAsync(doctorId);
                return Ok(adminAppointments);
            }

            // If user is a Doctor, we must ensure they are requesting their own ID
            // In a real system, we'd look up the Doctor entity by UserId to get the correct DoctorId
            // For now, we'll keep the logic but note the need for a Doctor-User mapping check if DoctorId != UserId
            if (currentUserId != doctorId)
            {
                return Forbid("You can only view your own appointments.");
            }

            var appointments = await _appointmentService.GetAppointmentsByDoctorIdAsync(doctorId);
            return Ok(appointments);
        }

        [HttpPut("{appointmentId}/status")]
        [Authorize(Roles = "Doctor,Admin")] // Doctors or Admin can update status
        public async Task<IActionResult> UpdateAppointmentStatus(int appointmentId, [FromQuery] string status)
        {
            var updatedAppointment = await _appointmentService.UpdateAppointmentStatusAsync(appointmentId, status);
            if (updatedAppointment == null) return NotFound();
            return Ok(updatedAppointment);
        }

        [HttpDelete("{appointmentId}")]
        [Authorize(Roles = "Patient,Admin")] // Patients can cancel their own, Admin can cancel any
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            // Further logic needed to ensure patient can only cancel their own appointment
            var result = await _appointmentService.CancelAppointmentAsync(appointmentId);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
