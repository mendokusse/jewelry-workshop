using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using workshop_web_app.DataAccess; 
using workshop_web_app.Models;     
using Npgsql;

namespace workshop_web_app.Repositories
{
    public class RoleRepository : PostgresDataAccess
    {
        public RoleRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            var roles = new List<Role>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = "SELECT role_id, role_name FROM roles;";
                using (var command = new NpgsqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var role = new Role
                        {
                            RoleId = reader.GetInt32(0),
                            RoleName = reader.GetString(1)
                        };
                        roles.Add(role);
                    }
                }
            }

            return roles;
        }

        public async Task<Role> GetRoleByIdAsync(int roleId)
        {
            Role role = null;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = "SELECT role_id, role_name FROM roles WHERE role_id = @roleId;";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("roleId", roleId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            role = new Role
                            {
                                RoleId = reader.GetInt32(0),
                                RoleName = reader.GetString(1)
                            };
                        }
                    }
                }
            }

            return role;
        }
    }
}