using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using ProjetoAPIDanilo.Data;
using ProjetoAPIDanilo.Modelos;
using System.Collections.Generic;

namespace ProjetoAPIDanilo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutoController : ControllerBase
    {
        private readonly Database _db;

        public ProdutoController(Database db)
        {
            _db = db;
        }

        // GET: api/produto
        [HttpGet("listar")]
        public IActionResult Listar()
        {
            var lista = new List<Produto>();

            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM Produto", conn);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new Produto
                {
                    Id = rd.GetInt32("Id"),
                    Nome = rd.GetString("Nome"),
                    Quantidade = rd.GetInt32("Quantidade"),
                    PodeEmprestar = rd.GetBoolean("PodeEmprestar"),
                    PodeDoar = rd.GetBoolean("PodeDoar")
                });
            }

            return Ok(lista);
        }

        // POST: api/produto/cadastrar
        [HttpPost("cadastrar")]
        public IActionResult Cadastrar([FromBody] Produto p)
        {
            if (string.IsNullOrWhiteSpace(p.Nome))
                return BadRequest("O nome do produto é obrigatório.");

            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                INSERT INTO Produto (Nome, Quantidade, PodeEmprestar, PodeDoar, TipoProduto)
                VALUES (@n, @q, @emp, @doa, @tp);
                SELECT LAST_INSERT_ID();
            ", conn);

            cmd.Parameters.AddWithValue("@n", p.Nome);
            cmd.Parameters.AddWithValue("@q", p.Quantidade);
            cmd.Parameters.AddWithValue("@emp", p.PodeEmprestar);
            cmd.Parameters.AddWithValue("@doa", p.PodeDoar);
            cmd.Parameters.AddWithValue("@tp", "Produto");

            p.Id = Convert.ToInt32(cmd.ExecuteScalar());

            return Ok(new { Mensagem = "Produto cadastrado com sucesso.", Dados = p });
        }

        // PUT: api/produto/atualizar/{id}
        [HttpPut("atualizar/{id}")]
        public IActionResult Atualizar(int id, [FromBody] Produto p)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                UPDATE Produto
                SET Nome=@n, Quantidade=@q, PodeEmprestar=@emp, PodeDoar=@doa
                WHERE Id=@id;
            ", conn);

            cmd.Parameters.AddWithValue("@n", p.Nome);
            cmd.Parameters.AddWithValue("@q", p.Quantidade);
            cmd.Parameters.AddWithValue("@emp", p.PodeEmprestar);
            cmd.Parameters.AddWithValue("@doa", p.PodeDoar);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0 ? Ok("Produto atualizado com sucesso.") : NotFound("Produto não encontrado.");
        }

        // DELETE: api/produto/remover/{id}
        [HttpDelete("remover/{id}")]
        public IActionResult Remover(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            // Verifica se existem requisições ligadas
            var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM Requisicao WHERE ProdutoId=@id", conn);
            checkCmd.Parameters.AddWithValue("@id", id);
            int count = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (count > 0)
                return BadRequest("Não é possível remover este produto. Existem requisições associadas.");

            // Remove o produto
            var cmd = new MySqlCommand("DELETE FROM Produto WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0 ? Ok("Produto removido com sucesso.") : NotFound("Produto não encontrado.");
        }
    }
}
