namespace APIEvent.Models
{
    public class Event
    {
        public int? id_evento { get; set; }
        public string? nombre_evento { get; set; }
        public DateTime? fecha_creacion { get; set; }
        public string? imagen { get; set; }
        public string? tipo_evento { get; set; }
        public float? valor_evento { get; set; }
        public int? cedulaadmin1 { get; set; }

    }
}
