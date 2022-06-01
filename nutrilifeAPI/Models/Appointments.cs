namespace nutrilifeAPI.Models
{
    public class Appointments
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Note { get; set; }
        public bool Status { get; set; } = true;
        public int NutritionistId { get; set; }
        public int PatientId { get; set; }

    }
}
