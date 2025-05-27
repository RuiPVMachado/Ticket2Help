using DAL;
using DAL.Interfaces;
using Microsoft.Data.SqlClient;
using MODELS;
using System;
using System.Collections.Generic;

namespace DAL.Repositories
{
    public class UtilizadorRepository : IUtilizadorRepository
    {
        public Utilizador ObterPorUsername(string username)
        {
            using var conn = new SqlConnection(DatabaseConfig.ConnectionString);
            conn.Open();

            string query = @"SELECT * FROM Utilizadores WHERE Username = @Username";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Username", username);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Utilizador
                {
                    Id = (int)reader["Id"],
                    Nome = reader["Nome"].ToString(),
                    Username = reader["Username"].ToString(),
                    PasswordHash = reader["PasswordHash"].ToString(),
                    Tipo = Enum.Parse<TipoUtilizador>(reader["TipoUtilizador"].ToString()),
                    DataCriacao = Convert.ToDateTime(reader["DataCriacao"])
                };
            }

            return null;
        }

        public List<Utilizador> ObterTodos()
        {
            var lista = new List<Utilizador>();
            using var conn = new SqlConnection(DatabaseConfig.ConnectionString);
            conn.Open();

            string query = @"SELECT * FROM Utilizadores";
            using var cmd = new SqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Utilizador
                {
                    Id = (int)reader["Id"],
                    Nome = reader["Nome"].ToString(),
                    Username = reader["Username"].ToString(),
                    PasswordHash = reader["PasswordHash"].ToString(),
                    Tipo = Enum.Parse<TipoUtilizador>(reader["TipoUtilizador"].ToString()),
                    DataCriacao = Convert.ToDateTime(reader["DataCriacao"])
                });
            }

            return lista;
        }
    }
}
