namespace APIEvent.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public int EventId { get; set; }
        public string? DocumentNumber { get; set; }
    }
}
