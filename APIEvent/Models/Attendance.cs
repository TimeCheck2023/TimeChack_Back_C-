namespace APIEvent.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public int EventId { get; set; }
        public string? DocumentNumber { get; set; }
        public string? UserEmail { get; set; }
        public string? EventName { get; internal set; }
        public string? EventDescription { get; internal set; }
        public DateTime EndDate { get; internal set; }
        public DateTime StartDate { get; internal set; }
        public string? Location { get; internal set; }
        public string? EventImage { get; internal set; }
        public int Capacity { get; internal set; }
        public string? EventType { get; internal set; }
        public double TotalValue { get; internal set; }
    }
}
