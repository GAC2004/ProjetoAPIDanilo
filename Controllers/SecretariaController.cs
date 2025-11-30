using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using ProjetoAPIDanilo.Data;
using ProjetoAPIDanilo.Modelos;
using System.Collections.Generic;

namespace ProjetoAPIDanilo.Controllers
{
    [ApiController]
    [Route("api/secretaria")]
    public class SecretariaController : ControllerBase
    {
        private readonly Database _db;

        public SecretariaController(Database db)
        {
            _db = db;
        }

        // GET: api/secretaria/listar
        [HttpGet("listar")]
        public IActionResult Listar()
        {
            var lista = new List<Usuario>();
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM Usuario WHERE TipoUsuario='Secretaria'", conn);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new Usuario
                {
                    Id = rd.GetInt32("Id"),
                    Nome = rd.GetString("Nome")
                });
            }

            return Ok(lista);
        }

        // GET: api/secretaria/buscar/{id}
        [HttpGet("buscar/{id}")]
        public IActionResult Buscar(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM Usuario WHERE Id=@id AND TipoUsuario='Secretaria'", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var rd = cmd.ExecuteReader();
            if (rd.Read())
            {
                var sec = new Usuario
                {
                    Id = rd.GetInt32("Id"),
                    Nome = rd.GetString("Nome")
                };
                return Ok(sec);
            }
            return NotFound("Secretaria não encontrada.");
        }

        // POST: api/secretaria/cadastrar
        [HttpPost("cadastrar")]
        public IActionResult Cadastrar([FromBody] Usuario s)
        {
            if (string.IsNullOrWhiteSpace(s.Nome))
                return BadRequest("Nome é obrigatório.");

            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("INSERT INTO Usuario (Nome, TipoUsuario) VALUES (@nome,'Secretaria'); SELECT LAST_INSERT_ID();", conn);
            cmd.Parameters.AddWithValue("@nome", s.Nome);

            s.Id = Convert.ToInt32(cmd.ExecuteScalar());

            return Ok(new { Mensagem = "Secretaria cadastrada com sucesso.", Dados = s });
        }

        // PUT: api/secretaria/atualizar/{id}
        [HttpPut("atualizar/{id}")]
        public IActionResult Atualizar(int id, [FromBody] Usuario s)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("UPDATE Usuario SET Nome=@nome WHERE Id=@id AND TipoUsuario='Secretaria'", conn);
            cmd.Parameters.AddWithValue("@nome", s.Nome);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0 ? Ok("Secretaria atualizada com sucesso.") : NotFound("Secretaria não encontrada.");
        }

        // DELETE: api/secretaria/remover/{id}
        [HttpDelete("remover/{id}")]
        public IActionResult Remover(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            // Verifica se existem requisições ligadas à secretaria
            var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM Requisicao WHERE UsuarioId=@id", conn);
            checkCmd.Parameters.AddWithValue("@id", id);
            int count = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (count > 0)
                return BadRequest("Não é possível remover esta secretaria. Existem requisições associadas.");

            var cmd = new MySqlCommand("DELETE FROM Usuario WHERE Id=@id AND TipoUsuario='Secretaria'", conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0 ? Ok("Secretaria removida com sucesso.") : NotFound("Secretaria não encontrada.");
        }
    }
}
