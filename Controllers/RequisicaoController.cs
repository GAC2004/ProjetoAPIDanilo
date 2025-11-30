using Microsoft.AspNetCore.Mvc;
using ProjetoAPIDanilo.Modelos;
using ProjetoAPIDanilo.Data;
using System;
using System.Collections.Generic;

namespace ProjetoAPIDanilo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequisicaoController : ControllerBase
    {
        private readonly Database _db;

        public RequisicaoController(Database db)
        {
            _db = db;
        }

        // ==========================================================
        // LISTAR TODAS AS REQUISIÇÕES
        // ==========================================================
        [HttpGet("listar")]
        public IActionResult Listar()
        {
            try
            {
                var lista = Requisicao.Listar(_db);
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao listar requisições: {ex.Message}");
            }
        }

        // ==========================================================
        // BUSCAR REQUISIÇÃO POR ID
        // ==========================================================
        [HttpGet("buscar/{id}")]
        public IActionResult Buscar(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            var req = Requisicao.BuscarPorId(_db, id);
            if (req == null) return NotFound("Requisição não encontrada.");

            return Ok(req);
        }

        // ==========================================================
        // REGISTRAR NOVA REQUISIÇÃO
        // ==========================================================
        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] Requisicao r)
        {
            if (r == null) return BadRequest("JSON inválido.");
            if (r.Quantidade <= 0) return BadRequest("Quantidade deve ser maior que zero.");
            if (string.IsNullOrWhiteSpace(r.TipoRequisicao)) return BadRequest("Tipo de requisição obrigatório.");

            try
            {
                var sucesso = r.Registrar(_db);
                if (!sucesso) return BadRequest("Erro ao registrar requisição. Verifique estoque ou permissões do produto.");

                return Ok(new { Mensagem = "Requisição registrada com sucesso.", Dados = r });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao registrar requisição: {ex.Message}");
            }
        }

        // ==========================================================
        // CANCELAR REQUISIÇÃO
        // ==========================================================
        [HttpDelete("cancelar/{id}")]
        public IActionResult Cancelar(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            var req = Requisicao.BuscarPorId(_db, id);
            if (req == null) return NotFound("Requisição não encontrada.");

            try
            {
                var sucesso = req.Cancelar(_db);
                if (!sucesso) return BadRequest("Erro ao cancelar requisição.");

                return Ok(new { Mensagem = "Requisição cancelada e estoque restaurado." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao cancelar requisição: {ex.Message}");
            }
        }
    }
}
