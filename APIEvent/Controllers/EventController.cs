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
        private readonly string cloudName;
        private readonly string apiKey;
        private readonly string apiSecret;

        public EventController(IConfiguration config)
        {
            //Cadena de conexion a la DB desde el archivo appsettings.json
            cadenaSQL = config.GetConnectionString("CadenaSQL");

            //Variable del archivo appsettings.json que contiene CloudName, ApiKey, ApiSecret de Cloudinary
            var cloudinarySettings = config.GetSection("CloudinarySettings");
            cloudName = cloudinarySettings.GetValue<string>("CloudName");
            apiKey = cloudinarySettings.GetValue<string>("ApiKey");
            apiSecret = cloudinarySettings.GetValue<string>("ApiSecret");
        }

        //Endpoint para listar los eventos
        [HttpGet]
        [Route("Lista")]
        public IActionResult Lista()
        {
            List<Event> lista = new List<Event>();
            try
            {
                //Se conecta la DB usando SqlConnection y la cadena de conexion
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    //Se abre la conexion a la DB
                    conexion.Open();
                    //Se ejecuta el procedimiento USP_EVENTO_LISTAR
                    var cmd = new SqlCommand("USP_EVENTO_LISTAR", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lista.Add(new Event
                            {
                                //Se le asgina los campos respectivos
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
                //Retorna un mensaje "ok" si sale todo bien y un estado 200
                return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok", response = lista });
            }
            catch (Exception error)
            {
                //Retorna un mensaje de error si salió algo malo y un estado 500
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message, response = lista });
            }
        }



        //Endpoint para obtener un solo evento
        [HttpGet]
        [Route("Obtener/{id_evento:int}")]
        //Se le pide un parametro el cual es el id del evento
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



        //Endpoint para guardar un nuevo evento en la DB
        [HttpPost]
        [Route("Enviar")]
        public IActionResult GuardarEvento(string nombreEvento, string categoria, float valorEvento, DateTime fechaEvento, string sitio, string descripcion, int aforo, float valorTotal, long cedulaAdmin, string imagen)
        {
            try
            {
                // Subir imagen a Cloudinary
                var cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
                var uploadResult = cloudinary.Upload(new ImageUploadParams
                {
                    PublicId = nombreEvento,
                    File = new FileDescription(imagen)
                });

                // Guardar evento en la base de datos
                using (SqlConnection connection = new SqlConnection(cadenaSQL))
                {
                    connection.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_INSERT_EVENTO", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Se agregan los parámetros necesarios para insertar un evento en la tabla correspondiente
                        cmd.Parameters.AddWithValue("@nombre_evento", nombreEvento);
                        cmd.Parameters.AddWithValue("@imagen", uploadResult.Url.ToString());
                        cmd.Parameters.AddWithValue("@valor_evento", valorEvento);
                        cmd.Parameters.AddWithValue("@categoria", categoria);
                        cmd.Parameters.AddWithValue("@id_tipo_evento2", 1); //No se está usando
                        cmd.Parameters.AddWithValue("@id_subconju_cedula_org2", 1); //No se está usando
                        cmd.Parameters.AddWithValue("@id_Gestion_evento2", 1); //No se está usando
                        cmd.Parameters.AddWithValue("@valor_total", valorTotal);
                        cmd.Parameters.AddWithValue("@fecha", fechaEvento);
                        cmd.Parameters.AddWithValue("@sitio", sitio);
                        cmd.Parameters.AddWithValue("@descripcion", descripcion);
                        cmd.Parameters.AddWithValue("@aforo", aforo);
                        cmd.Parameters.AddWithValue("@total", valorTotal);
                        cmd.Parameters.AddWithValue("@cedula2", cedulaAdmin);

                        //SqlParameter respuesta = new SqlParameter("@respuesta", SqlDbType.VarChar, 50);
                        //respuesta.Direction = ParameterDirection.Output;
                        //cmd.Parameters.Add(respuesta);

                        cmd.ExecuteNonQuery();

                        // Si todo ha ido bien, retorna un objeto Ok()
                        return Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                // Si ha habido un error, retorna un objeto BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }
        }



    }
}
