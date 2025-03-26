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

        public async Task<User> GetUserByIdAsync(int userId)
        {
            User user = null;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = @"
                    SELECT 
                        u.user_id, 
                        u.role_id, 
                        u.user_name, 
                        u.user_password_hash, 
                        u.user_phone, 
                        u.user_email,
                        r.role_id, 
                        r.role_name
                    FROM users u
                    LEFT JOIN roles r ON u.role_id = r.role_id
                    WHERE u.user_id = @userId;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("userId", userId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            user = new User
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
                        }
                    }
                }
            }
            return user;
        }

        public async Task<List<User>> GetSearchUsersByNameAsync(string userName)
        {
            var users = new List<User>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = @"
                    SELECT 
                        u.user_id, 
                        u.role_id, 
                        u.user_name, 
                        u.user_password_hash, 
                        u.user_phone, 
                        u.user_email,
                        r.role_id, 
                        r.role_name
                    FROM users u
                    LEFT JOIN roles r ON u.role_id = r.role_id
                    WHERE u.user_name ILIKE @userName;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("userName", "%" + userName + "%");

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
            }
            return users;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            int affectedRows = 0;
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = @"
                    UPDATE users
                    SET role_id = @role_id,
                        user_name = @user_name,
                        user_password_hash = @user_password_hash,
                        user_phone = @user_phone,
                        user_email = @user_email
                    WHERE user_id = @user_id;
                ";
                
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("role_id", user.RoleId);
                    command.Parameters.AddWithValue("user_name", user.UserName);
                    command.Parameters.AddWithValue("user_password_hash", user.UserPasswordHash);
                    command.Parameters.AddWithValue("user_phone", (object)user.UserPhone ?? DBNull.Value);
                    command.Parameters.AddWithValue("user_email", (object)user.UserEmail ?? DBNull.Value);
                    command.Parameters.AddWithValue("user_id", user.UserId);
                    
                    affectedRows = await command.ExecuteNonQueryAsync();
                }
            }
            return affectedRows > 0;
        }

        public async Task<int> AddUserAsync(User user)
        {
            int newUserId = 0;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = @"
                    INSERT INTO users (role_id, user_name, user_password_hash, user_phone, user_email)
                    VALUES (@role_id, @user_name, @user_password_hash, @user_phone, @user_email)
                    RETURNING user_id;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("role_id", user.RoleId);
                    command.Parameters.AddWithValue("user_name", user.UserName);
                    command.Parameters.AddWithValue("user_password_hash", user.UserPasswordHash);
                    command.Parameters.AddWithValue("user_phone", (object)user.UserPhone ?? DBNull.Value);
                    command.Parameters.AddWithValue("user_email", (object)user.UserEmail ?? DBNull.Value);

                    newUserId = (int)await command.ExecuteScalarAsync();
                }
            }
            return newUserId;
        }

        public async Task<bool> DeleteUserByIdAsync(int userId)
        {
            int affectedRows = 0;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = "DELETE FROM users WHERE user_id = @userId;";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("userId", userId);
                    affectedRows = await command.ExecuteNonQueryAsync();
                }
            }

            return affectedRows > 0;
        }
    }
}