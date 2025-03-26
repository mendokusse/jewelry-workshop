using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using workshop_web_app.DataAccess; 
using workshop_web_app.Models;     
using Npgsql;

namespace workshop_web_app.Repositories
{
    public class StatusRepository : PostgresDataAccess
    {
        public StatusRepository(IConfiguration configuration) : base(configuration)
        {
        }


        public async Task<List<Status>> GetAllStatusesAsync()
        {
            var statuses = new List<Status>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = "SELECT status_id, status_name FROM statuses;";
                using (var command = new NpgsqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var status = new Status
                        {
                            StatusId = reader.GetInt32(0),
                            StatusName = reader.GetString(1)
                        };
                        statuses.Add(status);
                    }
                }
            }

            return statuses;
        }

        public async Task<Status> GetStatusByIdAsync(int statusId)
        {
            Status status = null;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = "SELECT status_id, status_name FROM statuses WHERE status_id = @statusId;";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("statusId", statusId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            status = new Status
                            {
                                StatusId = reader.GetInt32(0),
                                StatusName = reader.GetString(1)
                            };
                        }
                    }
                }
            }

            return status;
        }
    }
}