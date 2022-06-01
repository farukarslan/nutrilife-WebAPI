namespace nutrilifeAPI.Models
{
    public class Recipes
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Materials { get; set; }
        public string Explanation { get; set; }
        public int NutritionistId { get; set; }
        public string Picture { get; set; }
    }
}
