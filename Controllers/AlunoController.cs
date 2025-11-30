using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using ProjetoAPIDanilo.Data;
using ProjetoAPIDanilo.Modelos;
using System.Collections.Generic;

namespace ProjetoAPIDanilo.Controllers
{
    [ApiController]
    [Route("api/aluno")]
    public class AlunoController : ControllerBase
    {
        private readonly Database _db;

        public AlunoController(Database db)
        {
            _db = db;
        }

        [HttpGet("listar")]
        public IActionResult Listar()
        {
            var lista = new List<Aluno>();
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM Usuario WHERE TipoUsuario='Aluno'", conn);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new Aluno
                {
                    Id = rd.GetInt32("Id"),
                    Nome = rd.GetString("Nome")
                });
            }

            return Ok(lista);
        }

        [HttpGet("buscar/{id}")]
        public IActionResult Buscar(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM Usuario WHERE Id=@id AND TipoUsuario='Aluno'", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var rd = cmd.ExecuteReader();
            if (rd.Read())
            {
                var aluno = new Aluno
                {
                    Id = rd.GetInt32("Id"),
                    Nome = rd.GetString("Nome")
                };
                return Ok(aluno);
            }
            return NotFound("Aluno não encontrado.");
        }

        [HttpPost("cadastrar")]
        public IActionResult Cadastrar([FromBody] Aluno a)
        {
            if (string.IsNullOrWhiteSpace(a.Nome))
                return BadRequest("Nome é obrigatório.");

            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("INSERT INTO Usuario (Nome, TipoUsuario) VALUES (@nome,'Aluno'); SELECT LAST_INSERT_ID();", conn);
            cmd.Parameters.AddWithValue("@nome", a.Nome);

            a.Id = Convert.ToInt32(cmd.ExecuteScalar());
            return Ok(new { Mensagem = "Aluno cadastrado com sucesso.", Dados = a });
        }

        [HttpPut("atualizar/{id}")]
        public IActionResult Atualizar(int id, [FromBody] Aluno a)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("UPDATE Usuario SET Nome=@nome WHERE Id=@id AND TipoUsuario='Aluno'", conn);
            cmd.Parameters.AddWithValue("@nome", a.Nome);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0 ? Ok("Aluno atualizado com sucesso.") : NotFound("Aluno não encontrado.");
        }

        [HttpDelete("remover/{id}")]
        public IActionResult Remover(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            // Verifica se existem requisições ligadas ao aluno
            var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM Requisicao WHERE UsuarioId=@id", conn);
            checkCmd.Parameters.AddWithValue("@id", id);
            int count = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (count > 0)
                return BadRequest("Não é possível remover este aluno. Existem requisições associadas.");

            var cmd = new MySqlCommand("DELETE FROM Usuario WHERE Id=@id AND TipoUsuario='Aluno'", conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0 ? Ok("Aluno removido com sucesso.") : NotFound("Aluno não encontrado.");
        }
    }
}
