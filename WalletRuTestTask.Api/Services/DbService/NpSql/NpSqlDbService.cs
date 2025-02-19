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
    
    public async Task Add<T>(string tableName, T newElement)
    {
        Type type = typeof(T);
        PropertyInfo[] properties = type.GetProperties();

        // column names and parameter names
        string[] columns = properties.Select(p => p.Name).ToArray();
        string[] paramNames = properties.Select(p => "@" + p.Name).ToArray();

        // query
        string query = $"INSERT INTO {tableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", paramNames)})";

        // parameters
        List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
        foreach (PropertyInfo prop in properties)
        {
            object value = prop.GetValue(newElement) ?? DBNull.Value; // Handle null values
            parameters.Add(new NpgsqlParameter("@" + prop.Name, value));
        }

        // request
        await ExecuteNonQuery(query, parameters.ToArray());
    }
    
    public List<T> Get<T>(string tableName, Dictionary<string, (string Operator, object Value)> filters = null) where T : new()
    {
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