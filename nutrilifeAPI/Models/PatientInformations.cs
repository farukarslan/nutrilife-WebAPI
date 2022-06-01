namespace nutrilifeAPI.Models
{
    public class PatientInformations
    {
        public int Id { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public double Size { get; set; }
        public double Weight { get; set; }
        public string WeightStatus { get; set; }
        public string? ChronicDiseases { get; set; }
        public string? BloodValues { get; set; }
        public string? AnthropometricMeasurement { get; set; }
        public string? FatMuscleMeasurement { get; set; }
        public double? BodyMassIndex { get; set; }
        public bool? SmokingStatus { get; set; }
        public bool? AlcoholStatus { get; set; }
        public int? PatientId { get; set; }
    }
}
