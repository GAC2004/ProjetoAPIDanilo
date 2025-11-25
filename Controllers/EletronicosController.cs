using MySqlConnector;
using ProjetoAPIDanilo.Data;
using ProjetoAPIDanilo.Modelos;

namespace ProjetoAPIDanilo.Controllers
{
    public class EletronicosController
    {
        private readonly Database _db;

        public EletronicosController(Database db)
        {
            _db = db;
        }

        // -------------------------------------------------------------
        // LISTAR
        // -------------------------------------------------------------
        public List<Eletronicos> Listar()
        {
            var lista = new List<Eletronicos>();

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
                SELECT Produto.Id, Produto.Nome, Produto.Quantidade,
                       Eletronicos.TipoEletronico
                FROM Produto
                INNER JOIN Eletronicos ON Produto.Id = Eletronicos.Id
                WHERE Produto.TipoProduto = 'Eletronico';
            ";

            using var cmd = new MySqlCommand(sql, conn);
            using var rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                lista.Add(new Eletronicos
                {
                    Id = rd.GetInt32("Id"),
                    Nome = rd.GetString("Nome"),
                    Quantidade = rd.GetInt32("Quantidade"),
                    TipoEletronico = rd.GetString("TipoEletronico")
                });
            }

            return lista;
        }

        // -------------------------------------------------------------
        // CADASTRAR
        // -------------------------------------------------------------
        public Eletronicos Cadastrar(Eletronicos e)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var trans = conn.BeginTransaction();

            try
            {
                var cmd1 = new MySqlCommand(@"
                    INSERT INTO Produto (Nome, Quantidade, PodeEmprestar, PodeDoar, TipoProduto)
                    VALUES (@n, @q, @emp, @doa, 'Eletronico');
                    SELECT LAST_INSERT_ID();
                ", conn, trans);

                cmd1.Parameters.AddWithValue("@n", e.Nome);
                cmd1.Parameters.AddWithValue("@q", e.Quantidade);
                cmd1.Parameters.AddWithValue("@emp", e.PodeEmprestar);
                cmd1.Parameters.AddWithValue("@doa", e.PodeDoar);

                e.Id = Convert.ToInt32(cmd1.ExecuteScalar());

                var cmd2 = new MySqlCommand(
                    "INSERT INTO Eletronicos (Id, TipoEletronico) VALUES (@id, @t)",
                    conn, trans);

                cmd2.Parameters.AddWithValue("@id", e.Id);
                cmd2.Parameters.AddWithValue("@t", e.TipoEletronico);
                cmd2.ExecuteNonQuery();

                trans.Commit();
                return e;
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
        public bool Atualizar(int id, Eletronicos atualizado)
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
                    @"UPDATE Eletronicos SET TipoEletronico=@t WHERE Id=@id;",
                    conn, trans);

                cmd2.Parameters.AddWithValue("@t", atualizado.TipoEletronico);
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
