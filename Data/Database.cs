using MySqlConnector;

namespace ProjetoAPIDanilo.Data
{
    public class Database
    {
        private readonly string _conn;

        public Database(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }

        public MySqlConnection GetConnection() => new MySqlConnection(_conn);
    }
}
