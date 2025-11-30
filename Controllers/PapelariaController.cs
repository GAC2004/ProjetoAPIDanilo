using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using ProjetoAPIDanilo.Modelos;
using ProjetoAPIDanilo.Data;
using System.Collections.Generic;

namespace ProjetoAPIDanilo.Controllers
{
    [ApiController]
    [Route("api/papelaria")]
    public class PapelariaController : ControllerBase
    {
        private readonly Database _db;

        public PapelariaController(Database db)
        {
            _db = db;
        }

        [HttpGet("listar")]
        public ActionResult<List<Papelaria>> Listar()
        {
            var lista = new List<Papelaria>();

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
                SELECT p.Id, p.Nome, p.Quantidade, p.PodeEmprestar, p.PodeDoar, pa.TipoItem
                FROM Produto p
                INNER JOIN Papelaria pa ON p.Id = pa.Id;
            ";

            using var cmd = new MySqlCommand(sql, conn);
            using var rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                lista.Add(new Papelaria
                {
                    Id = rd.GetInt32("Id"),
                    Nome = rd.GetString("Nome"),
                    Quantidade = rd.GetInt32("Quantidade"),
                    PodeEmprestar = rd.GetBoolean("PodeEmprestar"),
                    PodeDoar = rd.GetBoolean("PodeDoar"),
                    TipoItem = rd.GetString("TipoItem")
                });
            }

            return Ok(lista);
        }

        [HttpGet("buscar/{id}")]
        public ActionResult<Papelaria> Buscar(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                SELECT p.Id, p.Nome, p.Quantidade, p.PodeEmprestar, p.PodeDoar, pa.TipoItem
                FROM Produto p
                INNER JOIN Papelaria pa ON p.Id = pa.Id
                WHERE p.Id = @id;
            ", conn);

            cmd.Parameters.AddWithValue("@id", id);

            using var rd = cmd.ExecuteReader();
            if (rd.Read())
            {
                return Ok(new Papelaria
                {
                    Id = rd.GetInt32("Id"),
                    Nome = rd.GetString("Nome"),
                    Quantidade = rd.GetInt32("Quantidade"),
                    PodeEmprestar = rd.GetBoolean("PodeEmprestar"),
                    PodeDoar = rd.GetBoolean("PodeDoar"),
                    TipoItem = rd.GetString("TipoItem")
                });
            }

            return NotFound("Item de papelaria não encontrado.");
        }

        [HttpPost("cadastrar")]
        public ActionResult Cadastrar([FromBody] Papelaria p)
        {
            if (p == null || string.IsNullOrWhiteSpace(p.Nome))
                return BadRequest("Nome é obrigatório.");

            using var conn = _db.GetConnection();
            conn.Open();

            using var trans = conn.BeginTransaction();
            try
            {
                var cmdProd = new MySqlCommand(@"
                    INSERT INTO Produto (Nome, Quantidade, PodeEmprestar, PodeDoar, TipoProduto)
                    VALUES (@n, @q, @emp, @doa, 'Papelaria');
                    SELECT LAST_INSERT_ID();
                ", conn, trans);

                cmdProd.Parameters.AddWithValue("@n", p.Nome);
                cmdProd.Parameters.AddWithValue("@q", p.Quantidade);
                cmdProd.Parameters.AddWithValue("@emp", p.PodeEmprestar);
                cmdProd.Parameters.AddWithValue("@doa", p.PodeDoar);

                p.Id = Convert.ToInt32(cmdProd.ExecuteScalar());

                var cmdPa = new MySqlCommand("INSERT INTO Papelaria (Id, TipoItem) VALUES (@id, @tipo)", conn, trans);
                cmdPa.Parameters.AddWithValue("@id", p.Id);
                cmdPa.Parameters.AddWithValue("@tipo", p.TipoItem ?? "");
                cmdPa.ExecuteNonQuery();

                trans.Commit();
                return Ok("Item de papelaria cadastrado com sucesso.");
            }
            catch
            {
                try { trans.Rollback(); } catch { }
                return BadRequest("Erro ao cadastrar item de papelaria.");
            }
        }

        [HttpPut("atualizar/{id}")]
        public ActionResult Atualizar(int id, Papelaria novo)
        {
            if (novo == null || string.IsNullOrWhiteSpace(novo.Nome))
                return BadRequest("Nome é obrigatório.");

            using var conn = _db.GetConnection();
            conn.Open();

            using var trans = conn.BeginTransaction();
            try
            {
                var cmdProd = new MySqlCommand(@"
                    UPDATE Produto
                    SET Nome=@n, Quantidade=@q, PodeEmprestar=@emp, PodeDoar=@doa
                    WHERE Id=@id;
                ", conn, trans);

                cmdProd.Parameters.AddWithValue("@n", novo.Nome);
                cmdProd.Parameters.AddWithValue("@q", novo.Quantidade);
                cmdProd.Parameters.AddWithValue("@emp", novo.PodeEmprestar);
                cmdProd.Parameters.AddWithValue("@doa", novo.PodeDoar);
                cmdProd.Parameters.AddWithValue("@id", id);

                int affected = cmdProd.ExecuteNonQuery();
                if (affected == 0) { trans.Rollback(); return NotFound("Item não encontrado."); }

                var cmdPa = new MySqlCommand("UPDATE Papelaria SET TipoItem=@tipo WHERE Id=@id", conn, trans);
                cmdPa.Parameters.AddWithValue("@tipo", novo.TipoItem ?? "");
                cmdPa.Parameters.AddWithValue("@id", id);
                cmdPa.ExecuteNonQuery();

                trans.Commit();
                return Ok("Item atualizado com sucesso.");
            }
            catch
            {
                try { trans.Rollback(); } catch { }
                return BadRequest("Erro ao atualizar item de papelaria.");
            }
        }

        [HttpDelete("remover/{id}")]
        public ActionResult Remover(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("DELETE FROM Produto WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0 ? Ok("Item removido.") : NotFound("Item não encontrado.");
        }

        [HttpPut("adicionar-estoque/{id}/{qtd}")]
        public ActionResult AdicionarEstoque(int id, int qtd)
        {
            if (qtd <= 0) return BadRequest("Quantidade inválida.");

            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("UPDATE Produto SET Quantidade = Quantidade + @q WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@q", qtd);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0 ? Ok("Estoque atualizado.") : NotFound("Item não encontrado.");
        }

        [HttpPut("retirar-estoque/{id}/{qtd}")]
        public ActionResult RetirarEstoque(int id, int qtd)
        {
            if (qtd <= 0) return BadRequest("Quantidade inválida.");

            using var conn = _db.GetConnection();
            conn.Open();

            var check = new MySqlCommand("SELECT Quantidade FROM Produto WHERE Id=@id", conn);
            check.Parameters.AddWithValue("@id", id);
            var current = check.ExecuteScalar();
            if (current == null) return NotFound("Item não encontrado.");

            int qtAtual = Convert.ToInt32(current);
            if (qtAtual < qtd) return BadRequest("Quantidade insuficiente.");

            var cmd = new MySqlCommand("UPDATE Produto SET Quantidade = Quantidade - @q WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@q", qtd);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            return Ok("Estoque atualizado.");
        }
    }
}
