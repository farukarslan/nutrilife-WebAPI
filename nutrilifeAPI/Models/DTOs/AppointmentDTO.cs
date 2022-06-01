namespace nutrilifeAPI.Models.DTOs
{
    public class AppointmentDTO
    {
        public DateTime AppointmentDate { get; set; }
        public string? Note { get; set; } = "";
        public string NutritionistName { get; set; }
        public string PatientName { get; set; }
    }
}
