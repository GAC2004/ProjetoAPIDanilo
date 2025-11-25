using ProjetoAPIDanilo.Modelos;

namespace ProjetoAPIDanilo.Modelos { 

public class Aluno : Usuario
{
    public override string ExibirDados()
    {
        return $"Aluno: {Nome}";
    }

    public string SolicitarProduto()
    {
        return $"{Nome} solicitou um produto.";
    }
}
}