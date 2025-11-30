using ProjetoAPIDanilo.Data;
using System;
using System.Collections.Generic;
using MySqlConnector;

namespace ProjetoAPIDanilo.Modelos
{
    public class Requisicao
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public string TipoRequisicao { get; set; } // Doacao | Emprestimo
        public DateTime Data { get; set; } = DateTime.Now;

        // ==========================================================
        // Registrar Requisicao
        // ==========================================================
        public bool Registrar(Database db)
        {
            using var conn = db.GetConnection();
            conn.Open();

            // Buscar produto
            var cmdProd = new MySqlCommand("SELECT * FROM Produto WHERE Id=@id", conn);
            cmdProd.Parameters.AddWithValue("@id", ProdutoId);

            Produto produto = null;
            using (var rd = cmdProd.ExecuteReader())
            {
                if (rd.Read())
                {
                    produto = new Produto
                    {
                        Id = rd.GetInt32("Id"),
                        Nome = rd.GetString("Nome"),
                        Quantidade = rd.GetInt32("Quantidade"),
                        PodeDoar = rd.GetBoolean("PodeDoar"),
                        PodeEmprestar = rd.GetBoolean("PodeEmprestar")
                    };
                }
                else return false;
            }

            // Validações
            if (TipoRequisicao == "Doacao" && !produto.PodeDoar) return false;
            if (TipoRequisicao == "Emprestimo" && !produto.PodeEmprestar) return false;
            if (produto.Quantidade < Quantidade) return false;

            // Atualizar estoque
            produto.Quantidade -= Quantidade;

            var cmdUpdate = new MySqlCommand(
                @"UPDATE Produto SET Quantidade=@q WHERE Id=@id", conn);
            cmdUpdate.Parameters.AddWithValue("@q", produto.Quantidade);
            cmdUpdate.Parameters.AddWithValue("@id", produto.Id);
            cmdUpdate.ExecuteNonQuery();

            // Registrar requisição
            var cmdInsert = new MySqlCommand(
                @"INSERT INTO Requisicao (UsuarioId, ProdutoId, Quantidade, TipoRequisicao, Data)
                  VALUES (@u, @p, @q, @t, @d);", conn);

            cmdInsert.Parameters.AddWithValue("@u", UsuarioId);
            cmdInsert.Parameters.AddWithValue("@p", ProdutoId);
            cmdInsert.Parameters.AddWithValue("@q", Quantidade);
            cmdInsert.Parameters.AddWithValue("@t", TipoRequisicao);
            cmdInsert.Parameters.AddWithValue("@d", Data);

            cmdInsert.ExecuteNonQuery();

            return true;
        }

        // ==========================================================
        // Cancelar Requisicao
        // ==========================================================
        public bool Cancelar(Database db)
        {
            using var conn = db.GetConnection();
            conn.Open();

            // Buscar requisição
            var cmd = new MySqlCommand("SELECT * FROM Requisicao WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", Id);

            int produtoId = 0;
            int qtd = 0;

            using (var rd = cmd.ExecuteReader())
            {
                if (rd.Read())
                {
                    produtoId = rd.GetInt32("ProdutoId");
                    qtd = rd.GetInt32("Quantidade");
                }
                else return false;
            }

            // Restaurar estoque
            var cmdEstoque = new MySqlCommand(
                @"UPDATE Produto SET Quantidade = Quantidade + @q WHERE Id=@id", conn);
            cmdEstoque.Parameters.AddWithValue("@q", qtd);
            cmdEstoque.Parameters.AddWithValue("@id", produtoId);
            cmdEstoque.ExecuteNonQuery();

            // Remover requisição
            var cmdDel = new MySqlCommand(
                "DELETE FROM Requisicao WHERE Id=@id", conn);
            cmdDel.Parameters.AddWithValue("@id", Id);
            cmdDel.ExecuteNonQuery();

            return true;
        }

        // ==========================================================
        // LISTAR TODAS AS REQUISIÇÕES
        // ==========================================================
        public static List<Requisicao> Listar(Database db)
        {
            var lista = new List<Requisicao>();

            using var conn = db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM Requisicao", conn);

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new Requisicao
                {
                    Id = rd.GetInt32("Id"),
                    UsuarioId = rd.GetInt32("UsuarioId"),
                    ProdutoId = rd.GetInt32("ProdutoId"),
                    Quantidade = rd.GetInt32("Quantidade"),
                    TipoRequisicao = rd.GetString("TipoRequisicao"),
                    Data = rd.GetDateTime("Data")
                });
            }

            return lista;
        }

        // ==========================================================
        // BUSCAR REQUISIÇÃO POR ID
        // ==========================================================
        public static Requisicao BuscarPorId(Database db, int id)
        {
            using var conn = db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM Requisicao WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var rd = cmd.ExecuteReader();

            if (rd.Read())
            {
                return new Requisicao
                {
                    Id = rd.GetInt32("Id"),
                    UsuarioId = rd.GetInt32("UsuarioId"),
                    ProdutoId = rd.GetInt32("ProdutoId"),
                    Quantidade = rd.GetInt32("Quantidade"),
                    TipoRequisicao = rd.GetString("TipoRequisicao"),
                    Data = rd.GetDateTime("Data")
                };
            }

            return null;
        }
    }
}
