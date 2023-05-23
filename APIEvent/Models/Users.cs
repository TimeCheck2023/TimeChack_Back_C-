namespace APIEvent.Models
{
    //idUsuario = Convert.ToInt32(reader["id_usuario"]),
    //                        nombreUsuario = reader["nombre_usuario"].ToString(),
    //                        emailUsuario = reader["email_usuario"].ToString()
    public class Users
    {
        public string idUsuario { get; set; }
        public string? nombreUsuario { get; set; }
        public string? emailUsuario { get; set; }
    }
}
