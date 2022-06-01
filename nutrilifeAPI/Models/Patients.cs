namespace nutrilifeAPI.Models
{
    public class Patients
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Mail { get; set; }
        public string Password { get; set; }
        public int? InfoId { get; set; }
        public string PhoneNumber { get; set; }
    }
}
