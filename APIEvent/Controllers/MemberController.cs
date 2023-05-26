using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using APIEvent.Models;

namespace APIEvent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly string connectionString;
        public MemberController(IConfiguration config)
        {
            // Cadena de conexion a la DB desde el archivo appsettings.json
            connectionString = config.GetConnectionString("CadenaSQL");
        }

        // Endpoint para eliminar un miembro
        /// <summary>
        /// Elimina un miembro de la tabla Miembro.
        /// </summary>
        /// <param name="nroDocumentoUsuario">Número de documento del usuario a eliminar.</param>
        /// <param name="idSuborganizacion">ID de la suborganización a la que pertenece el miembro.</param>
        /// <returns>
        /// Retorna un objeto IActionResult que indica el resultado de la operación.
        /// Si el miembro se ha eliminado correctamente, devuelve un objeto StatusCode con el estado 200 y un mensaje de éxito.
        /// Si hay un error interno en el servidor, devuelve un objeto StatusCode con el estado 500 y un mensaje de error.
        /// </returns>
        [HttpDelete]
        [Route("DeleteMember")]
        public IActionResult EliminarMiembro(string nroDocumentoUsuario, int idSuborganizacion)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("USP_EliminarMiembro", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@nro_documento_usuario1", nroDocumentoUsuario);
                    command.Parameters.AddWithValue("@id_suborganizacion", idSuborganizacion);

                    command.ExecuteNonQuery();

                    return Ok("Miembro eliminado correctamente.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // Endpoint para editar el rol de un miembro
        /// <summary>
        /// Edita el rol de un miembro en la tabla Miembro.
        /// </summary>
        /// <param name="miembro">Objeto que contiene los datos del miembro a editar.</param>
        /// <returns>
        /// Retorna un objeto IActionResult que indica el resultado de la operación.
        /// Si el rol se ha editado correctamente, devuelve un objeto StatusCode con el estado 200 y un mensaje de éxito.
        /// Si hay un error interno en el servidor, devuelve un objeto StatusCode con el estado 500 y un mensaje de error.
        /// </returns>
        [HttpPut]
        [Route("EditMemberRole")]
        public IActionResult EditarRolMiembro([FromBody] Miembro miembro)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("USP_EditarRolMiembro", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@nro_documento_usuario", miembro.nro_documento_usuario1);
                    command.Parameters.AddWithValue("@id_suborganizacion", miembro.id_suborganizacion2);
                    command.Parameters.AddWithValue("@nuevo_rol", miembro.Rol);

                    command.ExecuteNonQuery();

                    return Ok("Rol del miembro editado correctamente.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }


}
