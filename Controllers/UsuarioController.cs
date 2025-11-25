using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using ProjetoAPIDanilo.Modelos;

namespace ProjetoAPIDanilo.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    public class UsuarioController : ControllerBase
    {
        private readonly string _connectionString;

        public UsuarioController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ============================================================
        // GET – LISTAR TODOS
        // ============================================================
        [HttpGet]
        public IActionResult Listar()
        {
            var lista = new List<Usuario>();

            using var con = new MySqlConnection(_connectionString);
            con.Open();

            string sql = "SELECT Id, Nome FROM Usuario";

            using var cmd = new MySqlCommand(sql, con);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Usuario
                {
                    Id = reader.GetInt32("Id"),
                    Nome = reader.GetString("Nome")
                });
            }

            return Ok(lista);
        }

        // ============================================================
        // GET – EXIBIR UM
        // ============================================================
        [HttpGet("{id}")]
        public IActionResult BuscarPorId(int id)
        {
            Usuario usuario = null;

            using var con = new MySqlConnection(_connectionString);
            con.Open();

            string sql = "SELECT Id, Nome FROM Usuario WHERE Id = @Id";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            {
                if (reader.Read())
                {
                    usuario = new Usuario
                    {
                        Id = reader.GetInt32("Id"),
                        Nome = reader.GetString("Nome")
                    };
                }
            }

            if (usuario == null)
                return NotFound("Usuário não encontrado");

            return Ok(new
            {
                usuario.Id,
                usuario.Nome,
                Descricao = usuario.ExibirDados()
            });
        }

        // ============================================================
        // POST – CADASTRAR
        // ============================================================
        [HttpPost]
        public IActionResult Cadastrar(Usuario usuario)
        {
            using var con = new MySqlConnection(_connectionString);
            con.Open();

            string sql = @"INSERT INTO Usuario (Nome) 
                           VALUES (@Nome);
                           SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Nome", usuario.Nome);

            usuario.Id = Convert.ToInt32(cmd.ExecuteScalar());

            return Ok(usuario);
        }

        // ============================================================
        // DELETE – REMOVER
        // ============================================================
        [HttpDelete("{id}")]
        public IActionResult Deletar(int id)
        {
            using var con = new MySqlConnection(_connectionString);
            con.Open();

            string sql = "DELETE FROM Usuario WHERE Id = @Id";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Id", id);

            int linhas = cmd.ExecuteNonQuery();
            if (linhas == 0)
                return NotFound("Usuário não encontrado");

            return Ok("Usuário removido com sucesso.");
        }
    }
}
