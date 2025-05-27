using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DAL.Interfaces;
using Microsoft.Data.SqlClient;
using MODELS;

namespace DAL.Repositories
{
    public class DetalhesSoftwareRepository : IDetalhesSoftwareRepository
    {
        public int Criar(DetalhesSoftware detalhes)
        {
            const string sql = @"
                INSERT INTO DetalhesSoftware
                  (TicketId, Aplicacao, DescricaoNecessidade, SolucaoAplicada)
                OUTPUT INSERTED.Id
                VALUES
                  (@TicketId, @Aplicacao, @DescricaoNecessidade, @SolucaoAplicada);
            ";

            using var conn = new SqlConnection(DatabaseConfig.ConnectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TicketId", detalhes.TicketId);
            cmd.Parameters.AddWithValue("@Aplicacao", detalhes.Aplicacao);
            cmd.Parameters.AddWithValue("@DescricaoNecessidade", detalhes.DescricaoNecessidade);
            cmd.Parameters.AddWithValue("@SolucaoAplicada", (object?)detalhes.SolucaoAplicada ?? DBNull.Value);

            return (int)cmd.ExecuteScalar();
        }

        public DetalhesSoftware ObterPorTicketId(int ticketId)
        {
            const string sql = "SELECT * FROM DetalhesSoftware WHERE TicketId = @TicketId";
            using var conn = new SqlConnection(DatabaseConfig.ConnectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TicketId", ticketId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return new DetalhesSoftware
            {
                Id = (int)reader["Id"],
                TicketId = (int)reader["TicketId"],
                Aplicacao = reader["Aplicacao"].ToString(),
                DescricaoNecessidade = reader["DescricaoNecessidade"].ToString(),
                SolucaoAplicada = reader["SolucaoAplicada"] as string
            };
        }

        public bool Atualizar(DetalhesSoftware detalhes)
        {
            const string sql = @"
                UPDATE DetalhesSoftware
                SET Aplicacao = @Aplicacao,
                    DescricaoNecessidade = @DescricaoNecessidade,
                    SolucaoAplicada = @SolucaoAplicada
                WHERE TicketId = @TicketId
            ";

            using var conn = new SqlConnection(DatabaseConfig.ConnectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Aplicacao", detalhes.Aplicacao);
            cmd.Parameters.AddWithValue("@DescricaoNecessidade", detalhes.DescricaoNecessidade);
            cmd.Parameters.AddWithValue("@SolucaoAplicada", (object?)detalhes.SolucaoAplicada ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TicketId", detalhes.TicketId);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
