namespace nutrilifeAPI.Models
{
    public class Nutritionists
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Mail { get; set; }
        public string Password { get; set; }
        public string Gender { get; set; }
        public string? CV { get; set; }
        public string? Degree { get; set; }
        public double Point { get; set; }
        public bool WeightManagement { get; set; }
        public bool SportsNutrition { get; set; }
        public bool PregnancyNutrition { get; set; }
        public bool DiabeticNutrition { get; set; }
        public bool ChildNutrition { get; set; }
        public string? ProfilePicture { get; set; }
        public string PhoneNumber { get; set; }
    }
}
