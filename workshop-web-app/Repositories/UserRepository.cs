using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using workshop_web_app.DataAccess; 
using workshop_web_app.Models;
using Npgsql;

namespace workshop_web_app.Repositories
{
    public class UserRepository : PostgresDataAccess
    {
        public UserRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = @"
                    SELECT u.user_id, u.role_id, u.user_name, u.user_password_hash, u.user_phone, u.user_email,
                           r.role_id, r.role_name
                    FROM users u
                    LEFT JOIN roles r ON u.role_id = r.role_id;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var user = new User
                        {
                            UserId = reader.GetInt32(0),
                            RoleId = reader.GetInt32(1),
                            UserName = reader.GetString(2),
                            UserPasswordHash = reader.GetString(3),
                            UserPhone = reader.IsDBNull(4) ? null : reader.GetString(4),
                            UserEmail = reader.IsDBNull(5) ? null : reader.GetString(5),
                            Role = new Role
                            {
                                RoleId = reader.GetInt32(6),
                                RoleName = reader.GetString(7)
                            }
                        };

                        users.Add(user);
                    }
                }
            }
            return users;
        }
    }
}