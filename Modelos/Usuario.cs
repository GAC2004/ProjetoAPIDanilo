using System.ComponentModel.DataAnnotations;

namespace ProjetoAPIDanilo.Modelos
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "TipoUsuario é obrigatório.")]
        public string TipoUsuario { get; set; }  

        public virtual string ExibirDados()
        {
            return $"Usuário: {Nome}";
        }
    }
}
