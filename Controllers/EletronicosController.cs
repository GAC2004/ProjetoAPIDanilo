using Microsoft.AspNetCore.Mvc;
using ProjetoAPIDanilo.Modelos;
using ProjetoAPIDanilo.Data;
using MySqlConnector;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class EletronicosController : ControllerBase
{
    private readonly Database _db;

    public EletronicosController(Database db)
    {
        _db = db;
    }

    // ==================================================
    // GET: api/eletronicos/listar
    // ==================================================
    [HttpGet("listar")]
    public ActionResult<List<Eletronicos>> Listar()
    {
        var lista = new List<Eletronicos>();
        using var conn = _db.GetConnection();
        conn.Open();

        var cmd = new MySqlCommand(@"
            SELECT p.Id, p.Nome, p.Quantidade, p.PodeEmprestar, p.PodeDoar, e.TipoEletronico
            FROM Produto p
            INNER JOIN Eletronicos e ON p.Id = e.Id
        ", conn);

        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            lista.Add(new Eletronicos
            {
                Id = rd.GetInt32("Id"),
                Nome = rd.GetString("Nome"),
                Quantidade = rd.GetInt32("Quantidade"),
                PodeEmprestar = rd.GetBoolean("PodeEmprestar"),
                PodeDoar = rd.GetBoolean("PodeDoar"),
                TipoEletronico = rd.GetString("TipoEletronico")
            });
        }

        return Ok(lista);
    }

    // ==================================================
    // GET: api/eletronicos/buscar/{id}
    // ==================================================
    [HttpGet("buscar/{id}")]
    public ActionResult<Eletronicos> Buscar(int id)
    {
        using var conn = _db.GetConnection();
        conn.Open();

        var cmd = new MySqlCommand(@"
            SELECT p.Id, p.Nome, p.Quantidade, p.PodeEmprestar, p.PodeDoar, e.TipoEletronico
            FROM Produto p
            INNER JOIN Eletronicos e ON p.Id = e.Id
            WHERE p.Id = @id
        ", conn);
        cmd.Parameters.AddWithValue("@id", id);

        using var rd = cmd.ExecuteReader();
        if (rd.Read())
        {
            var eletronico = new Eletronicos
            {
                Id = rd.GetInt32("Id"),
                Nome = rd.GetString("Nome"),
                Quantidade = rd.GetInt32("Quantidade"),
                PodeEmprestar = rd.GetBoolean("PodeEmprestar"),
                PodeDoar = rd.GetBoolean("PodeDoar"),
                TipoEletronico = rd.GetString("TipoEletronico")
            };
            return Ok(eletronico);
        }

        return NotFound("Eletrônico não encontrado.");
    }

    // ==================================================
    // POST: api/eletronicos/cadastrar
    // ==================================================
    [HttpPost("cadastrar")]
    public ActionResult Cadastrar([FromBody] Eletronicos e)
    {
        if (string.IsNullOrWhiteSpace(e.Nome))
            return BadRequest("Nome é obrigatório.");

        using var conn = _db.GetConnection();
        conn.Open();

        // Inserir na tabela Produto
        var cmd = new MySqlCommand(@"
            INSERT INTO Produto (Nome, Quantidade, PodeEmprestar, PodeDoar)
            VALUES (@nome, @qtd, @emprestar, @doar); SELECT LAST_INSERT_ID();
        ", conn);
        cmd.Parameters.AddWithValue("@nome", e.Nome);
        cmd.Parameters.AddWithValue("@qtd", e.Quantidade);
        cmd.Parameters.AddWithValue("@emprestar", e.PodeEmprestar);
        cmd.Parameters.AddWithValue("@doar", e.PodeDoar);

        e.Id = Convert.ToInt32(cmd.ExecuteScalar());

        // Inserir na tabela Eletronicos
        var cmdEle = new MySqlCommand(@"
            INSERT INTO Eletronicos (Id, TipoEletronico) VALUES (@id, @tipo)
        ", conn);
        cmdEle.Parameters.AddWithValue("@id", e.Id);
        cmdEle.Parameters.AddWithValue("@tipo", e.TipoEletronico);
        cmdEle.ExecuteNonQuery();

        return Ok(new { Mensagem = "Eletrônico cadastrado com sucesso.", Dados = e });
    }

    // ==================================================
    // PUT: api/eletronicos/atualizar/{id}
    // ==================================================
    [HttpPut("atualizar/{id}")]
    public ActionResult Atualizar(int id, [FromBody] Eletronicos e)
    {
        using var conn = _db.GetConnection();
        conn.Open();

        var cmd = new MySqlCommand(@"
            UPDATE Produto SET Nome=@nome, Quantidade=@qtd, PodeEmprestar=@emprestar, PodeDoar=@doar
            WHERE Id=@id
        ", conn);
        cmd.Parameters.AddWithValue("@nome", e.Nome);
        cmd.Parameters.AddWithValue("@qtd", e.Quantidade);
        cmd.Parameters.AddWithValue("@emprestar", e.PodeEmprestar);
        cmd.Parameters.AddWithValue("@doar", e.PodeDoar);
        cmd.Parameters.AddWithValue("@id", id);

        int updated = cmd.ExecuteNonQuery();
        if (updated == 0) return NotFound("Eletrônico não encontrado.");

        var cmdEle = new MySqlCommand("UPDATE Eletronicos SET TipoEletronico=@tipo WHERE Id=@id", conn);
        cmdEle.Parameters.AddWithValue("@tipo", e.TipoEletronico);
        cmdEle.Parameters.AddWithValue("@id", id);
        cmdEle.ExecuteNonQuery();

        return Ok("Eletrônico atualizado com sucesso.");
    }

    // ==================================================
    // DELETE: api/eletronicos/remover/{id}
    // ==================================================
    [HttpDelete("remover/{id}")]
    public ActionResult Remover(int id)
    {
        using var conn = _db.GetConnection();
        conn.Open();

        var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM Requisicao WHERE ProdutoId=@id", conn);
        checkCmd.Parameters.AddWithValue("@id", id);
        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

        if (count > 0)
            return BadRequest("Não é possível remover este eletrônico. Existem requisições associadas.");

        // Remover Eletronicos
        var cmdEle = new MySqlCommand("DELETE FROM Eletronicos WHERE Id=@id", conn);
        cmdEle.Parameters.AddWithValue("@id", id);
        cmdEle.ExecuteNonQuery();

        // Remover Produto
        var cmd = new MySqlCommand("DELETE FROM Produto WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0 ? Ok("Eletrônico removido com sucesso.") : NotFound("Eletrônico não encontrado.");
    }
}
