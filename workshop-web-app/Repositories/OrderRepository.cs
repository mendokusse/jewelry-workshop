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

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = new List<Order>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string sqlOrders = @"
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
                        s.status_name
                    FROM orders o
                    LEFT JOIN product_types pt ON o.product_type_id = pt.product_type_id
                    LEFT JOIN statuses s ON o.status_id = s.status_id;
                ";

                using (var cmdOrders = new NpgsqlCommand(sqlOrders, connection))
                using (var readerOrders = await cmdOrders.ExecuteReaderAsync())
                {
                    while (await readerOrders.ReadAsync())
                    {
                        var order = new Order
                        {
                            OrderId = readerOrders.GetInt32(0),
                            ProductTypeId = readerOrders.GetInt32(1),
                            CustomerUserId = readerOrders.IsDBNull(2) ? (int?)null : readerOrders.GetInt32(2),
                            ManagerUserId = readerOrders.IsDBNull(3) ? (int?)null : readerOrders.GetInt32(3),
                            JewelerUserId = readerOrders.IsDBNull(4) ? (int?)null : readerOrders.GetInt32(4),
                            StatusId = readerOrders.GetInt32(5),
                            OrderComment = readerOrders.IsDBNull(6) ? null : readerOrders.GetString(6),
                            OrderPrice = readerOrders.IsDBNull(7) ? (decimal?)null : readerOrders.GetDecimal(7),
                            OrderDate = readerOrders.IsDBNull(8) ? (DateTime?)null : readerOrders.GetDateTime(8),
                            OrderUpdateDate = readerOrders.IsDBNull(9) ? (DateTime?)null : readerOrders.GetDateTime(9),
                            ProductType = new ProductType
                            {
                                ProductTypeId = readerOrders.GetInt32(10),
                                ProductTypeName = readerOrders.GetString(11)
                            },
                            Status = new Status
                            {
                                StatusId = readerOrders.GetInt32(12),
                                StatusName = readerOrders.GetString(13)
                            }
                        };

                        orders.Add(order);
                    }
                }

                // Получаем детали всех заказов
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
                    LEFT JOIN units u ON m.unit_id = u.unit_id;
                ";

                using (var cmdDetails = new NpgsqlCommand(sqlDetails, connection))
                using (var readerDetails = await cmdDetails.ExecuteReaderAsync())
                {
                    var orderDetailsDict = new Dictionary<int, List<OrderDetail>>();

                    while (await readerDetails.ReadAsync())
                    {
                        var detail = new OrderDetail
                        {
                            DetailsListId = readerDetails.GetInt32(0),
                            OrderId = readerDetails.GetInt32(1),
                            MaterialId = readerDetails.GetInt32(2),
                            OrderMaterialWeight = readerDetails.GetFloat(3),
                            Material = new Material
                            {
                                MaterialId = readerDetails.GetInt32(4),
                                MaterialName = readerDetails.GetString(5),
                                MaterialPrice = readerDetails.GetDecimal(6),
                                MaterialQuantity = readerDetails.GetFloat(7),
                                Unit = new Unit
                                {
                                    UnitId = readerDetails.GetInt32(8),
                                    UnitShortName = readerDetails.GetString(9),
                                    UnitFullName = readerDetails.GetString(10)
                                }
                            }
                        };

                        if (!orderDetailsDict.ContainsKey(detail.OrderId))
                        {
                            orderDetailsDict[detail.OrderId] = new List<OrderDetail>();
                        }
                        orderDetailsDict[detail.OrderId].Add(detail);
                    }

                    // Заполняем навигационное свойство OrderDetails для каждого заказа
                    foreach (var order in orders)
                    {
                        if (orderDetailsDict.ContainsKey(order.OrderId))
                        {
                            order.OrderDetails = orderDetailsDict[order.OrderId];
                        }
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

                string sqlOrder = @"
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
                        s.status_name
                    FROM orders o
                    LEFT JOIN product_types pt ON o.product_type_id = pt.product_type_id
                    LEFT JOIN statuses s ON o.status_id = s.status_id
                    WHERE o.order_id = @orderId;
                ";

                using (var cmdOrder = new NpgsqlCommand(sqlOrder, connection))
                {
                    cmdOrder.Parameters.AddWithValue("orderId", orderId);
                    using (var readerOrder = await cmdOrder.ExecuteReaderAsync())
                    {
                        if (await readerOrder.ReadAsync())
                        {
                            order = new Order
                            {
                                OrderId = readerOrder.GetInt32(0),
                                ProductTypeId = readerOrder.GetInt32(1),
                                CustomerUserId = readerOrder.IsDBNull(2) ? (int?)null : readerOrder.GetInt32(2),
                                ManagerUserId = readerOrder.IsDBNull(3) ? (int?)null : readerOrder.GetInt32(3),
                                JewelerUserId = readerOrder.IsDBNull(4) ? (int?)null : readerOrder.GetInt32(4),
                                StatusId = readerOrder.GetInt32(5),
                                OrderComment = readerOrder.IsDBNull(6) ? null : readerOrder.GetString(6),
                                OrderPrice = readerOrder.IsDBNull(7) ? (decimal?)null : readerOrder.GetDecimal(7),
                                OrderDate = readerOrder.IsDBNull(8) ? (DateTime?)null : readerOrder.GetDateTime(8),
                                OrderUpdateDate = readerOrder.IsDBNull(9) ? (DateTime?)null : readerOrder.GetDateTime(9),
                                ProductType = new ProductType
                                {
                                    ProductTypeId = readerOrder.GetInt32(10),
                                    ProductTypeName = readerOrder.GetString(11)
                                },
                                Status = new Status
                                {
                                    StatusId = readerOrder.GetInt32(12),
                                    StatusName = readerOrder.GetString(13)
                                }
                            };
                        }
                    }
                }

                if (order != null)
                {
                    order.OrderDetails = await GetOrderDetailsByOrderIdAsync(orderId, connection);
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

                string sql = @"
                    INSERT INTO order_details (order_id, material_id, order_material_weight)
                    VALUES (@order_id, @material_id, @order_material_weight)
                    RETURNING details_list_id;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("order_id", detail.OrderId);
                    command.Parameters.AddWithValue("material_id", detail.MaterialId);
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