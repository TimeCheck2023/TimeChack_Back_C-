using APIEvent.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet.Core;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;



namespace APIEvent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly string cadenaSQL;
        public EventController(IConfiguration config)
        {
            cadenaSQL = config.GetConnectionString("CadenaSQL");
        }

        [HttpGet]
        [Route("Lista")]
        public IActionResult Lista()
        {
            List<Event> lista = new List<Event>();
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("USP_EVENTO_LISTAR", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lista.Add(new Event
                            {
                                id_evento = Convert.ToInt32(rd["id_evento"]),
                                nombre_evento = rd["nombre_evento"].ToString(),
                                fecha_creacion = Convert.ToDateTime(rd["fecha_creacion"]),
                                imagen = rd["imagen"].ToString(),
                                tipo_evento = rd["tipo_evento"].ToString(),
                                valor_evento = Convert.ToInt32(rd["valor_evento"]),
                                cedulaadmin1 = Convert.ToInt32(rd["cedulaadmin1"]),
                            });
                        }
                    }
                }
                return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok", response = lista });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message, response = lista });
            }
        }

        [HttpGet]
        [Route("Obtener/{id_evento:int}")]
        public IActionResult Obtener(int id_evento)
        {
            List<Event> lista = new List<Event>();
            Event oproducto = new Event();
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("USP_EVENTO_CONSULTAR", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lista.Add(new Event
                            {
                                id_evento = Convert.ToInt32(rd["id_evento"]),
                                nombre_evento = rd["nombre_evento"].ToString(),
                                fecha_creacion = Convert.ToDateTime(rd["fecha_creacion"]),
                                imagen = rd["imagen"].ToString(),
                                tipo_evento = rd["tipo_evento"].ToString(),
                                valor_evento = Convert.ToInt32(rd["valor_evento"]),
                                cedulaadmin1 = Convert.ToInt32(rd["cedulaadmin1"]),
                            });
                        }
                    }
                }
                oproducto = lista.Where(item => item.id_evento == id_evento).FirstOrDefault();
                return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok", response = oproducto });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message, response = oproducto });
            }
        }



        [HttpPost]
        [Route("Enviar")]
        public IActionResult GuardarEvento(string nombreEvento, string tipoEvento, float valorEvento, DateTime fechaEvento, string sitio, string descripcion, int aforo, float total, string cedulaAdmin, [FromForm] Microsoft.AspNetCore.Http.IFormFile imagen)
        {
            try
            {
                // Subir imagen a Cloudinary
                var cloudinary = new Cloudinary(new Account("centroconveciones", "626197298893936", "RIJ0WEIegehMqcFxdjJZ7xaV7W4"));
                var uploadResult = cloudinary.Upload(new ImageUploadParams
                {
                    File = new FileDescription(imagen.FileName, imagen.OpenReadStream())
                });

                // Guardar evento en la base de datos
                using (SqlConnection connection = new SqlConnection(cadenaSQL))
                {
                    connection.Open();

                    using (SqlCommand cmd = new SqlCommand("USP_EVENTO_INSERTAR", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@imagen", uploadResult.Url.ToString());
                        cmd.Parameters.AddWithValue("@nombre_evento", nombreEvento);
                        cmd.Parameters.AddWithValue("@tipo_evento", tipoEvento);
                        cmd.Parameters.AddWithValue("@valor_evento", valorEvento);
                        cmd.Parameters.AddWithValue("@fecha_evento", fechaEvento);
                        cmd.Parameters.AddWithValue("@sitio", sitio);
                        cmd.Parameters.AddWithValue("@descripcion", descripcion);
                        cmd.Parameters.AddWithValue("@aforo", aforo);
                        cmd.Parameters.AddWithValue("@total", total);
                        cmd.Parameters.AddWithValue("@cedulaadmin1", cedulaAdmin);

                        cmd.ExecuteNonQuery();

                        return Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



    }
}
