using Microsoft.AspNetCore.Mvc;
using ProjetoAPIDanilo.Data;
using ProjetoAPIDanilo.Modelos;

namespace ProjetoAPIDanilo.Controllers
{
    [ApiController]
    [Route("api/requisicoes")]
    public class RequisicaoController : ControllerBase
    {
        private readonly Database _db;

        public RequisicaoController(Database db)
        {
            _db = db;
        }

        // ==========================================================
        // POST → Registrar
        // ==========================================================
        [HttpPost]
        public IActionResult Registrar([FromBody] Requisicao req)
        {
            bool ok = req.Registrar(_db);

            if (!ok)
                return BadRequest("Erro ao registrar requisição.");

            return Ok(new
            {
                Mensagem = "Requisição registrada com sucesso.",
                Dados = req
            });
        }

        // ==========================================================
        // DELETE → Cancelar
        // ==========================================================
        [HttpDelete("{id}")]
        public IActionResult Cancelar(int id)
        {
            var r = new Requisicao { Id = id };

            bool ok = r.Cancelar(_db);

            if (!ok)
                return NotFound("Requisição não encontrada.");

            return Ok(new
            {
                Mensagem = "Requisição cancelada e estoque restaurado."
            });
        }
    }
}
