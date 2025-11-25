using MySqlConnector;
using ProjetoAPIDanilo.Modelos;
using ProjetoAPIDanilo.Data;
//teste
namespace ProjetoAPIDanilo.Controllers
{
    public class AlunoController
    {
        private readonly Database _db;

        public AlunoController(Database db)
        {
            _db = db;
        }

        public List<Aluno> Listar()
        {
            var lista = new List<Aluno>();

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
                SELECT Usuario.Id, Usuario.Nome
                FROM Usuario
                INNER JOIN Aluno ON Usuario.Id = Aluno.Id;
            ";

            using var cmd = new MySqlCommand(sql, conn);
            using var rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                lista.Add(new Aluno
                {
                    Id = rd.GetInt32("Id"),
                    Nome = rd.GetString("Nome")
                });
            }

            return lista;
        }

        public Aluno Cadastrar(Aluno aluno)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var trans = conn.BeginTransaction();

            try
            {
                // Inserir em Usuario
                var cmd1 = new MySqlCommand(
                    @"INSERT INTO Usuario (Nome, TipoUsuario) 
                      VALUES (@n, 'Aluno'); 
                      SELECT LAST_INSERT_ID();",
                    conn, trans
                );
                cmd1.Parameters.AddWithValue("@n", aluno.Nome);

                aluno.Id = Convert.ToInt32(cmd1.ExecuteScalar());

                // Inserir em Aluno
                var cmd2 = new MySqlCommand(
                    "INSERT INTO Aluno (Id) VALUES (@id)",
                    conn, trans
                );
                cmd2.Parameters.AddWithValue("@id", aluno.Id);
                cmd2.ExecuteNonQuery();

                trans.Commit();
                return aluno;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }
    }
}
