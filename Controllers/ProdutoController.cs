using MySqlConnector;
using ProjetoAPIDanilo.Data;
using ProjetoAPIDanilo.Modelos;

namespace ProjetoAPIDanilo.Controllers
{
    public class ProdutoController
    {
        private readonly Database _db;

        public ProdutoController(Database db)
        {
            _db = db;
        }

        // LISTAR
        public List<Produto> Listar()
        {
            var lista = new List<Produto>();

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
                SELECT Id, Nome, Quantidade, PodeEmprestar, PodeDoar 
                FROM Produto";

            using var cmd = new MySqlCommand(sql, conn);
            using var rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                lista.Add(new Produto
                {
                    Id = rd.GetInt32("Id"),
                    Nome = rd.GetString("Nome"),
                    Quantidade = rd.GetInt32("Quantidade"),
                    PodeEmprestar = rd.GetBoolean("PodeEmprestar"),
                    PodeDoar = rd.GetBoolean("PodeDoar")
                });
            }

            return lista;
        }

        // CADASTRAR
        public Produto Cadastrar(Produto p)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                INSERT INTO Produto (Nome, Quantidade, PodeEmprestar, PodeDoar)
                VALUES (@n, @q, @emp, @doa);
                SELECT LAST_INSERT_ID();
            ", conn);

            cmd.Parameters.AddWithValue("@n", p.Nome);
            cmd.Parameters.AddWithValue("@q", p.Quantidade);
            cmd.Parameters.AddWithValue("@emp", p.PodeEmprestar);
            cmd.Parameters.AddWithValue("@doa", p.PodeDoar);

            p.Id = Convert.ToInt32(cmd.ExecuteScalar());
            return p;
        }

        // ATUALIZAR
        public bool Atualizar(int id, Produto p)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                UPDATE Produto
                SET Nome=@n, Quantidade=@q, PodeEmprestar=@emp, PodeDoar=@doa
                WHERE Id=@id;
            ", conn);

            cmd.Parameters.AddWithValue("@n", p.Nome);
            cmd.Parameters.AddWithValue("@q", p.Quantidade);
            cmd.Parameters.AddWithValue("@emp", p.PodeEmprestar);
            cmd.Parameters.AddWithValue("@doa", p.PodeDoar);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }

        // REMOVER
        public bool Remover(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                "DELETE FROM Produto WHERE Id=@id",
                conn);

            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }

        // ADICIONAR ESTOQUE
        public bool AdicionarEstoque(int id, int qtd)
        {
            Produto p = Buscar(id);
            if (p == null) return false;

            p.Adicionar(qtd);

            return Atualizar(id, p);
        }

        // RETIRAR ESTOQUE
        public bool RetirarEstoque(int id, int qtd)
        {
            if (qtd <= 0) return false;

            Produto p = Buscar(id);
            if (p == null) return false;

            if (p.Quantidade < qtd) return false;

            p.Retirar(qtd);

            return Atualizar(id, p);
        }

        // BUSCAR UM PRODUTO
        public Produto Buscar(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
                SELECT Id, Nome, Quantidade, PodeEmprestar, PodeDoar
                FROM Produto
                WHERE Id=@id
            ";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var rd = cmd.ExecuteReader();

            if (!rd.Read()) return null;

            return new Produto
            {
                Id = rd.GetInt32("Id"),
                Nome = rd.GetString("Nome"),
                Quantidade = rd.GetInt32("Quantidade"),
                PodeDoar = rd.GetBoolean("PodeDoar"),
                PodeEmprestar = rd.GetBoolean("PodeEmprestar")
            };
        }
    }
}
