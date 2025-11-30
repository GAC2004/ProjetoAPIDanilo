using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using ProjetoAPIDanilo.Modelos;
using ProjetoAPIDanilo.Data;
using System.Collections.Generic;

namespace ProjetoAPIDanilo.Controllers
{
    [ApiController]
    [Route("api/usuario")]
    public class UsuarioController : ControllerBase
    {
        private readonly Database _db;

        public UsuarioController(Database db)
        {
            _db = db;
        }

        // GET: api/usuario/listar
        [HttpGet("listar")]
        public ActionResult<List<Usuario>> Listar()
        {
            var lista = new List<Usuario>();

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = "SELECT id, nome, tipousuario FROM usuario";

            using var cmd = new MySqlCommand(sql, conn);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new Usuario
                {
                    Id = rd.GetInt32("id"),
                    Nome = rd.GetString("nome"),
                    TipoUsuario = rd.GetString("tipousuario")
                });
            }

            return Ok(lista);
        }

        // GET: api/usuario/buscar/{id}
        [HttpGet("buscar/{id}")]
        public ActionResult BuscarPorId(int id)
        {
            Usuario usuario = null;

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = "SELECT id, nome, tipousuario FROM usuario WHERE id = @Id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                usuario = new Usuario
                {
                    Id = reader.GetInt32("id"),
                    Nome = reader.GetString("nome"),
                    TipoUsuario = reader.GetString("tipousuario")
                };
            }

            if (usuario == null) return NotFound("Usuário não encontrado.");

            return Ok(new
            {
                usuario.Id,
                usuario.Nome,
                usuario.TipoUsuario,
                Descricao = usuario.ExibirDados()
            });
        }

        // POST: api/usuario/cadastrar
        [HttpPost("cadastrar")]
        public ActionResult Cadastrar([FromBody] Usuario usuario)
        {
            if (usuario == null || string.IsNullOrWhiteSpace(usuario.Nome))
                return BadRequest("Nome é obrigatório.");

            if (string.IsNullOrWhiteSpace(usuario.TipoUsuario))
                return BadRequest("TipoUsuario é obrigatório.");

            using var conn = _db.GetConnection();
            conn.Open();

            // Inserir usuário
            string sqlInsert = "INSERT INTO usuario (nome, tipousuario) VALUES (@Nome, @TipoUsuario)";
            using var cmd = new MySqlCommand(sqlInsert, conn);
            cmd.Parameters.AddWithValue("@Nome", usuario.Nome);
            cmd.Parameters.AddWithValue("@TipoUsuario", usuario.TipoUsuario);
            cmd.ExecuteNonQuery();

            // Recuperar o ID gerado
            string sqlId = "SELECT LAST_INSERT_ID()";
            using var cmdId = new MySqlCommand(sqlId, conn);
            usuario.Id = Convert.ToInt32(cmdId.ExecuteScalar());

            return Ok(new
            {
                Mensagem = "Usuário cadastrado com sucesso.",
                Dados = usuario
            });
        }

        // DELETE: api/usuario/remover/{id}
        [HttpDelete("remover/{id}")]
        public ActionResult Deletar(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string sql = "DELETE FROM usuario WHERE id = @Id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            int linhasAfetadas = cmd.ExecuteNonQuery();
            if (linhasAfetadas == 0) return NotFound("Usuário não encontrado.");

            return Ok(new { Mensagem = "Usuário removido com sucesso." });
        }
    }
}
