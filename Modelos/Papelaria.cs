namespace ProjetoAPIDanilo.Modelos
{
    public class Papelaria : Produto
    {
        public string TipoItem { get; set; }

        public override string ExibirInfo()
        {
            return $"Item de Papelaria: {Nome} | Tipo: {TipoItem} | Quantidade: {Quantidade}";
        }
    }
}
