namespace nutrilifeAPI.Models
{
    public class Messages
    {
        public int Id { get; set; }
        public DateTime MessageDate { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string MessageContent { get; set; }
    }
}
