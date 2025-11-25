using ProjetoAPIDanilo.Modelos;

namespace ProjetoAPIDanilo.Modelos
{
    public class Secretaria : Usuario
    {
        public void CadastrarProduto(Produto produto, List<Produto> banco)
        {
            banco.Add(produto);
        }

        public void AtualizarEstoque(Produto produto, int qtd)
        {
            produto.Quantidade = qtd;
        }

        public override string ExibirDados()
        {
            return $"Secretária: {Nome}";
        }
    }
}
