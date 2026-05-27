using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLySinhVien;

internal static class Database
{
    private const string ConnectionString =
        "Server=.;Database=QLSV;Integrated Security=True;TrustServerCertificate=True;Encrypt=Optional";

    public static DataTable Query(string sql, params SqlParameter[] parameters)
    {
        using SqlConnection connection = new(ConnectionString);
        using SqlCommand command = new(sql, connection);
        command.Parameters.AddRange(parameters);

        using SqlDataAdapter adapter = new(command);
        DataTable result = new();
        adapter.Fill(result);
        return result;
    }

    public static int Execute(string sql, params SqlParameter[] parameters)
    {
        using SqlConnection connection = new(ConnectionString);
        using SqlCommand command = new(sql, connection);
        command.Parameters.AddRange(parameters);
        connection.Open();
        return command.ExecuteNonQuery();
    }
}
