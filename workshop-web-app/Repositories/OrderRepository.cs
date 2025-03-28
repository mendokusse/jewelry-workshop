using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using workshop_web_app.DataAccess; 
using workshop_web_app.Models;     
using Npgsql;

namespace workshop_web_app.Repositories
{
    public class OrderRepository : PostgresDataAccess
    {
        public OrderRepository(IConfiguration configuration) : base(configuration)
        {
        }

        // Метод для получения всех заказов с данными о пользователях
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = new List<Order>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = @"
                    SELECT 
                        o.order_id,
                        o.product_type_id,
                        o.customer_user_id,
                        o.manager_user_id,
                        o.jeweler_user_id,
                        o.status_id,
                        o.order_comment,
                        o.order_price,
                        o.order_date,
                        o.order_update_date,
                        pt.product_type_id,
                        pt.product_type_name,
                        s.status_id,
                        s.status_name,
                        cu.user_id,
                        cu.user_name,
                        cu.user_email,
                        mu.user_id,
                        mu.user_name,
                        mu.user_email,
                        ju.user_id,
                        ju.user_name,
                        ju.user_email
                    FROM orders o
                    LEFT JOIN product_types pt ON o.product_type_id = pt.product_type_id
                    LEFT JOIN statuses s ON o.status_id = s.status_id
                    LEFT JOIN users cu ON o.customer_user_id = cu.user_id
                    LEFT JOIN users mu ON o.manager_user_id = mu.user_id
                    LEFT JOIN users ju ON o.jeweler_user_id = ju.user_id
                    ORDER BY o.order_id;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var order = new Order
                        {
                            OrderId = reader.GetInt32(0),
                            ProductTypeId = reader.GetInt32(1),
                            CustomerUserId = reader.IsDBNull(2) ? default(int?) : reader.GetInt32(2),
                            ManagerUserId = reader.IsDBNull(3) ? default(int?) : reader.GetInt32(3),
                            JewelerUserId = reader.IsDBNull(4) ? default(int?) : reader.GetInt32(4),
                            StatusId = reader.GetInt32(5),
                            OrderComment = reader.IsDBNull(6) ? default(string?) : reader.GetString(6),
                            OrderPrice = reader.GetDecimal(7),
                            OrderDate = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                            OrderUpdateDate = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9),
                            ProductType = new ProductType
                            {
                                ProductTypeId = reader.GetInt32(10),
                                ProductTypeName = reader.GetString(11)
                            },
                            Status = new Status
                            {
                                StatusId = reader.GetInt32(12),
                                StatusName = reader.GetString(13)
                            }
                        };

                        if (!reader.IsDBNull(14))
                        {
                            order.CustomerUser = new User
                            {
                                UserId = reader.GetInt32(14),
                                UserName = reader.GetString(15),
                                UserEmail = reader.GetString(16)
                            };
                        }

                        if (!reader.IsDBNull(17))
                        {
                            order.ManagerUser = new User
                            {
                                UserId = reader.GetInt32(17),
                                UserName = reader.GetString(18),
                                UserEmail = reader.GetString(19)
                            };
                        }

