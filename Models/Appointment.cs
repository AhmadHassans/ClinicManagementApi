namespace ClinicApi.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        
        // Prescription sirf Doctor edit kar sakega
        public string? Prescription { get; set; } 
        public DateTime AppointmentDate { get; set; } = DateTime.UtcNow;

        // Kis ne yeh entry banayi
        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;
    }
}