using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CoderZProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var token = await _authService.LoginAsync(loginDto);
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Invalid credentials");
            }
            return Ok(new { Token = token });
        }

        [HttpPost("register/patient")]
        public async Task<IActionResult> RegisterPatient([FromBody] RegisterDto registerDto)
        {
            var user = await _authService.RegisterAsync(registerDto, RoleEnum.Patient.ToString());
            if (user == null)
            {
                return BadRequest("Could not register patient");
            }
            return Ok(new { Message = "Patient registered successfully", UserId = user.Id });
        }

        [HttpPost("register/doctor")]
        public async Task<IActionResult> RegisterDoctor([FromBody] RegisterDto registerDto)
        {
            var user = await _authService.RegisterAsync(registerDto, RoleEnum.Doctor.ToString());
            if (user == null)
            {
                return BadRequest("Could not register doctor");
            }
            return Ok(new { Message = "Doctor registered successfully", UserId = user.Id });
        }
    }
}
