using System;

namespace ProjetoAPIDanilo.Modelos
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int Quantidade { get; set; }
        public bool PodeEmprestar { get; set; }
        public bool PodeDoar { get; set; }

        public virtual string ExibirInfo()
        {
            return $"Produto: {Nome} | Quantidade: {Quantidade}";
        }

        // SOBRECARGA
        public void Adicionar(int qtd)
        {
            Quantidade += qtd;
        }

        public void Adicionar(int qtd, string motivo)
        {
            Quantidade += qtd;
            Console.WriteLine($"+{qtd} unidades adicionadas. Motivo: {motivo}");
        }

        public void Retirar(int qtd)
        {
            Quantidade -= qtd;
        }
    }
}
