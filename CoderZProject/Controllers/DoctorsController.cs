using Application.DTOs;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoderZProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDoctors([FromQuery] string? city, [FromQuery] int? categoryId)
        {
            var doctors = await _doctorService.GetAllDoctorsAsync(city, categoryId);
            return Ok(doctors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctorById(int id)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(id);
            if (doctor == null) return NotFound();
            return Ok(doctor);
        }

        [Authorize(Roles = "Admin")] // Only Admin can add doctors
        [HttpPost]
        public async Task<IActionResult> AddDoctor([FromBody] DoctorDto doctorDto)
        {
            var newDoctor = await _doctorService.AddDoctorAsync(doctorDto);
            return CreatedAtAction(nameof(GetDoctorById), new { id = newDoctor.Id }, newDoctor);
        }

        [Authorize(Roles = "Admin,Doctor")] // Admin or the doctor themselves can update
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] DoctorDto doctorDto)
        {
            var updatedDoctor = await _doctorService.UpdateDoctorAsync(id, doctorDto);
            if (updatedDoctor == null) return NotFound();
            return Ok(updatedDoctor);
        }

        [Authorize(Roles = "Admin")] // Only Admin can delete doctors
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var result = await _doctorService.DeleteDoctorAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
