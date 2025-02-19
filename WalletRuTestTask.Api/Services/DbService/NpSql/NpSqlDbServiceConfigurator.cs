namespace WalletRuTestTask.Api.Services.DbService.NpSql;

public static class NpSqlDbServiceConfigurator
{
    public static void AddNpSqlDbService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddNpSqlDbService(configuration.GetConnectionString("PostgresConnection"));
    }
    
    public static void AddNpSqlDbService(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton(new NpSqlDbService(connectionString));
    }
}