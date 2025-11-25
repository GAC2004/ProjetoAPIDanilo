using MySqlConnector;
using ProjetoAPIDanilo.Data;
using ProjetoAPIDanilo.Modelos;

namespace ProjetoAPIDanilo.Controllers
{
    public class SecretariaController
    {
        private readonly Database _db;

        public SecretariaController(Database db)
        {
            _db = db;
        }

        public List<Secretaria> Listar()
        {
            var lista = new List<Secretaria>();

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"SELECT Usuario.Id, Usuario.Nome 
                           FROM Usuario
                           INNER JOIN Secretaria ON Usuario.Id = Secretaria.Id";

            using var cmd = new MySqlCommand(sql, conn);
            using var rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                lista.Add(new Secretaria
                {
                    Id = rd.GetInt32("Id"),
                    Nome = rd.GetString("Nome")
                });
            }

            return lista;
        }

        public Secretaria Cadastrar(Secretaria s)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd1 = new MySqlCommand(
                @"INSERT INTO Usuario (Nome, TipoUsuario) 
                  VALUES (@n, 'Secretaria'); 
                  SELECT LAST_INSERT_ID();",
                conn);

            cmd1.Parameters.AddWithValue("@n", s.Nome);

            s.Id = Convert.ToInt32(cmd1.ExecuteScalar());

            var cmd2 = new MySqlCommand(
                "INSERT INTO Secretaria (Id) VALUES (@id)", conn);

            cmd2.Parameters.AddWithValue("@id", s.Id);
            cmd2.ExecuteNonQuery();

            return s;
        }
    }
}
