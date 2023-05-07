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



        //Endpoint para guardar una nueva asistencia en la DB
        [HttpPost]
        [Route("Send")]
        public IActionResult InsertAttendance(Attendance attendance)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("USP_InsertarAsistencia", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@id_evento", attendance.EventId);
                    command.Parameters.AddWithValue("@nro_documento_usuario", attendance.DocumentNumber);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok("Asistencia registrada correctamente.");
                    }
                    else
                    {
                        return BadRequest("No se pudo registrar la asistencia.");
                    }
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
    }
}
