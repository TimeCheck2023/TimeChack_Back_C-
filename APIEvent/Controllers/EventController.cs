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

       



    }
}
