using MySqlConnector;
using ProjetoAPIDanilo.Data;
using ProjetoAPIDanilo.Modelos;

namespace ProjetoAPIDanilo.Controllers
{
    public class PapelariaController
    {
        private readonly Database _db;

        public PapelariaController(Database db)
        {
            _db = db;
        }

        // -------------------------------------------------------------
        // LISTAR
        // -------------------------------------------------------------
        public List<Papelaria> Listar()
        {
            var lista = new List<Papelaria>();

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
                SELECT Produto.Id, Produto.Nome, Produto.Quantidade,
                       Papelaria.TipoItem
                FROM Produto
                INNER JOIN Papelaria ON Produto.Id = Papelaria.Id
                WHERE Produto.TipoProduto = 'Papelaria';
            ";

            using var cmd = new MySqlCommand(sql, conn);
            using var rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                lista.Add(new Papelaria
                {
                    Id = rd.GetInt32("Id"),
                    Nome = rd.GetString("Nome"),
                    Quantidade = rd.GetInt32("Quantidade"),
                    TipoItem = rd.GetString("TipoItem")
                });
            }

            return lista;
        }

        // -------------------------------------------------------------
        // CADASTRAR
        // -------------------------------------------------------------
        public Papelaria Cadastrar(Papelaria p)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var trans = conn.BeginTransaction();

            try
            {
                // Inserir na tabela Produto
                var cmd1 = new MySqlCommand(@"
                    INSERT INTO Produto (Nome, Quantidade, PodeEmprestar, PodeDoar, TipoProduto)
                    VALUES (@n, @q, @emp, @doa, 'Papelaria');
                    SELECT LAST_INSERT_ID();
                ", conn, trans);

                cmd1.Parameters.AddWithValue("@n", p.Nome);
                cmd1.Parameters.AddWithValue("@q", p.Quantidade);
                cmd1.Parameters.AddWithValue("@emp", p.PodeEmprestar);
                cmd1.Parameters.AddWithValue("@doa", p.PodeDoar);

                p.Id = Convert.ToInt32(cmd1.ExecuteScalar());

                // Inserir na tabela Papelaria
                var cmd2 = new MySqlCommand(
                    "INSERT INTO Papelaria (Id, TipoItem) VALUES (@id, @t)",
                    conn, trans);

                cmd2.Parameters.AddWithValue("@id", p.Id);
                cmd2.Parameters.AddWithValue("@t", p.TipoItem);

                cmd2.ExecuteNonQuery();

                trans.Commit();
                return p;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        // -------------------------------------------------------------
        // ATUALIZAR
        // -------------------------------------------------------------
        public bool Atualizar(int id, Papelaria atualizado)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var trans = conn.BeginTransaction();

            try
            {
                var cmd1 = new MySqlCommand(
                    @"UPDATE Produto SET Nome=@n, Quantidade=@q WHERE Id=@id;",
                    conn, trans);

                cmd1.Parameters.AddWithValue("@n", atualizado.Nome);
                cmd1.Parameters.AddWithValue("@q", atualizado.Quantidade);
                cmd1.Parameters.AddWithValue("@id", id);
                cmd1.ExecuteNonQuery();

                var cmd2 = new MySqlCommand(
                    @"UPDATE Papelaria SET TipoItem=@t WHERE Id=@id;",
                    conn, trans);

                cmd2.Parameters.AddWithValue("@t", atualizado.TipoItem);
                cmd2.Parameters.AddWithValue("@id", id);
                cmd2.ExecuteNonQuery();

                trans.Commit();
                return true;
            }
            catch
            {
                trans.Rollback();
                return false;
            }
        }
    }
}
