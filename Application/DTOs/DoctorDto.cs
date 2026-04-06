using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class DoctorDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Biography { get; set; }
        public string Specialization { get; set; }
        public int ExperienceYears { get; set; }
        public string City { get; set; }
        public int ClinicId { get; set; }
        public string ClinicName { get; set; }
    }
}
