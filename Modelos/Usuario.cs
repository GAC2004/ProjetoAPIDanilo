using ProjetoAPIDanilo.Modelos;
namespace ProjetoAPIDanilo.Modelos
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }

        public virtual string ExibirDados()
        {
            return $"Usuário: {Nome}";
        }
    }
}
