namespace APIEvent.Models
{
    public class Event
    {
        public int IdEvento { get; set; }
        public string NombreEvento { get; set; }
        public string DescripcionEvento { get; set; }
        public string ImagenEvento { get; set; }
        public DateTime FechaInicioEvento { get; set; }
        public DateTime FechaFinalEvento { get; set; }
        public DateTime FechaCreacionEvento { get; set; }
        public string LugarEvento { get; set; }
        public int AforoEvento { get; set; }
        public decimal ValorEvento { get; set; }
        public decimal Iva { get; set; }
        public decimal ValorTotalEvento { get; set; }
        public int IdSuborganizacion { get; set; }
        public int IdTipoEvento { get; set; }
        public string TipoEvento { get; set; }

    }
}
