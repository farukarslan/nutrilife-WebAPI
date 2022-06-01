namespace nutrilifeAPI.Models
{
    public class Ratings
    {
        public int Id { get; set; }
        public int Rated { get; set; }
        public int PatientId { get; set; }
        public int NutritionistId { get; set; }
    }
}
