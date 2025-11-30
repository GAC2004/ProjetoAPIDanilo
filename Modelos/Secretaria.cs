using System.Collections.Generic;

namespace ProjetoAPIDanilo.Modelos
{
    public class Secretaria : Usuario
    {
        public override string ExibirDados()
        {
            return $"Secretária: {Nome}";
        }

        public void CadastrarProduto(Produto produto, List<Produto> banco)
        {
            banco.Add(produto);
        }

        public void AtualizarEstoque(Produto produto, int qtd)
        {
            produto.Quantidade = qtd;
        }
    }
}
