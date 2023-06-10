using APIEvent.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet.Core;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore;
using System.Reflection;
using Swashbuckle.AspNetCore.Annotations;




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


        /// <summary>
        /// Lista los eventos
        /// </summary>
        /// <returns>Devuelve una lista de eventos</returns>
        //Endpoint para listar los eventos
        [HttpGet]
        [Route("List")]
        [SwaggerOperation(Summary = "Lista los eventos")]

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
                    var cmd = new SqlCommand("USP_ListarEventos", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lista.Add(new Event
                            {
                                //Se le asgina los campos respectivos
                                IdEvento = Convert.ToInt32(rd["id_evento"]),
                                NombreEvento = rd["nombre_evento"].ToString(),
                                DescripcionEvento = rd["descripcion_evento"].ToString(),
                                ImagenEvento = rd["imagen_evento"].ToString(),
                                FechaInicioEvento = Convert.ToDateTime(rd["fecha_inicio_evento"]),
                                FechaFinalEvento = Convert.ToDateTime(rd["fecha_final_evento"]),
                                FechaCreacionEvento = Convert.ToDateTime(rd["fecha_creacion_evento"]),
                                LugarEvento = rd["lugar_evento"].ToString(),
                                AforoEvento = Convert.ToInt32(rd["aforo_evento"]),
                                ValorEvento = Convert.ToDecimal(rd["valor_evento"]),
                                Iva = Convert.ToDecimal(rd["iva"]),
                                ValorTotalEvento = Convert.ToDecimal(rd["valor_total_evento"]),
                                IdSuborganizacion = Convert.ToInt32(rd["id_suborganización1"]),
                                TipoEvento = rd["tipo_evento"].ToString(),
                                CuposDisponibles = Convert.ToInt32(rd["cupos_disponibles"]),
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



        /// <summary>
        /// Obtiene un evento por su ID
        /// </summary>
        /// <param name="IdEvento">ID del evento</param>
        /// <returns>Devuelve un evento</returns>
        //Endpoint para obtener un solo evento
        [HttpGet]
        [Route("Consult/{IdEvento:int}")]
        //Se le pide un parametro el cual es el id del evento
        [SwaggerOperation(Summary = "Obtiene un evento por su ID")]
        public IActionResult Obtener(int IdEvento)
        {
            List<Event> lista = new List<Event>();
            Event oproducto = new Event();
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("USP_ListarEventos", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lista.Add(new Event
                            {
                                IdEvento = Convert.ToInt32(rd["id_evento"]),
                                NombreEvento = rd["nombre_evento"].ToString(),
                                DescripcionEvento = rd["descripcion_evento"].ToString(),
                                ImagenEvento = rd["imagen_evento"].ToString(),
                                FechaInicioEvento = Convert.ToDateTime(rd["fecha_inicio_evento"]),
                                FechaFinalEvento = Convert.ToDateTime(rd["fecha_final_evento"]),
                                FechaCreacionEvento = Convert.ToDateTime(rd["fecha_creacion_evento"]),
                                LugarEvento = rd["lugar_evento"].ToString(),
                                AforoEvento = Convert.ToInt32(rd["aforo_evento"]),
                                ValorEvento = Convert.ToDecimal(rd["valor_evento"]),
                                Iva = Convert.ToDecimal(rd["iva"]),
                                ValorTotalEvento = Convert.ToDecimal(rd["valor_total_evento"]),
                                IdSuborganizacion = Convert.ToInt32(rd["id_suborganización1"]),
                                TipoEvento = rd["tipo_evento"].ToString(),
                                CuposDisponibles = Convert.ToInt32(rd["cupos_disponibles"])
                            });
                        }
                    }
                }
                oproducto = lista.Where(item => item.IdEvento == IdEvento).FirstOrDefault();
                return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok", response = oproducto });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message, response = oproducto });
            }
        }




        /// <summary>
        /// Guarda un nuevo evento en la DB
        /// </summary>
        /// <param name="nombreEvento">Nombre del evento</param>
        /// <param name="descripcion">Descripción del evento</param>
        /// <param name="imagen">Url de la imagen del evento desde Cloudinary</param>
        /// <param name="fecha_inicio">Fecha de inicio del evento</param>
        /// <param name="fecha_final">Fecha del final del evento</param>
        /// <param name="lugar">Lugar del evento</param>
        /// <param name="aforo">Capcacidad de personas para el evento</param>
        /// <param name="valor_total">Precio del evento</param>
        /// <param name="id_suborganizacion">ID de la suborganización que creó el evento</param>
        /// <param name="id_tipo_evento">ID del tipo de evento o categoria</param>
        /// <returns>
        /// Retorna un objeto de tipo IActionResult que indica el resultado de la operación.
        /// Si el evento se guarda correctamente, devuelve un objeto Ok() con el estado 200.
        /// Si hay errores en los parámetros proporcionados, devuelve un objeto BadRequest con el estado 400 y un mensaje de error.
        /// Si hay un error interno en el servidor, devuelve un objeto BadRequest con el estado 500 y un mensaje de error.
        /// </returns>

        //Endpoint para guardar un nuevo evento en la DB
        [HttpPost]
        [Route("Send")]
        [SwaggerOperation(Summary = "Guarda un nuevo evento en la base de datos")]
        [SwaggerResponse(StatusCodes.Status200OK, "El evento se guardó correctamente")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Error en los parámetros proporcionados")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Error interno del servidor")]
        public IActionResult GuardarEvento(string nombreEvento, string descripcion, string imagen, DateTime fecha_inicio, DateTime fecha_final, string lugar, int aforo, int valor_total, int id_suborganizacion, int id_tipo_evento)
        {
            try
            {

                // Validar la longitud de la descripción
                if (descripcion.Length > 150)
                {
                    return BadRequest("La descripción no puede exceder los 150 caracteres.");
                }

                // Validar las fechas
                DateTime fechaActual = DateTime.Now;

                if (fecha_inicio < fechaActual)
                {
                    return BadRequest("La fecha de inicio no puede ser anterior a la fecha actual.");
                }

                if (fecha_final < fecha_inicio)
                {
                    return BadRequest("La fecha final no puede ser anterior a la fecha de inicio.");
                }

                // Validar el aforo
                if (aforo < 0)
                {
                    return BadRequest("El valor de la cantidad de personas no puede ser negativo!");
                }

                // Guardar evento en la base de datos
                using (SqlConnection connection = new SqlConnection(cadenaSQL))
                {
                    connection.Open();

                    using (SqlCommand cmd = new SqlCommand("USP_AgregarEvento", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Se agregan los parámetros necesarios para insertar un evento en la tabla correspondiente
                        cmd.Parameters.AddWithValue("@nombre", nombreEvento);
                        cmd.Parameters.AddWithValue("@descripcion", descripcion);
                        cmd.Parameters.AddWithValue("@imagen", imagen);
                        cmd.Parameters.AddWithValue("@fecha_inicio", fecha_inicio);
                        cmd.Parameters.AddWithValue("@fecha_final", fecha_final);
                        cmd.Parameters.AddWithValue("@lugar", lugar);
                        cmd.Parameters.AddWithValue("@aforo", aforo);
                        cmd.Parameters.AddWithValue("@valor", 0);//No se esta usando por el momento por lo cual se asigna 0
                        cmd.Parameters.AddWithValue("@iva", 0);//No se esta usando por el momento por lo cual se asigna 0
                        cmd.Parameters.AddWithValue("@valor_total", valor_total);
                        cmd.Parameters.AddWithValue("@id_suborganizacion", id_suborganizacion);
                        cmd.Parameters.AddWithValue("@id_tipo_evento", id_tipo_evento);


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

        /// <summary>
        /// Actualiza los campos de un evento por su ID
        /// </summary>
        /// <param name="IdEvento"></param>
        /// <param name="nombreEvento">Nombre del evento</param>
        /// <param name="descripcion">Descripción del evento</param>
        /// <param name="imagen">Url de la imagen del evento desde Cloudinary</param>
        /// <param name="fecha_inicio">Fecha de inicio del evento</param>
        /// <param name="fecha_final">Fecha del final del evento</param>
        /// <param name="lugar">Lugar del evento</param>
        /// <param name="aforo">Capcacidad de personas para el evento</param>
        /// <param name="id_tipo_evento">ID del tipo de evento o categoria</param>
        /// <returns>
        /// Retorna un objeto de tipo IActionResult que indica el resultado de la operación.
        /// Si el evento se actualiza correctamente, devuelve un objeto StatusCode con el estado 200 y un mensaje de éxito.
        /// Si hay errores en los parámetros proporcionados, devuelve un objeto BadRequest con el estado 400 y un mensaje de error.
        /// Si hay un error interno en el servidor, devuelve un objeto StatusCode con el estado 500 y un mensaje de error.
        /// </returns>

        // Endpoint para actualizar un evento en la DB
        [HttpPut]
        [Route("Update/{IdEvento:int}")]
        public IActionResult ActualizarEvento(int IdEvento, string nombreEvento, string descripcion, string imagen, DateTime fecha_inicio, DateTime fecha_final, string lugar, int aforo, int valor_total, int id_tipo_evento)
        {
            try
            {
                // Validar la longitud de la descripción
                if (descripcion.Length > 150)
                {
                    return BadRequest("La descripción no puede exceder los 150 caracteres.");
                }

                // Validar las fechas
                DateTime fechaActual = DateTime.Now;

                if (fecha_inicio < fechaActual)
                {
                    return BadRequest("La fecha de inicio no puede ser anterior a la fecha actual.");
                }

                if (fecha_final < fecha_inicio)
                {
                    return BadRequest("La fecha final no puede ser anterior a la fecha de inicio.");
                }

                // Validar el aforo
                if (aforo < 0)
                {
                    return BadRequest("El valor de la cantidad de personas no puede ser negativo.");
                }

                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();

                    // Obtener la imagen actual del evento
                    string imagenActual = "";
                    using (var obtenerImagenCmd = new SqlCommand("USP_ObtenerImagenEvento", conexion))
                    {
                        obtenerImagenCmd.CommandType = CommandType.StoredProcedure;
                        obtenerImagenCmd.Parameters.AddWithValue("@id_evento", IdEvento);
                        using (var reader = obtenerImagenCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                imagenActual = reader["imagen_evento"].ToString();
                            }
                        }
                    }

                    // Si se proporciona una nueva imagen, eliminar la anterior
                    if (!string.IsNullOrEmpty(imagen))
                    {
                        var cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
                        var publicId = Path.GetFileNameWithoutExtension(imagenActual);
                        var deletionParams = new DeletionParams(publicId) { ResourceType = ResourceType.Image };
                        cloudinary.Destroy(deletionParams);
                    }
                    else
                    {
                        // Si no se proporciona una nueva imagen, utilizar la imagen actual
                        imagen = imagenActual;
                    }

                    // Actualizar el evento en la base de datos
                    using (var actualizarEventoCmd = new SqlCommand("USP_EditarEvento", conexion))
                    {
                        actualizarEventoCmd.CommandType = CommandType.StoredProcedure;
                        actualizarEventoCmd.Parameters.AddWithValue("@id_evento", IdEvento);
                        actualizarEventoCmd.Parameters.AddWithValue("@nombre", nombreEvento);
                        actualizarEventoCmd.Parameters.AddWithValue("@descripcion", descripcion);
                        actualizarEventoCmd.Parameters.AddWithValue("@imagen", imagen);
                        actualizarEventoCmd.Parameters.AddWithValue("@fecha_inicio", fecha_inicio);
                        actualizarEventoCmd.Parameters.AddWithValue("@fecha_final", fecha_final);
                        actualizarEventoCmd.Parameters.AddWithValue("@lugar", lugar);
                        actualizarEventoCmd.Parameters.AddWithValue("@aforo", aforo);
                        actualizarEventoCmd.Parameters.AddWithValue("@valor", 0);
                        actualizarEventoCmd.Parameters.AddWithValue("@iva", 0);
                        actualizarEventoCmd.Parameters.AddWithValue("@valor_total", valor_total);
                        actualizarEventoCmd.Parameters.AddWithValue("@id_tipo_evento", id_tipo_evento);
                        actualizarEventoCmd.ExecuteNonQuery();
                    }
                }

                // Se retorna un mensaje de éxito
                return StatusCode(StatusCodes.Status200OK, new { mensaje = "El evento se actualizó correctamente" });
            }
            catch (Exception error)
            {
                // Si hay algún error se retorna un mensaje de error
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }


        /// <summary>
        /// Elimina un evento por su ID
        /// </summary>
        /// <param name="idEvento">ID dek evebti</param>
        /// <returns>
        /// Retorna un objeto de tipo IActionResult que indica el resultado de la operación.
        /// Si el evento se elimina correctamente, devuelve un objeto StatusCode con el estado 200 y un mensaje de éxito.
        /// Si la URL de la imagen es inválida, devuelve un objeto BadRequest con el estado 400 y un mensaje de error.
        /// Si hay un error al eliminar la imagen de Cloudinary, devuelve un objeto StatusCode con el estado 500 y un mensaje de error.
        /// Si hay un error interno en el servidor, devuelve un objeto StatusCode con el estado 500 y un mensaje de error.
        /// </returns>

        // Endpoint para eliminar un evento con todos sus registros
        [HttpDelete]
        [Route("Delete/{idEvento:int}")]
        public IActionResult EliminarEvento(int idEvento)
        {
            try
            {
                // Obtener imagen del evento a eliminar
                string imagenUrl = "";
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("USP_ObtenerEvento", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_evento", idEvento);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Se obtiene el campo de la imagen en la DB y se guarda en imagenUrl
                            imagenUrl = reader["imagen_evento"].ToString();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(imagenUrl))
                {
                    // Eliminar imagen de Cloudinary
                    var cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
                    var publicId = Path.GetFileNameWithoutExtension(imagenUrl);
                    var deletionParams = new DeletionParams(publicId) { ResourceType = ResourceType.Image };
                    var deletionResult = cloudinary.Destroy(deletionParams);

                    if (deletionResult.Result == "ok")
                    {
                        // Eliminar evento y registros relacionados de la base de datos
                        using (var conexion = new SqlConnection(cadenaSQL))
                        {
                            conexion.Open();
                            var cmd = new SqlCommand("USP_EliminarEvento", conexion);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@id_evento", idEvento);
                            cmd.ExecuteNonQuery();
                        }

                        // Retornar mensaje de éxito
                        return StatusCode(StatusCodes.Status200OK, new { mensaje = "El evento se eliminó correctamente" });
                    }
                    else
                    {
                        // Si la eliminación de la imagen falla, puedes manejar el error como desees
                        return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al eliminar la imagen de Cloudinary" });
                    }
                }
                else
                {
                    // Si la URL de la imagen está vacía, puedes manejarlo como desees
                    return StatusCode(StatusCodes.Status400BadRequest, new { mensaje = "La URL de la imagen es inválida" });
                }
            }
            catch (Exception error)
            {
                // Si hay algún error, se retorna un mensaje de error
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }




        /// <summary>
        /// Obtiene todos los tipos de eventos
        /// </summary>
        /// <returns>
        /// Retorna un objeto de tipo IActionResult que indica el resultado de la operación.
        /// Si se obtienen los tipos de eventos correctamente, devuelve un objeto StatusCode con el estado 200 y una lista de objetos Event.
        /// Si hay un error interno en el servidor, devuelve un objeto StatusCode con el estado 500 y un mensaje de error.
        /// </returns>

        [HttpGet]
        [Route("get_event_types")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EventsType>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public IActionResult GetEventsType()
        {
            List<EventsType> lista = new List<EventsType>();
            try
            {
                //Se conecta la DB usando SqlConnection y la cadena de conexion
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    //Se abre la conexion a la DB
                    conexion.Open();
                    //Se ejecuta el procedimiento USP_ObtenerTiposEventos
                    var cmd = new SqlCommand("USP_ObtenerTiposEventos", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lista.Add(new EventsType
                            {
                                //Se le asgina los campos respectivos
                                TipoEvento = rd["tipo_evento"].ToString(),
                                idTipoEvento = Convert.ToInt32(rd["id_tipo_evento"]),
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

        /// <summary>
        /// Obtiene los eventos asociados a una organización.
        /// </summary>
        /// <param name="OrganizacionId">ID de la organización.</param>
        /// <returns>Lista de eventos asociados a la organización.</returns>
        [HttpGet]
        [Route("GetEventsOrg")]
        public IActionResult ObtenerEventosPorOrganizacion(int OrganizacionId)
        {
            // Nombre del procedimiento almacenado
            string storedProcedure = "USP_ObtenerEventosPorOrganizacion";

            try
            {
                // Crear una lista para almacenar los eventos
                List<Event> eventos = new List<Event>();

                // Establecer la conexión con la base de datos
                using (SqlConnection connection = new SqlConnection(cadenaSQL))
                {
                    connection.Open();

                    // Crear un comando para ejecutar el procedimiento almacenado
                    using (SqlCommand command = new SqlCommand(storedProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Agregar el parámetro de la organización ID al comando
                        command.Parameters.AddWithValue("@IdOrganizacion", OrganizacionId); // Cambio de nombre del parámetro


                        // Ejecutar el comando y obtener los resultados en un lector de datos   
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Leer los resultados y agregarlos a la lista de eventos
                            while (reader.Read())
                            {
                                Event evento = new Event
                                {
                                    IdEvento = (int)reader["id_evento"],
                                    NombreEvento = (string)reader["nombre_evento"],
                                    DescripcionEvento = (string)reader["descripcion_evento"],
                                    ImagenEvento = (string)reader["imagen_evento"],
                                    FechaInicioEvento = (DateTime)reader["fecha_inicio_evento"],
                                    FechaFinalEvento = (DateTime)reader["fecha_final_evento"],
                                    FechaCreacionEvento = (DateTime)reader["fecha_creacion_evento"],
                                    LugarEvento = (string)reader["lugar_evento"],
                                    AforoEvento = (int)reader["aforo_evento"],
                                    ValorEvento = Convert.ToDecimal(reader["valor_evento"]),
                                    Iva = Convert.ToDecimal(reader["iva"]),
                                    ValorTotalEvento = Convert.ToDecimal(reader["valor_total_evento"]),
                                    IdSuborganizacion = (int)reader["id_suborganización1"],
                                    IdTipoEvento = (int)reader["id_tipo_evento1"],
                                    TipoEvento = (string)reader["tipo_evento"],
                                    CuposDisponibles = (int)reader["cupos_disponibles"]

                                    // Otras propiedades del evento...
                                };

                                eventos.Add(evento);
                            }
                        }
                    }
                }

                // Devolver la lista de eventos como una respuesta HTTP 200 (OK)
                return Ok(eventos);
            }
            catch (Exception ex)
            {
                // Manejar cualquier error que ocurra durante la ejecución del procedimiento almacenado
                return StatusCode(500, "Error: " + ex.Message);
            }
        }



    }
}
