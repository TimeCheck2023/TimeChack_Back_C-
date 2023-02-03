using APIEvent.Models;
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
                    var cmd = new SqlCommand("usp_listevents", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lista.Add(new Event
                            {
                                id_event = Convert.ToInt32(rd["id_event"]),
                                name_event = rd["name_event"].ToString(),
                                cost_event = Convert.ToInt32(rd["cost_event"]),
                                date_event = Convert.ToDateTime(rd["date_event"]),
                                location_event = rd["location_event"].ToString(),
                                description_event = rd["description_event"].ToString(),
                                capacity_event = Convert.ToInt32(rd["capacity_event"]),
                                id_admin = Convert.ToInt32(rd["id_admin"]),
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
        [Route("Obtener/{id_event:int}")]
        public IActionResult Obtener(int id_event)
        {
            List<Event> lista = new List<Event>();
            Event oproducto = new Event();
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("usp_listevents", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lista.Add(new Event
                            {
                                id_event = Convert.ToInt32(rd["id_event"]),
                                name_event = rd["name_event"].ToString(),
                                cost_event = Convert.ToInt32(rd["cost_event"]),
                                date_event = Convert.ToDateTime(rd["date_event"]),
                                location_event = rd["location_event"].ToString(),
                                description_event = rd["description_event"].ToString(),
                                capacity_event = Convert.ToInt32(rd["capacity_event"]),
                                id_admin = Convert.ToInt32(rd["id_admin"]),
                            });
                        }
                    }
                }
                oproducto = lista.Where(item => item.id_event == id_event).FirstOrDefault();
                return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok", response = oproducto });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message, response = oproducto });
            }
        }

        [HttpPost]
        [Route("Guardar")]
        public IActionResult Guardar([FromBody] Event objeto)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("usp_insertevent", conexion);
                    cmd.Parameters.AddWithValue("name_event", objeto.name_event);
                    cmd.Parameters.AddWithValue("cost_event", objeto.cost_event);
                    cmd.Parameters.AddWithValue("date_event", objeto.date_event);
                    cmd.Parameters.AddWithValue("location_event", objeto.location_event);
                    cmd.Parameters.AddWithValue("description_event", objeto.description_event);
                    cmd.Parameters.AddWithValue("capacity_event", objeto.capacity_event);
                    cmd.Parameters.AddWithValue("id_admin", objeto.id_admin);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.ExecuteNonQuery();
                }
                return StatusCode(StatusCodes.Status200OK, new { mensaje = "agregado" });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }


        [HttpPut]
        [Route("Editar")]
        public IActionResult Editar([FromBody] Event objeto)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("usp_editevents", conexion);
                    cmd.Parameters.AddWithValue("id_event", objeto.id_event == 0 ? DBNull.Value : objeto.id_event);
                    cmd.Parameters.AddWithValue("name_event", objeto.name_event is null ? DBNull.Value : objeto.name_event);
                    cmd.Parameters.AddWithValue("cost_event", objeto.cost_event  == 0 ? DBNull.Value : objeto.cost_event);
                    cmd.Parameters.AddWithValue("date_event", objeto.date_event);
                    cmd.Parameters.AddWithValue("location_event", objeto.location_event is null ? DBNull.Value : objeto.location_event);
                    cmd.Parameters.AddWithValue("description_event", objeto.description_event is null ? DBNull.Value : objeto.description_event);
                    cmd.Parameters.AddWithValue("capacity_event", objeto.capacity_event == 0 ? DBNull.Value : objeto.capacity_event);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.ExecuteNonQuery();
                }
                return StatusCode(StatusCodes.Status200OK, new { mensaje = "editado" });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }


        [HttpDelete]
        [Route("Eliminar/{id_event:int}")]
        public IActionResult Eliminar(int id_event)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("usp_deleteevent", conexion);
                    cmd.Parameters.AddWithValue("id_event", id_event);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.ExecuteNonQuery();
                }
                return StatusCode(StatusCodes.Status200OK, new { mensaje = "eliminado" });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }



    }
}
