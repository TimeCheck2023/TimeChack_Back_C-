using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
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
            //Cadena de conexion a la DB desde el archivo appsettings.json
            connectionString = config.GetConnectionString("CadenaSQL");
        }


        // Endpoint para listar las suborganizaciones
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

                    List<User> suborganizaciones = new List<User>();

                    while (reader.Read())
                    {
                        User suborganizacion = new();
                        {
                            suborganizacion.idSuborganization = Convert.ToInt32(reader["id_suborganizacion"]);
                            suborganizacion.nameSuborganization = reader["nombre_suborganizacion"].ToString();
                            suborganizacion.descriptionSuborganization = reader["descripcion_suborganizacion"].ToString();
                            suborganizacion.nameOrganization = reader["nombre_organizacion"].ToString();
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

    }
}
