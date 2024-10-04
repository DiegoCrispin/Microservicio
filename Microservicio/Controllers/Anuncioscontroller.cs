using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using AnunciosAPI.Models;
using Microsoft.Extensions.Configuration;

namespace AnunciosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnunciosController : ControllerBase
    {
        private readonly string _connectionString;

        public AnunciosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/Anuncios
        [HttpGet]
        public ActionResult<List<Anuncio>> GetAnuncios()
        {
            List<Anuncio> anuncios = new List<Anuncio>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT * FROM anuncios";
                using (var command = new MySqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        anuncios.Add(new Anuncio
                        {
                            Id = reader.GetInt32("Id"),
                            Titulo = reader.GetString("Titulo"),
                            Contenido = reader.GetString("Contenido"),
                            Urgente = reader.GetBoolean("Urgente"),
                            Destacado = reader.GetBoolean("Destacado")
                        });
                    }
                }
            }

            return Ok(anuncios);
        }

        // GET: api/Anuncios/5
        [HttpGet("{id}")]
        public ActionResult<Anuncio> GetAnuncio(int id)
        {
            Anuncio anuncio = null;

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT * FROM anuncios WHERE Id = @Id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            anuncio = new Anuncio
                            {
                                Id = reader.GetInt32("Id"),
                                Titulo = reader.GetString("Titulo"),
                                Contenido = reader.GetString("Contenido"),
                                Urgente = reader.GetBoolean("Urgente"),
                                Destacado = reader.GetBoolean("Destacado")
                            };
                        }
                    }
                }
            }

            if (anuncio == null)
            {
                return NotFound();  // Devuelve 404 si no se encuentra el anuncio
            }

            return Ok(anuncio);  // Devuelve el anuncio encontrado
        }

        // POST: api/Anuncios
        [HttpPost]
        public ActionResult CreateAnuncio([FromBody] Anuncio anuncio)
        {
            if (anuncio == null)
            {
                return BadRequest();  // Devuelve 400 si el anuncio es nulo
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "INSERT INTO anuncios (Titulo, Contenido, Urgente, Destacado) VALUES (@Titulo, @Contenido, @Urgente, @Destacado)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Titulo", anuncio.Titulo);
                    command.Parameters.AddWithValue("@Contenido", anuncio.Contenido);
                    command.Parameters.AddWithValue("@Urgente", anuncio.Urgente);
                    command.Parameters.AddWithValue("@Destacado", anuncio.Destacado);

                    command.ExecuteNonQuery();  // Ejecuta la inserción
                }
            }

            return Ok(new { message = "Anuncio creado con éxito." });  // Devuelve un mensaje de éxito
        }

        // PUT: api/Anuncios/5
        [HttpPut("{id}")]
        public ActionResult UpdateAnuncio(int id, [FromBody] Anuncio anuncio)
        {
            if (anuncio == null || id != anuncio.Id)
            {
                return BadRequest();  // Devuelve 400 si el anuncio es nulo o el ID no coincide
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "UPDATE anuncios SET Titulo = @Titulo, Contenido = @Contenido, Urgente = @Urgente, Destacado = @Destacado WHERE Id = @Id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", anuncio.Id);
                    command.Parameters.AddWithValue("@Titulo", anuncio.Titulo);
                    command.Parameters.AddWithValue("@Contenido", anuncio.Contenido);
                    command.Parameters.AddWithValue("@Urgente", anuncio.Urgente);
                    command.Parameters.AddWithValue("@Destacado", anuncio.Destacado);

                    command.ExecuteNonQuery();  // Ejecuta la actualización
                }
            }

            return Ok(new { message = "Anuncio Actualizado con éxito." });  // Devuelve un mensaje de éxito
        }

        // DELETE: api/Anuncios/5
        [HttpDelete("{id}")]
        public ActionResult DeleteAnuncio(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "DELETE FROM anuncios WHERE Id = @Id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = command.ExecuteNonQuery();  // Ejecuta la eliminación

                    if (rowsAffected == 0)
                    {
                        return NotFound();  // Devuelve 404 si no se encontró el anuncio para eliminar
                    }
                }
            }

            return Ok(new { message = "Anuncio Eliminado con éxito." });  // Devuelve un mensaje de éxito
        }
    }
}
