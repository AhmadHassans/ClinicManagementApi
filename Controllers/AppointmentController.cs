using ClinicApi.Data;
using ClinicApi.DTOs;
using ClinicApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppointmentController(AppDbContext context)
        {
            _context = context;
        }
        // only login user can see appointments
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAppointments()
        {
            var appointments = await _context.Appointments
                .Select(a => new {
                    a.Id,
                    a.PatientName,
                    a.Reason,
                    a.AppointmentDate,
                    a.Prescription
                })
                .ToListAsync();

            return Ok(appointments);
        }

       
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentDto request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("Error: Invalid token or user not found.");
            }

            var appointment = new Appointment
            {
                PatientName = request.PatientName,
                Reason = request.Reason,
                AppointmentDate = DateTime.UtcNow.AddDays(1),
                CreatedByUserId = userId
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Appointment created successfully!" });
        }

        // Prescription "Prescription.Write" permission 
        [HttpPut("{id}/prescription")]
        [Authorize]
        public async Task<IActionResult> WritePrescription(int id, [FromBody] PrescriptionDto request)
        {
            // Step 1: token se user ki Id
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("Error: Invalid token.");
            }

            // Step 2: user + role + permissions database fetch
            var user = await _context.Users
                .Include(u => u.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Unauthorized("Error: User not found.");
            }

            // Step 3: permission check with code
            bool hasPermission = user.Role.RolePermissions
                .Any(rp => rp.Permission.Code == "Prescription.Write");

            if (!hasPermission)
            {
                return Forbid(); // 403
            }

            // Step 4: appointment check
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound("Error: Appointment not found.");
            }

            // Step 5: prescription save
            appointment.Prescription = request.PrescriptionText;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Prescription successfully saved!" });
        }
    }

    public class AppointmentDto
    {
        public string PatientName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}