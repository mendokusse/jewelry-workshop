using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using workshop_web_app.DataAccess; 
using workshop_web_app.Models;     
using Npgsql;

namespace workshop_web_app.Repositories
{
    public class ProductTypeRepository : PostgresDataAccess
    {
        public ProductTypeRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<ProductType>> GetAllProductTypesAsync()
        {
            var productTypes = new List<ProductType>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = "SELECT product_type_id, product_type_name FROM product_types;";
                using (var command = new NpgsqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var productType = new ProductType
                        {
                            ProductTypeId = reader.GetInt32(0),
                            ProductTypeName = reader.GetString(1)
                        };
                        productTypes.Add(productType);
                    }
                }
            }

            return productTypes;
        }

        public async Task<ProductType> GetProductTypeByIdAsync(int id)
        {
            ProductType productType = null;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = "SELECT product_type_id, product_type_name FROM product_types WHERE product_type_id = @id;";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            productType = new ProductType
                            {
                                ProductTypeId = reader.GetInt32(0),
                                ProductTypeName = reader.GetString(1)
                            };
                        }
                    }
                }
            }

            return productType;
        }

        public async Task<int> AddProductTypeAsync(ProductType productType)
        {
            int newProductTypeId = 0;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = @"
                    INSERT INTO product_types (product_type_name)
                    VALUES (@product_type_name)
                    RETURNING product_type_id;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("product_type_name", productType.ProductTypeName);
                    newProductTypeId = (int)await command.ExecuteScalarAsync();
                }
            }

            return newProductTypeId;
        }

        public async Task<bool> UpdateProductTypeAsync(ProductType productType)
        {
            int affectedRows = 0;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = @"
                    UPDATE product_types
                    SET product_type_name = @product_type_name
                    WHERE product_type_id = @product_type_id;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("product_type_name", productType.ProductTypeName);
                    command.Parameters.AddWithValue("product_type_id", productType.ProductTypeId);
                    affectedRows = await command.ExecuteNonQueryAsync();
                }
            }

            return affectedRows > 0;
        }

        public async Task<bool> DeleteProductTypeAsync(int id)
        {
            int affectedRows = 0;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = "DELETE FROM product_types WHERE product_type_id = @id;";
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