                        if (!reader.IsDBNull(20))
                        {
                            order.JewelerUser = new User
                            {
                                UserId = reader.GetInt32(20),
                                UserName = reader.GetString(21),
                                UserEmail = reader.GetString(22)
                            };
                        }
                        orders.Add(order);
                    }
                }
            }

            return orders;
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            Order order = null;
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = @"
                    SELECT 
                        o.order_id,
                        o.product_type_id,
                        o.customer_user_id,
                        o.manager_user_id,
                        o.jeweler_user_id,
                        o.status_id,
                        o.order_comment,
                        o.order_price,
                        o.order_date,
                        o.order_update_date,
                        pt.product_type_id,
                        pt.product_type_name,
                        s.status_id,
                        s.status_name,
                        cu.user_id,
                        cu.user_name,
                        cu.user_email,
                        mu.user_id,
                        mu.user_name,
                        mu.user_email,
                        ju.user_id,
                        ju.user_name,
                        ju.user_email
                    FROM orders o
                    LEFT JOIN product_types pt ON o.product_type_id = pt.product_type_id
                    LEFT JOIN statuses s ON o.status_id = s.status_id
                    LEFT JOIN users cu ON o.customer_user_id = cu.user_id
                    LEFT JOIN users mu ON o.manager_user_id = mu.user_id
                    LEFT JOIN users ju ON o.jeweler_user_id = ju.user_id
                    WHERE o.order_id = @orderId;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("orderId", orderId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            order = new Order
                            {
                                OrderId = reader.GetInt32(0),
                                ProductTypeId = reader.GetInt32(1),
                                CustomerUserId = reader.IsDBNull(2) ? default(int?) : reader.GetInt32(2),
                                ManagerUserId = reader.IsDBNull(3) ? default(int?) : reader.GetInt32(3),
                                JewelerUserId = reader.IsDBNull(4) ? default(int?) : reader.GetInt32(4),
                                StatusId = reader.GetInt32(5),
                                OrderComment = reader.IsDBNull(6) ? default(string) : reader.GetString(6),
                                OrderPrice = reader.GetDecimal(7),
                                OrderDate = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                                OrderUpdateDate = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9),
                                ProductType = new ProductType
                                {
                                    ProductTypeId = reader.GetInt32(10),
                                    ProductTypeName = reader.GetString(11)
                                },
                                Status = new Status
                                {
                                    StatusId = reader.GetInt32(12),
                                    StatusName = reader.GetString(13)
                                }
                            };

                            if (!reader.IsDBNull(14))
                            {
                                order.CustomerUser = new User
                                {
                                    UserId = reader.GetInt32(14),
                                    UserName = reader.GetString(15),
                                    UserEmail = reader.GetString(16)
                                };
                            }

                            if (!reader.IsDBNull(17))
                            {
                                order.ManagerUser = new User
                                {
                                    UserId = reader.GetInt32(17),
                                    UserName = reader.GetString(18),
                                    UserEmail = reader.GetString(19)
                                };
                            }

                            if (!reader.IsDBNull(20))
                            {
                                order.JewelerUser = new User
                                {
                                    UserId = reader.GetInt32(20),
                                    UserName = reader.GetString(21),
                                    UserEmail = reader.GetString(22)
                                };
                            }
                        }
                    }
                }
                // Теперь, после закрытия первого reader, запрашиваем детали заказа.
                if (order != null)
                {
                    order.OrderDetails = await GetOrderDetailsByOrderIdAsync(order.OrderId, connection);
                }
            }
            return order;
        }

        public async Task<int> AddOrderAsync(Order order)
        {
            int newOrderId = 0;
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = @"
                    INSERT INTO orders (product_type_id, customer_user_id, manager_user_id, jeweler_user_id, status_id, order_comment, order_price, order_date, order_update_date)
                    VALUES (@product_type_id, @customer_user_id, @manager_user_id, @jeweler_user_id, @status_id, @order_comment, @order_price, @order_date, @order_update_date)
                    RETURNING order_id;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("product_type_id", order.ProductTypeId);
                    command.Parameters.AddWithValue("customer_user_id", (object)order.CustomerUserId ?? DBNull.Value);
                    command.Parameters.AddWithValue("manager_user_id", (object)order.ManagerUserId ?? DBNull.Value);
                    command.Parameters.AddWithValue("jeweler_user_id", (object)order.JewelerUserId ?? DBNull.Value);
                    command.Parameters.AddWithValue("status_id", order.StatusId);
                    command.Parameters.AddWithValue("order_comment", (object)order.OrderComment ?? DBNull.Value);
                    command.Parameters.AddWithValue("order_price", (object)order.OrderPrice ?? DBNull.Value);
                    command.Parameters.AddWithValue("order_date", (object)order.OrderDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("order_update_date", (object)order.OrderUpdateDate ?? DBNull.Value);

                    newOrderId = (int)await command.ExecuteScalarAsync();
                }
            }
            return newOrderId;
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            int affectedRows = 0;
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = "DELETE FROM orders WHERE order_id = @orderId;";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("orderId", orderId);
                    affectedRows = await command.ExecuteNonQueryAsync();
                }
            }
            return affectedRows > 0;
        }

        // Добавить новую деталь заказа
        public async Task<int> AddOrderDetailAsync(OrderDetail detail)
        {
            int newDetailId = 0;
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                // Если объект Material задан, но MaterialId не установлен (0), пытаемся найти его по имени
                if (detail.Material != null && detail.Material.MaterialId == 0)
                {
                    // Попытка найти материал по имени
                    string sqlFind = "SELECT material_id FROM materials WHERE material_name = @material_name LIMIT 1;";
                    using (var cmdFind = new NpgsqlCommand(sqlFind, connection))
                    {
                        cmdFind.Parameters.AddWithValue("material_name", detail.Material.MaterialName);
                        var result = await cmdFind.ExecuteScalarAsync();
                        if (result != null)
                        {
                            detail.Material.MaterialId = (int)result;
                        }
                        else
                        {
                            // Если материал не найден, вставляем новый с дефолтными значениями.
                            // Обратите внимание: здесь мы используем фиксированные значения для unit_id, material_price, material_quantity,
                            // их нужно настроить под вашу логику.
                            string sqlInsertMaterial = @"
                                INSERT INTO materials (unit_id, material_price, material_name, material_quantity)
                                VALUES (@unit_id, @material_price, @material_name, @material_quantity)
                                RETURNING material_id;
                            ";
                            using (var cmdInsert = new NpgsqlCommand(sqlInsertMaterial, connection))
                            {
                                // Здесь можно задать unit_id, material_price и material_quantity по умолчанию.
                                cmdInsert.Parameters.AddWithValue("unit_id", 1);
                                cmdInsert.Parameters.AddWithValue("material_price", 0m);
                                cmdInsert.Parameters.AddWithValue("material_name", detail.Material.MaterialName);
                                cmdInsert.Parameters.AddWithValue("material_quantity", 0f);
                                detail.Material.MaterialId = (int)await cmdInsert.ExecuteScalarAsync();
                            }
                        }
                    }
                }

                string sql = @"
                    INSERT INTO order_details (order_id, material_id, order_material_weight)
                    VALUES (@order_id, @material_id, @order_material_weight)
                    RETURNING details_list_id;
                ";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("order_id", detail.OrderId);
                    command.Parameters.AddWithValue("material_id", detail.Material != null ? detail.Material.MaterialId : (object)DBNull.Value);
                    command.Parameters.AddWithValue("order_material_weight", detail.OrderMaterialWeight);

                    newDetailId = (int)await command.ExecuteScalarAsync();
                }
            }
            return newDetailId;
        }

        public async Task<bool> DeleteOrderDetailAsync(int detailsListId)
        {
            int affectedRows = 0;
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sql = "DELETE FROM order_details WHERE details_list_id = @detailsListId;";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("detailsListId", detailsListId);
                    affectedRows = await command.ExecuteNonQueryAsync();
                }
            }
            return affectedRows > 0;
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            int affectedRows = 0;
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string sql = @"
                    UPDATE orders
                    SET product_type_id = @product_type_id,
                        customer_user_id = @customer_user_id,
                        manager_user_id = @manager_user_id,
                        jeweler_user_id = @jeweler_user_id,
                        status_id = @status_id,
                        order_comment = @order_comment,
                        order_price = @order_price,
                        order_date = @order_date,
                        order_update_date = @order_update_date
                    WHERE order_id = @order_id;
                ";

                order.OrderUpdateDate = DateTime.Now;
                
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("product_type_id", order.ProductTypeId);
                    command.Parameters.AddWithValue("customer_user_id", (object)order.CustomerUserId ?? DBNull.Value);
                    command.Parameters.AddWithValue("manager_user_id", (object)order.ManagerUserId ?? DBNull.Value);
                    command.Parameters.AddWithValue("jeweler_user_id", (object)order.JewelerUserId ?? DBNull.Value);
                    command.Parameters.AddWithValue("status_id", order.StatusId);
                    command.Parameters.AddWithValue("order_comment", (object)order.OrderComment ?? DBNull.Value);
                    command.Parameters.AddWithValue("order_price", (object)order.OrderPrice ?? DBNull.Value);
                    command.Parameters.AddWithValue("order_date", (object)order.OrderDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("order_update_date", (object)order.OrderUpdateDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("order_id", order.OrderId);

                    affectedRows = await command.ExecuteNonQueryAsync();
                }
            }
            return affectedRows > 0;
        }

        private async Task<List<OrderDetail>> GetOrderDetailsByOrderIdAsync(int orderId, NpgsqlConnection connection)
        {
            var details = new List<OrderDetail>();

            string sqlDetails = @"
                SELECT 
                    od.details_list_id,
                    od.order_id,
                    od.material_id,
                    od.order_material_weight,
                    m.material_id,
                    m.material_name,
                    m.material_price,
                    m.material_quantity,
                    u.unit_id,
                    u.unit_short_name,
                    u.unit_full_name
                FROM order_details od
                LEFT JOIN materials m ON od.material_id = m.material_id
                LEFT JOIN units u ON m.unit_id = u.unit_id
                WHERE od.order_id = @orderId;
            ";

            using (var cmd = new NpgsqlCommand(sqlDetails, connection))
            {
                cmd.Parameters.AddWithValue("orderId", orderId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var detail = new OrderDetail
                        {
                            DetailsListId = reader.GetInt32(0),
                            OrderId = reader.GetInt32(1),
                            MaterialId = reader.GetInt32(2),
                            OrderMaterialWeight = reader.GetFloat(3),
                            Material = new Material
                            {
                                MaterialId = reader.GetInt32(4),
                                MaterialName = reader.GetString(5),
                                MaterialPrice = reader.GetDecimal(6),
                                MaterialQuantity = reader.GetFloat(7),
                                Unit = new Unit
                                {
                                    UnitId = reader.GetInt32(8),
                                    UnitShortName = reader.GetString(9),
                                    UnitFullName = reader.GetString(10)
                                }
                            }
                        };

                        details.Add(detail);
                    }
                }
            }

            return details;
        }
    }
}