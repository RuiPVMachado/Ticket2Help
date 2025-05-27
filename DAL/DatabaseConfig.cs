using Microsoft.Data.SqlClient;

namespace DAL
{
    public static class DatabaseConfig
    {
        public static string ConnectionString =>
            @"Data Source=.;Initial Catalog=Tick2DeskDB;Integrated Security=True;Encrypt=False;Trust Server Certificate=True";

        public static bool TestConnection()
        {
            try
            {
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}