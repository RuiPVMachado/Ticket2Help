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
    public class DetalhesHardwareRepository : IDetalhesHardwareRepository
    {
        public int Criar(DetalhesHardware detalhes)
        {
            const string sql = @"
                INSERT INTO DetalhesHardware
                  (TicketId, Equipamento, Avaria, PecaSubstituida, DescricaoReparacao)
                OUTPUT INSERTED.Id
                VALUES
                  (@TicketId, @Equipamento, @Avaria, @PecaSubstituida, @DescricaoReparacao);
            ";

            using var conn = new SqlConnection(DatabaseConfig.ConnectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TicketId", detalhes.TicketId);
            cmd.Parameters.AddWithValue("@Equipamento", detalhes.Equipamento);
            cmd.Parameters.AddWithValue("@Avaria", detalhes.Avaria);
            cmd.Parameters.AddWithValue("@PecaSubstituida", (object?)detalhes.PecaSubstituida ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DescricaoReparacao", (object?)detalhes.DescricaoReparacao ?? DBNull.Value);

            return (int)cmd.ExecuteScalar();
        }

        public DetalhesHardware ObterPorTicketId(int ticketId)
        {
            const string sql = "SELECT * FROM DetalhesHardware WHERE TicketId = @TicketId";
            using var conn = new SqlConnection(DatabaseConfig.ConnectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TicketId", ticketId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return new DetalhesHardware
            {
                Id = (int)reader["Id"],
                TicketId = (int)reader["TicketId"],
                Equipamento = reader["Equipamento"].ToString(),
                Avaria = reader["Avaria"].ToString(),
                PecaSubstituida = reader["PecaSubstituida"] as string,
                DescricaoReparacao = reader["DescricaoReparacao"] as string
            };
        }

        public bool Atualizar(DetalhesHardware detalhes)
        {
            const string sql = @"
                UPDATE DetalhesHardware
                SET Equipamento = @Equipamento,
                    Avaria = @Avaria,
                    PecaSubstituida = @PecaSubstituida,
                    DescricaoReparacao = @DescricaoReparacao
                WHERE TicketId = @TicketId
            ";

            using var conn = new SqlConnection(DatabaseConfig.ConnectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Equipamento", detalhes.Equipamento);
            cmd.Parameters.AddWithValue("@Avaria", detalhes.Avaria);
            cmd.Parameters.AddWithValue("@PecaSubstituida", (object?)detalhes.PecaSubstituida ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DescricaoReparacao", (object?)detalhes.DescricaoReparacao ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TicketId", detalhes.TicketId);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
