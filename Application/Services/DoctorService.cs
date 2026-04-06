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
    public class DoctorService : IDoctorService
    {
        private readonly ProjectDbContext _context;

        public DoctorService(ProjectDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync(string? city, int? categoryId)
        {
            var query = _context.Doctors.Include(d => d.Clinic).AsNoTracking();

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(d => d.City == city);
            }

            // Assuming Category is linked to Doctor, or we need to join through Clinic if Clinic has Category
            // For now, let's assume a direct link or we'll refine this later if needed.
            // If Doctor entity doesn't have CategoryId directly, we'll skip filtering by category for now.
            // This highlights a potential missing relationship in the Domain layer if doctors are categorized directly.

            var doctors = await query.ToListAsync();

            return doctors.Select(d => new DoctorDto
            {
                Id = d.Id,
                Name = d.Name,
                Email = d.Email,
                Biography = d.Biography,
                Specialization = d.Specialization,
                ExperienceYears = d.ExperienceYears,
                City = d.City,
                ClinicId = d.ClinicId,
                ClinicName = d.Clinic?.Name // Null-conditional operator for safety
            });
        }

        public async Task<DoctorDto> GetDoctorByIdAsync(int id)
        {
            var doctor = await _context.Doctors.Include(d => d.Clinic).AsNoTracking()
                                     .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null) return null;

            return new DoctorDto
            {
                Id = doctor.Id,
                Name = doctor.Name,
                Email = doctor.Email,
                Biography = doctor.Biography,
                Specialization = doctor.Specialization,
                ExperienceYears = doctor.ExperienceYears,
                City = doctor.City,
                ClinicId = doctor.ClinicId,
                ClinicName = doctor.Clinic?.Name
            };
        }

        public async Task<DoctorDto> AddDoctorAsync(DoctorDto doctorDto)
        {
            var doctor = new Doctor
            {
                Name = doctorDto.Name,
                Email = doctorDto.Email,
                Biography = doctorDto.Biography,
                Specialization = doctorDto.Specialization,
                ExperienceYears = doctorDto.ExperienceYears,
                City = doctorDto.City,
                ClinicId = doctorDto.ClinicId
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            // Refresh ClinicName after saving if needed, or assume it's passed correctly
            doctorDto.Id = doctor.Id;
            doctorDto.ClinicName = (await _context.Clinics.FindAsync(doctor.ClinicId))?.Name;

            return doctorDto;
        }

        public async Task<DoctorDto> UpdateDoctorAsync(int id, DoctorDto doctorDto)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return null;

            doctor.Name = doctorDto.Name;
            doctor.Email = doctorDto.Email;
            doctor.Biography = doctorDto.Biography;
            doctor.Specialization = doctorDto.Specialization;
            doctor.ExperienceYears = doctorDto.ExperienceYears;
            doctor.City = doctorDto.City;
            doctor.ClinicId = doctorDto.ClinicId;

            await _context.SaveChangesAsync();

            doctorDto.ClinicName = (await _context.Clinics.FindAsync(doctor.ClinicId))?.Name;

            return doctorDto;
        }

        public async Task<bool> DeleteDoctorAsync(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return false;

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
