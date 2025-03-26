using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using workshop_web_app.DataAccess; 
using workshop_web_app.Models;     
using Npgsql;

namespace workshop_web_app.Repositories
{
    public class UnitRepository : PostgresDataAccess
    {
        public UnitRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<Unit>> GetAllUnitsAsync()
        {
            var units = new List<Unit>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = "SELECT unit_id, unit_short_name, unit_full_name FROM units;";
                using (var command = new NpgsqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var unit = new Unit
                        {
                            UnitId = reader.GetInt32(0),
                            UnitShortName = reader.GetString(1),
                            UnitFullName = reader.GetString(2)
                        };
                        units.Add(unit);
                    }
                }
            }

            return units;
        }


        public async Task<Unit> GetUnitByIdAsync(int id)
        {
            Unit unit = null;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = "SELECT unit_id, unit_short_name, unit_full_name FROM units WHERE unit_id = @id;";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            unit = new Unit
                            {
                                UnitId = reader.GetInt32(0),
                                UnitShortName = reader.GetString(1),
                                UnitFullName = reader.GetString(2)
                            };
                        }
                    }
                }
            }

            return unit;
        }

        public async Task<int> AddUnitAsync(Unit unit)
        {
            int newUnitId = 0;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = @"
                    INSERT INTO units (unit_short_name, unit_full_name)
                    VALUES (@unit_short_name, @unit_full_name)
                    RETURNING unit_id;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("unit_short_name", unit.UnitShortName);
                    command.Parameters.AddWithValue("unit_full_name", unit.UnitFullName);
                    newUnitId = (int)await command.ExecuteScalarAsync();
                }
            }

            return newUnitId;
        }

        public async Task<bool> UpdateUnitAsync(Unit unit)
        {
            int affectedRows = 0;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = @"
                    UPDATE units
                    SET unit_short_name = @unit_short_name,
                        unit_full_name = @unit_full_name
                    WHERE unit_id = @unit_id;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("unit_short_name", unit.UnitShortName);
                    command.Parameters.AddWithValue("unit_full_name", unit.UnitFullName);
                    command.Parameters.AddWithValue("unit_id", unit.UnitId);
                    affectedRows = await command.ExecuteNonQueryAsync();
                }
            }

            return affectedRows > 0;
        }

        public async Task<bool> DeleteUnitAsync(int id)
        {
            int affectedRows = 0;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = "DELETE FROM units WHERE unit_id = @id;";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("id", id);
                    affectedRows = await command.ExecuteNonQueryAsync();
                }
            }

            return affectedRows > 0;
        }
    }
}