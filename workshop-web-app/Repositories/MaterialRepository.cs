using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using workshop_web_app.DataAccess;  
using workshop_web_app.Models;      
using Npgsql;

namespace workshop_web_app.Repositories
{
    public class MaterialRepository : PostgresDataAccess
    {
        public MaterialRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<decimal> GetMaterialPriceByIdAsync(int materialId)
        {
            decimal price = 0;
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = "SELECT material_price FROM materials WHERE material_id = @materialId;";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("materialId", materialId);
                    var result = await command.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        price = (decimal)result;
                    }
                }
            }
            return price;
        }

        public async Task<List<Material>> GetAllMaterialsAsync()
        {
            var materials = new List<Material>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = @"
                    SELECT 
                        m.material_id,
                        m.unit_id,
                        m.material_price,
                        m.material_name,
                        m.material_quantity,
                        u.unit_id,
                        u.unit_short_name,
                        u.unit_full_name
                    FROM materials m
                    LEFT JOIN units u ON m.unit_id = u.unit_id;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var material = new Material
                        {
                            MaterialId = reader.GetInt32(0),
                            UnitId = reader.GetInt32(1),
                            MaterialPrice = reader.GetDecimal(2),
                            MaterialName = reader.GetString(3),
                            MaterialQuantity = reader.GetFloat(4),
                            Unit = new Models.Unit
                            {
                                UnitId = reader.GetInt32(5),
                                UnitShortName = reader.GetString(6),
                                UnitFullName = reader.GetString(7)
                            }
                        };

                        materials.Add(material);
                    }
                }
            }

            return materials;
        }

        public async Task<Material> GetMaterialByIdAsync(int id)
        {
            Material material = null;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = @"
                    SELECT 
                        m.material_id,
                        m.unit_id,
                        m.material_price,
                        m.material_name,
                        m.material_quantity,
                        u.unit_id,
                        u.unit_short_name,
                        u.unit_full_name
                    FROM materials m
                    LEFT JOIN units u ON m.unit_id = u.unit_id
                    WHERE m.material_id = @id;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            material = new Material
                            {
                                MaterialId = reader.GetInt32(0),
                                UnitId = reader.GetInt32(1),
                                MaterialPrice = reader.GetDecimal(2),
                                MaterialName = reader.GetString(3),
                                MaterialQuantity = reader.GetFloat(4),
                                Unit = new Models.Unit
                                {
                                    UnitId = reader.GetInt32(5),
                                    UnitShortName = reader.GetString(6),
                                    UnitFullName = reader.GetString(7)
                                }
                            };
                        }
                    }
                }
            }

            return material;
        }

        public async Task<int> AddMaterialAsync(Material material)
        {
            int newMaterialId = 0;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = @"
                    INSERT INTO materials (unit_id, material_price, material_name, material_quantity)
                    VALUES (@unit_id, @material_price, @material_name, @material_quantity)
                    RETURNING material_id;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("unit_id", material.UnitId);
                    command.Parameters.AddWithValue("material_price", material.MaterialPrice);
                    command.Parameters.AddWithValue("material_name", material.MaterialName);
                    command.Parameters.AddWithValue("material_quantity", material.MaterialQuantity);

                    newMaterialId = (int)await command.ExecuteScalarAsync();
                }
            }

            return newMaterialId;
        }

        public async Task<bool> UpdateMaterialAsync(Material material)
        {
            int affectedRows = 0;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = @"
                    UPDATE materials
                    SET unit_id = @unit_id,
                        material_price = @material_price,
                        material_name = @material_name,
                        material_quantity = @material_quantity
                    WHERE material_id = @material_id;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("unit_id", material.UnitId);
                    command.Parameters.AddWithValue("material_price", material.MaterialPrice);
                    command.Parameters.AddWithValue("material_name", material.MaterialName);
                    command.Parameters.AddWithValue("material_quantity", material.MaterialQuantity);
                    command.Parameters.AddWithValue("material_id", material.MaterialId);

                    affectedRows = await command.ExecuteNonQueryAsync();
                }
            }

            return affectedRows > 0;
        }

        public async Task<bool> DeleteMaterialAsync(int id)
        {
            int affectedRows = 0;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = "DELETE FROM materials WHERE material_id = @id;";
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