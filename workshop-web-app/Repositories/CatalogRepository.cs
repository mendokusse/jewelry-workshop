using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using workshop_web_app.DataAccess; 
using workshop_web_app.Models;
using Npgsql;

namespace workshop_web_app.Repositories
{
    public class CatalogRepository : PostgresDataAccess
    {
        public CatalogRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<Catalog>> GetCatalogItemsAsync(decimal? minPrice, decimal? maxPrice, int? productTypeId)
        {
            var items = new List<Catalog>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = @"
                    SELECT 
                        c.catalog_photo_id, 
                        c.product_type_id, 
                        c.product_name, 
                        c.product_img_path, 
                        c.product_price,
                        pt.product_type_id,
                        pt.product_type_name
                    FROM catalog c
                    LEFT JOIN product_types pt ON c.product_type_id = pt.product_type_id
                    WHERE (@minPrice IS NULL OR c.product_price >= @minPrice)
                      AND (@maxPrice IS NULL OR c.product_price <= @maxPrice)
                      AND (@productTypeId IS NULL OR c.product_type_id = @productTypeId)
                    ORDER BY c.catalog_photo_id;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.Add("minPrice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = (object)minPrice ?? DBNull.Value;
                    command.Parameters.Add("maxPrice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = (object)maxPrice ?? DBNull.Value;
                    command.Parameters.Add("productTypeId", NpgsqlTypes.NpgsqlDbType.Integer).Value = (object)productTypeId ?? DBNull.Value;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = new Catalog
                            {
                                CatalogPhotoId = reader.GetInt32(0),
                                ProductTypeId = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                                ProductName = reader.GetString(2),
                                ProductImgPath = reader.GetString(3),
                                ProductPrice = reader.GetDecimal(4)
                            };

                            if (!reader.IsDBNull(5))
                            {
                                item.ProductType = new ProductType
                                {
                                    ProductTypeId = reader.GetInt32(5),
                                    ProductTypeName = reader.GetString(6)
                                };
                            }
                            items.Add(item);
                        }
                    }
                }
            }

            return items;
        }
    }
}