using ProjetoAPIDanilo;
namespace ProjetoAPIDanilo.Modelos
{
    public class Eletronicos : Produto
    {
        public string TipoEletronico { get; set; }

        public override string ExibirInfo()
        {
            return $"Eletrônico: {Nome} | Tipo: {TipoEletronico} | Quantidade: {Quantidade}";
        }
    }
}
