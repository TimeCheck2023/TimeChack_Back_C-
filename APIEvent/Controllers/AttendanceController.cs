﻿using APIEvent.Models;
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



        /// <summary>
        /// Endpoint para insertar una asistencia.
        /// </summary>
        /// <param name="attendance">Objeto Attendance con los datos de la asistencia.</param>
        /// <returns>
        /// Retorna un objeto IActionResult que indica el resultado de la operación.
        /// Si la asistencia se inserta correctamente, devuelve un objeto StatusCode con el estado 200 y un mensaje de éxito.
        /// Si hay un error interno en el servidor, devuelve un objeto StatusCode con el estado 500 y un mensaje de error.
        /// </returns>
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

                    SqlParameter messageParameter = new SqlParameter("@Message", SqlDbType.VarChar, 1000);
                    messageParameter.Direction = ParameterDirection.Output;
                    command.Parameters.Add(messageParameter);

                    command.ExecuteNonQuery();

                    string message = command.Parameters["@Message"].Value.ToString();

                    if (message == "Asistencia insertada correctamente.")
                    {
                        return Ok(message);
                    }
                    else
                    {
                        return BadRequest(message);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }










        /// <summary>
        /// Endpoint para consultar si un usuario tiene una asistencia en un evento específico.
        /// </summary>
        /// <param name="idEvento">ID del evento.</param>
        /// <param name="correo">Correo del usuario.</param>
        /// <returns>
        /// Retorna un objeto IActionResult que indica el resultado de la operación.
        /// Si se encuentra la asistencia, devuelve un objeto StatusCode con el estado 200 y un objeto anónimo con las propiedades "exists" (indicando si existe o no la asistencia) y "tipoAsistencia" (tipo de asistencia).
        /// Si hay un error interno en el servidor, devuelve un objeto StatusCode con el estado 500 y un mensaje de error.
        /// </returns>
        // Endpoint para consultar si ya tiene una asistencia en un evento especificado
        [HttpGet]
        [Route("CheckAttendance")]
        public IActionResult CheckAttendance(int idEvento, string correo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("USP_ObtenerAsistenciaPorEventoYCorreo", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@idEvento", idEvento);
                    command.Parameters.AddWithValue("@correo", correo);

                    SqlParameter existsParameter = new SqlParameter("@exists", SqlDbType.Bit);
                    existsParameter.Direction = ParameterDirection.Output;
                    command.Parameters.Add(existsParameter);

                    SqlParameter tipoAsistenciaParameter = new SqlParameter("@tipoAsistencia", SqlDbType.VarChar, 20);
                    tipoAsistenciaParameter.Direction = ParameterDirection.Output;
                    command.Parameters.Add(tipoAsistenciaParameter);

                    command.ExecuteNonQuery();

                    bool exists = Convert.ToBoolean(existsParameter.Value);
                    string tipoAsistencia = tipoAsistenciaParameter.Value.ToString();

                    return Ok(new { exists, tipoAsistencia });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }



        /// <summary>
        /// Endpoint para cancelar una asistencia.
        /// </summary>
        /// <param name="correoUsuario">Correo del usuario.</param>
        /// <param name="idEvento">ID del evento.</param>
        /// <returns>
        /// Retorna un objeto IActionResult que indica el resultado de la operación.
        /// Si se cancela la asistencia correctamente, devuelve un objeto StatusCode con el estado 200 y un mensaje de éxito.
        /// Si hay un error interno en el servidor, devuelve un objeto StatusCode con el estado 500 y un mensaje de error.
        /// </returns>
        //Endpoint para cambiar el estado de asistencia a cancelado
        [HttpDelete]
        [Route("CancelAttendance")]
        public IActionResult CancelAttendance(string correoUsuario, int idEvento)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("USP_CambiarEstadoAsistenciaCancelado", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@correo_usuario", correoUsuario);
                    command.Parameters.AddWithValue("@id_evento", idEvento);

                    command.ExecuteNonQuery();

                    return Ok("Se eliminó correctamente la asistencia");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        /// <summary>
        /// Endpoint para confirmar una asistencia.
        /// </summary>
        /// <param name="nroDocumentoUsuario">Número de documento del usuario.</param>
        /// <param name="idEvento">Identificador del evento.</param>
        /// <returns>
        /// Retorna un objeto IActionResult que indica el resultado de la operación.
        /// Si se confirma la asistencia correctamente, devuelve un objeto StatusCode con el estado 200 y un mensaje de éxito.
        /// Si hay un error interno en el servidor, devuelve un objeto StatusCode con el estado 500 y un mensaje de error.
        /// </returns>
        [HttpPut]
        [Route("ConfirmAttendance")]
        public IActionResult ConfirmAttendance(string nroDocumentoUsuario, string idEvento)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("USP_CambiarEstadoAsistenciaConfirmado", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@nro_documento_usuario", nroDocumentoUsuario);
                    command.Parameters.AddWithValue("@id_evento2 ", idEvento);

                    SqlParameter messageParameter = new SqlParameter("@mensaje", SqlDbType.VarChar, 100);
                    messageParameter.Direction = ParameterDirection.Output;
                    command.Parameters.Add(messageParameter);

                    command.ExecuteNonQuery();

                    string message = command.Parameters["@mensaje"].Value.ToString();

                    return Ok(message);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        /// <summary>
        /// Obtiene la información de los usuarios con estado de asistencia pendiente para un evento específico.
        /// </summary>
        /// <param name="idEvento">ID del evento.</param>
        /// <returns>
        /// Retorna un objeto IActionResult que contiene la lista de información de los usuarios con estado de asistencia pendiente.
        /// Si la operación se realiza correctamente, devuelve un objeto StatusCode con el estado 200 y la lista de usuarios.
        /// Si hay un error interno en el servidor, devuelve un objeto StatusCode con el estado 500 y un mensaje de error.
        /// </returns>
        [HttpGet]
        [Route("GetPendingAttendees")]
        public IActionResult GetPendingAttendees(int idEvento)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("USP_ObtenerInformacionUsuarioPendiente", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@id_evento", idEvento);

                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    List<UserInformation> attendees = new List<UserInformation>();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        UserInformation attendee = new UserInformation
                        {
                            Email = row["correo_usuario"].ToString(),
                            DocumentNumber = row["nro_documento_usuario"].ToString(),
                            FullName = row["nombre_completo_usuario"].ToString(),
                            DocumentType = row["tipo_documento_usuario"].ToString()
                        };

                        attendees.Add(attendee);
                    }

                    return Ok(attendees);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la información de los usuarios con estado de asistencia confirmado para un evento específico.
        /// </summary>
        /// <param name="idEvento">ID del evento.</param>
        /// <returns>
        /// Retorna un objeto IActionResult que contiene la lista de información de los usuarios con estado de asistencia confirmado.
        /// Si la operación se realiza correctamente, devuelve un objeto StatusCode con el estado 200 y la lista de usuarios.
        /// Si hay un error interno en el servidor, devuelve un objeto StatusCode con el estado 500 y un mensaje de error.
        /// </returns>
        [HttpGet]
        [Route("GetConfirmedAttendees")]
        public IActionResult GetConfirmedAttendees(int idEvento)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("USP_ObtenerInformacionUsuarioConfirmado", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@id_evento", idEvento);

                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    List<UserInformation> attendees = new List<UserInformation>();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        UserInformation attendee = new UserInformation
                        {
                            Email = row["correo_usuario"].ToString(),
                            DocumentNumber = row["nro_documento_usuario"].ToString(),
                            FullName = row["nombre_completo_usuario"].ToString(),
                            DocumentType = row["tipo_documento_usuario"].ToString()
                        };

                        attendees.Add(attendee);
                    }

                    return Ok(attendees);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la información de los usuarios con estado de asistencia "no asistido" para un evento específico.
        /// </summary>
        /// <param name="idEvento">ID del evento.</param>
        /// <returns>
        /// Retorna un objeto IActionResult que contiene la lista de información de los usuarios con estado de asistencia "no asistido".
        /// Si la operación se realiza correctamente, devuelve un objeto StatusCode con el estado 200 y la lista de usuarios.
        /// Si hay un error interno en el servidor, devuelve un objeto StatusCode con el estado 500 y un mensaje de error.
        /// </returns>
        [HttpGet]
        [Route("GetNonAttendingAttendees")]
        public IActionResult GetNonAttendingAttendees(int idEvento)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("USP_ObtenerInformacionUsuarioNoAsistido", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@id_evento", idEvento);

                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    List<UserInformation> attendees = new List<UserInformation>();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        UserInformation attendee = new UserInformation
                        {
                            Email = row["correo_usuario"].ToString(),
                            DocumentNumber = row["nro_documento_usuario"].ToString(),
                            FullName = row["nombre_completo_usuario"].ToString(),
                            DocumentType = row["tipo_documento_usuario"].ToString()
                        };

                        attendees.Add(attendee);
                    }

                    return Ok(attendees);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }




    }
}
