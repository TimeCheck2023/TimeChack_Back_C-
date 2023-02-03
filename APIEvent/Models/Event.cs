namespace APIEvent.Models
{
    public class Event
    {
        public int id_event { get; set; }
        public string? name_event { get; set; }
        public int cost_event { get; set; }
        public DateTime date_event { get; set; }
        public string? location_event { get; set; }
        public string? description_event { get; set; }
        public int capacity_event { get; set; }
        public int id_admin { get; set; }

    }
}
