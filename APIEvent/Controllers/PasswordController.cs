using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using BCrypt.Net;
using APIEvent.Models;

namespace APIEvent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly string connectionString;
        public PasswordController(IConfiguration config)
        {
            // Cadena de conexión a la DB desde el archivo appsettings.json
            connectionString = config.GetConnectionString("CadenaSQL");
        }

        // Endpoint para actualizar la contraseña
        /// <summary>
        /// Actualiza la contraseña de un usuario.
        /// </summary>
        /// <param name="model">Objeto que contiene los datos necesarios para la actualización de contraseña.</param>
        /// <returns>
        /// Retorna un objeto IActionResult que indica el resultado de la operación.
        /// Si la contraseña se ha actualizado correctamente, devuelve un objeto StatusCode con el estado 200 y un mensaje de éxito.
        /// Si la contraseña actual ingresada es incorrecta, devuelve un objeto StatusCode con el estado 400 y un mensaje de error.
        /// Si la nueva contraseña es igual a la actual, devuelve un objeto StatusCode con el estado 400 y un mensaje de error.
        /// Si hay un error interno en el servidor, devuelve un objeto StatusCode con el estado 500 y un mensaje de error.
        /// </returns>
        [HttpPut]
        [Route("UpdatePassword")]
        public IActionResult UpdatePassword([FromBody] Password model)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Verificar si la contraseña actual es correcta
                    SqlCommand obtenerContraseñaCommand = new SqlCommand("USP_ObtenerContraseña", connection);
                    obtenerContraseñaCommand.CommandType = CommandType.StoredProcedure;
                    obtenerContraseñaCommand.Parameters.AddWithValue("@id", model.Id);
                    obtenerContraseñaCommand.Parameters.Add("@contraseña", SqlDbType.VarChar, 255).Direction = ParameterDirection.Output;
                    obtenerContraseñaCommand.ExecuteNonQuery();
                    string contraseñaDB = obtenerContraseñaCommand.Parameters["@contraseña"].Value.ToString();

                    if (!BCrypt.Net.BCrypt.Verify(model.ContraseñaActual, contraseñaDB))
                    {
                        return BadRequest("La contraseña actual ingresada es incorrecta.");
                    }

                    // Verificar que la nueva contraseña sea diferente a la actual
                    if (model.ContraseñaActual == model.ContraseñaNueva)
                    {
                        return BadRequest("La nueva contraseña debe ser diferente a la actual.");
                    }

                    // Encriptar la nueva contraseña
                    string contraseñaNuevaEncriptada = BCrypt.Net.BCrypt.HashPassword(model.ContraseñaNueva);

                    // Actualizar la contraseña en la base de datos
                    SqlCommand actualizarContraseñaCommand = new SqlCommand("USP_ActualizarContraseña", connection);
                    actualizarContraseñaCommand.CommandType = CommandType.StoredProcedure;
                    actualizarContraseñaCommand.Parameters.AddWithValue("@id", model.Id);
                    actualizarContraseñaCommand.Parameters.AddWithValue("@contraseña", contraseñaNuevaEncriptada);
                    actualizarContraseñaCommand.ExecuteNonQuery();

                    return Ok("Contraseña actualizada correctamente.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }

}
