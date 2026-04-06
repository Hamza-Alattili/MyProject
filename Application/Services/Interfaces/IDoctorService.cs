using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync(string? city, int? categoryId);
        Task<DoctorDto> GetDoctorByIdAsync(int id);
        Task<DoctorDto> AddDoctorAsync(DoctorDto doctorDto);
        Task<DoctorDto> UpdateDoctorAsync(int id, DoctorDto doctorDto);
        Task<bool> DeleteDoctorAsync(int id);
    }
}
