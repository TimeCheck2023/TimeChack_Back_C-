using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using APIEvent.Models;

namespace APIEvent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly string connectionString;

        public UserController(IConfiguration config)
        {
            // Cadena de conexión a la base de datos desde el archivo appsettings.json
            connectionString = config.GetConnectionString("CadenaSQL");
        }

        // Endpoint para listar las suborganizaciones
        /// <summary>
        /// Obtiene una lista de suborganizaciones.
        /// </summary>
        /// <returns>
        /// Retorna un objeto IActionResult que indica el resultado de la operación.
        /// Si se encuentran suborganizaciones, devuelve un objeto StatusCode con el estado 200 y una lista de objetos User con los datos de las suborganizaciones.
        /// Si no se encuentran suborganizaciones, devuelve un objeto StatusCode con el estado 200 y una lista vacía.
        /// Si hay un error interno en el servidor, devuelve un objeto StatusCode con el estado 500 y un mensaje de error.
        /// </returns>
        [HttpGet]
        [Route("List")]
        public IActionResult List()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("USP_ListarSuborganizaciones", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    SqlDataReader reader = command.ExecuteReader();

                    List<Organización> suborganizaciones = new List<Organización>();

                    while (reader.Read())
                    {
                        Organización suborganizacion = new Organización
                        {
                            idSuborganization = Convert.ToInt32(reader["id_suborganizacion"]),
                            nameSuborganization = reader["nombre_suborganizacion"].ToString(),
                            descriptionSuborganization = reader["descripcion_suborganizacion"].ToString(),
                            nameOrganization = reader["nombre_organizacion"].ToString()
                        };

                        suborganizaciones.Add(suborganizacion);
                    }

                    return Ok(suborganizaciones);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // Endpoint para agregar un nuevo miembro
        /// <summary>
        /// Agrega un nuevo miembro a la tabla Miembros.
        /// </summary>
        /// <param name="miembro">Objeto User con los datos del nuevo miembro.</param>
        /// <returns>
        /// Retorna un objeto IActionResult que indica el resultado de la operación.
        /// Si el miembro se ha agregado correctamente, devuelve un objeto StatusCode con el estado 200 y un mensaje de éxito.
        /// Si hay un error interno en el servidor, devuelve un objeto StatusCode con el estado 500 y un mensaje de error.
        /// </returns>
        [HttpPost]
        [Route("NuevoMiembro")]
        public IActionResult NuevoMiembro([FromBody] Miembro miembro)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("USP_InsertarMiembro", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    // Agregar los parámetros al procedimiento almacenado
                    command.Parameters.AddWithValue("@id_suborganizacion2", miembro.id_suborganizacion2);
                    command.Parameters.AddWithValue("@nro_documento_usuario1", miembro.nro_documento_usuario1);

                    if (miembro.Rol != null) // Verificar si se proporcionó un valor para el rol
                    {
                        command.Parameters.AddWithValue("@rol", miembro.Rol);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@rol", DBNull.Value); // Pasar un valor nulo para el rol
                    }

                    command.ExecuteNonQuery();

                    return Ok("El nuevo miembro ha sido agregado correctamente.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // Endpoint para obtener usuarios sin una suborganización específica
        /// <summary>
        /// Obtiene una lista de usuarios que no pertenecen a una suborganización específica.
        /// </summary>
        /// <param name="idSuborganizacion">ID de la suborganización para la cual se desea obtener los usuarios.</param>
        /// <returns>
        /// Retorna un objeto IActionResult que indica el resultado de la operación.
        /// Si se encuentran usuarios, devuelve un objeto StatusCode con el estado 200 y una lista de objetos User con los datos de los usuarios.
        /// Si no se encuentran usuarios, devuelve un objeto StatusCode con el estado 200 y una lista vacía.
        /// Si hay un error interno en el servidor, devuelve un objeto StatusCode con el estado 500 y un mensaje de error.
        /// </returns>
        [HttpGet]
        [Route("UsuariosSinSuborganizacion/{idSuborganizacion}")]
        public IActionResult UsuariosSinSuborganizacion(int idSuborganizacion)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("USP_ObtenerUsuariosSinSuborganizacion", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    // Agregar el parámetro al procedimiento almacenado
                    command.Parameters.AddWithValue("@idSuborganizacion", idSuborganizacion);

                    SqlDataReader reader = command.ExecuteReader();

                    List<Users> usuarios = new List<Users>();

                    while (reader.Read())
                    {
                        Users usuario = new Users
                        {
                            idUsuario = reader["nro_documento_usuario"].ToString(),
                            nombreUsuario = reader["nombre_completo_usuario"].ToString(),
                            emailUsuario = reader["correo_usuario"].ToString()
                            // Agregar el resto de las propiedades del usuario según tu estructura de datos
                        };

                        usuarios.Add(usuario);
                    }

                    return Ok(usuarios);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
