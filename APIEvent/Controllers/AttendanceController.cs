using APIEvent.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace APIEvent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly string connectionString;
        public AttendanceController(IConfiguration config)
        {
            //Cadena de conexion a la DB desde el archivo appsettings.json
            connectionString = config.GetConnectionString("CadenaSQL");
        }


        // Endpoint para insertar una asistencia
        [HttpPost]
        [Route("send")]
        public IActionResult InsertAttendance(Attendance attendance)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("sp_InsertarAsistencia", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@eventId", attendance.EventId);
                    command.Parameters.AddWithValue("@userEmail", attendance.UserEmail);

                    command.ExecuteNonQuery();

                    return Ok("Asistencia insertada correctamente.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        [HttpGet]
        [Route("Consult/{Id}")]
        public IActionResult GetAttendance(int Id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("USP_ObtenerAsistencia", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@id_asistencia_evento", Id);

                    var reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        Attendance attendance = new Attendance();

                        while (reader.Read())
                        {
                            attendance.Id = (int)reader["id_asistencia_evento"];
                            attendance.EventId = (int)reader["id_evento"];
                            attendance.DocumentNumber = reader["nro_documento_usuario"].ToString();
                            attendance.Status = reader["estado_asistencia"].ToString();
                        }

                        return Ok(attendance);
                    }
                    else
                    {
                        return BadRequest("No se encontró la asistencia.");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }



        //Endpoint para actualizar un evento en la DB
        [HttpPut]
        [Route("Update/{Id:int}")]
        public IActionResult UpdateAttendance(int eventId, string documentNumber, string status)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("USP_ActualizarEstadoAsistencia", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@id_evento", eventId);
                    command.Parameters.AddWithValue("@nro_documento_usuario", documentNumber);
                    command.Parameters.AddWithValue("@estado_asistencia", status);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok("Estado de asistencia actualizado correctamente.");
                    }
                    else
                    {
                        return BadRequest("No se pudo actualizar el estado de asistencia.");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }




        //Endpoint para consultar los eventos que está pendiente un usuario 
        [HttpGet]
        [Route("GetEventsUser/{email}")]
        public IActionResult GetEventsWithPendingAttendance(string email)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("USP_ObtenerEventosAsistenciaPendiente", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@correoUsuario", email);

                    SqlDataReader reader = command.ExecuteReader();

                    List<Attendance> events = new List<Attendance>();

                    while (reader.Read())
                    {
                        Attendance eventItem = new Attendance();

                        eventItem.EventId = (int)reader["id_evento"];
                        eventItem.EventName = reader["nombre_evento"].ToString();
                        eventItem.EventDescription = reader["descripcion_evento"].ToString();
                        eventItem.EventImage = reader["imagen_evento"].ToString();
                        eventItem.StartDate = (DateTime)reader["fecha_inicio_evento"];
                        eventItem.EndDate = (DateTime)reader["fecha_final_evento"];
                        eventItem.Location = reader["lugar_evento"].ToString();
                        eventItem.Capacity = (int)reader["aforo_evento"];
                        eventItem.TotalValue = (double)reader["valor_total_evento"];
                        eventItem.EventType = reader["tipo_evento"].ToString();

                        events.Add(eventItem);
                    }

                    return Ok(events);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        //Endpoint para cambiar el estado de asistencia a cancelado
        [HttpPut]
        [Route("CancelAttendance")]
        public IActionResult CancelAttendance(Attendance attendance)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("USP_CambiarEstadoAsistenciaCancelado", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@correo_usuario", attendance.UserEmail);
                    command.Parameters.AddWithValue("@id_evento", attendance.EventId);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok("Estado de asistencia actualizado correctamente.");
                    }
                    else
                    {
                        return BadRequest("No se encontró la asistencia para el correo y el evento especificados.");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }









    }
}
