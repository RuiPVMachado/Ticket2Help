using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DAL.Interfaces;
using Microsoft.Data.SqlClient;
using MODELS;
using System;
using System.Collections.Generic;

namespace DAL.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        public int Criar(Ticket ticket)
        {
            const string sql = @"
                INSERT INTO Tickets
                  (Titulo, Descricao, Prioridade, Tipo, IdColaborador)
                OUTPUT INSERTED.Id
                VALUES
                  (@Titulo, @Descricao, @Prioridade, @Tipo, @IdColaborador);
            ";

            using var conn = new SqlConnection(DatabaseConfig.ConnectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Titulo", ticket.Titulo);
            cmd.Parameters.AddWithValue("@Descricao", ticket.Descricao);
            cmd.Parameters.AddWithValue("@Prioridade", ticket.Prioridade.ToString());
            cmd.Parameters.AddWithValue("@Tipo", ticket.Tipo.ToString());
            cmd.Parameters.AddWithValue("@IdColaborador", ticket.IdColaborador);

            return (int)cmd.ExecuteScalar();
        }

        public Ticket ObterPorId(int id)
        {
            const string sql = "SELECT * FROM Tickets WHERE Id = @Id";
            using var conn = new SqlConnection(DatabaseConfig.ConnectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return LerTicket(reader);
        }

        public List<Ticket> ObterTodos()
        {
            const string sql = "SELECT * FROM Tickets";
            var lista = new List<Ticket>();

            using var conn = new SqlConnection(DatabaseConfig.ConnectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                lista.Add(LerTicket(reader));

            return lista;
        }

        public List<Ticket> ObterPorEstado(EstadoTicket estado)
        {
            const string sql = "SELECT * FROM Tickets WHERE Estado = @Estado";
            var lista = new List<Ticket>();

            using var conn = new SqlConnection(DatabaseConfig.ConnectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Estado", estado.ToString());

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(LerTicket(reader));

            return lista;
        }

        public bool AtualizarEstado(int ticketId, EstadoTicket novoEstado, SubEstadoAtendimento? subEstado, int? tecnicoId = null)
        {
            const string sql = @"
                UPDATE Tickets
                SET Estado = @Estado,
                    SubEstadoAtendimento = @SubEstado,
                    DataAtendimento = GETDATE(),
                    IdTecnico = @IdTecnico
                WHERE Id = @Id
            ";

            using var conn = new SqlConnection(DatabaseConfig.ConnectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Estado", novoEstado.ToString());
            cmd.Parameters.AddWithValue("@SubEstado", (object?)subEstado?.ToString() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdTecnico", (object?)tecnicoId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Id", ticketId);

            return cmd.ExecuteNonQuery() > 0;
        }

        // Método auxiliar para ler um Ticket do reader
        private Ticket LerTicket(SqlDataReader reader)
        {
            return new Ticket
            {
                Id = (int)reader["Id"],
                Titulo = reader["Titulo"].ToString(),
                Descricao = reader["Descricao"].ToString(),
                Prioridade = Enum.Parse<Prioridade>(reader["Prioridade"].ToString()),
                Tipo = Enum.Parse<TipoTicket>(reader["Tipo"].ToString()),
                Estado = Enum.Parse<EstadoTicket>(reader["Estado"].ToString()),
                SubEstado = reader["SubEstadoAtendimento"] == DBNull.Value
                    ? null
                    : Enum.Parse<SubEstadoAtendimento>(reader["SubEstadoAtendimento"].ToString()),
                DataCriacao = Convert.ToDateTime(reader["DataCriacao"]),
                DataAtendimento = reader["DataAtendimento"] == DBNull.Value
                    ? (DateTime?)null
                    : Convert.ToDateTime(reader["DataAtendimento"]),
                IdColaborador = (int)reader["IdColaborador"],
                IdTecnico = reader["IdTecnico"] == DBNull.Value
                    ? (int?)null
                    : (int)reader["IdTecnico"]
            };
        }
    }
}
