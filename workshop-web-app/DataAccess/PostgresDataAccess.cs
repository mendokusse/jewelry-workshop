using System;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace workshop_web_app.DataAccess
{
    public class PostgresDataAccess
    {
        public readonly string _connectionString;

        public PostgresDataAccess(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        protected NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}