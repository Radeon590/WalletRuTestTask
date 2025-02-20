using System.Data;
using System.Reflection;
using System.Text;
using Npgsql;

namespace WalletRuTestTask.Api.Services.DbService.NpSql;

public class NpSqlDbService
{
    private NpgsqlConnection _npgsqlConnection;
    
    public NpSqlDbService(string connectionString)
    {
        _npgsqlConnection = new NpgsqlConnection(connectionString);
        _npgsqlConnection.Open(); // TODO: make it async
    }
    
    public DataTable ExecuteQuery(string query, NpgsqlParameter[] parameters = null)
    {
        using (NpgsqlCommand cmd = new NpgsqlCommand(query, _npgsqlConnection))
        {
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }
    }

    public async Task<int> ExecuteNonQuery(string query, NpgsqlParameter[] parameters = null)
    {
        using (NpgsqlCommand cmd = new NpgsqlCommand(query, _npgsqlConnection))
        {
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            return await cmd.ExecuteNonQueryAsync();
        }
    }
    
    public async Task AddAsync<T>(string tableName, T newElement)
    {
        tableName = tableName.ToLower();
        await EnsureTableExistsAsync<T>(tableName);
        await InsertDataAsync(tableName, newElement);
    }

    private async Task EnsureTableExistsAsync<T>(string tableName)
    {
        var checkTableQuery = "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = @tableName);";

        using var checkCmd = new NpgsqlCommand(checkTableQuery, _npgsqlConnection);
        checkCmd.Parameters.AddWithValue("@tableName", tableName);
        bool tableExists = (bool)await checkCmd.ExecuteScalarAsync();

        if (!tableExists)
        {
            Console.WriteLine($"Creating table: {tableName}");
            string createTableQuery = GenerateCreateTableQuery<T>(tableName);
            using var createCmd = new NpgsqlCommand(createTableQuery, _npgsqlConnection);
            await createCmd.ExecuteNonQueryAsync();
        }
    }

    private string GenerateCreateTableQuery<T>(string tableName)
    {
        var properties = typeof(T).GetProperties();
        StringBuilder query = new StringBuilder($"CREATE TABLE {tableName} (id SERIAL PRIMARY KEY, ");

        foreach (var prop in properties)
        {
            string columnType = prop.PropertyType switch
            {
                Type t when t == typeof(int) => "INTEGER",
                Type t when t == typeof(string) => "TEXT",
                Type t when t == typeof(bool) => "BOOLEAN",
                Type t when t == typeof(DateTime) => "TIMESTAMP",
                _ => "TEXT"
            };

            query.Append($"{prop.Name} {columnType}, ");
        }

        query.Length -= 2; // Remove last comma
        query.Append(");");

        return query.ToString();
    }

    private async Task InsertDataAsync<T>(string tableName, T newElement)
    {
        var properties = typeof(T).GetProperties();
        var columnNames = string.Join(", ", properties.Select(p => p.Name));
        var parameterNames = string.Join(", ", properties.Select(p => $"@{p.Name}"));

        string insertQuery = $"INSERT INTO {tableName} ({columnNames}) VALUES ({parameterNames});";

        using var cmd = new NpgsqlCommand(insertQuery, _npgsqlConnection);

        foreach (var prop in properties)
        {
            object value = prop.GetValue(newElement) ?? DBNull.Value;
            cmd.Parameters.AddWithValue($"@{prop.Name}", value);
        }

        await cmd.ExecuteNonQueryAsync();
    }
    
    public List<T> Get<T>(string tableName, Dictionary<string, (string Operator, object Value)> filters = null) where T : new()
    {
        tableName = tableName.ToLower();
        
        // query
        StringBuilder queryBuilder = new StringBuilder($"SELECT * FROM {tableName}");

        List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
        if (filters != null && filters.Count > 0)
        {
            queryBuilder.Append(" WHERE ");
            queryBuilder.Append(string.Join(" AND ", filters.Keys.Select(key => $"{key} {filters[key].Operator} @{key}")));

            foreach (var filter in filters)
            {
                parameters.Add(new NpgsqlParameter($"@{filter.Key}", filter.Value.Value));
            }
        }

        // request
        string query = queryBuilder.ToString();
        DataTable dt = ExecuteQuery(query, parameters.ToArray());

        // dto to list
        List<T> resultList = new List<T>();
        foreach (DataRow row in dt.Rows)
        {
            T obj = new T();
            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                if (dt.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                {
                    prop.SetValue(obj, Convert.ChangeType(row[prop.Name], prop.PropertyType));
                }
            }
            resultList.Add(obj);
        }

        return resultList;
    }
}