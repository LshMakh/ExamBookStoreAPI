using Exam.Models;
using Exam.Packages;
using Oracle.ManagedDataAccess.Client;
using System.Data;

public interface IPKG_LSH_ORDERS
{
    Task<(int orderId, string result)> CreateOrder(string customerName);
    Task<string> AddBookToOrder(int orderId, int bookId, int quantity);
    Task<string> ConfirmOrder(int orderId);
    Task<string> CancelOrder(int orderId);
    Task<List<Order>> GetPendingOrders();


}

public class PKG_LSH_ORDERS : PKG_BASE, IPKG_LSH_ORDERS
{
    public async Task<(int orderId, string result)> CreateOrder(string customerName)
    {
        using (var conn = new OracleConnection(ConnStr))
        {
            await conn.OpenAsync();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "pkg_lsh_books.create_order";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_customer_name", OracleDbType.Varchar2).Value = customerName;
                cmd.Parameters.Add("p_order_id", OracleDbType.Int32).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("p_result", OracleDbType.Varchar2, 100).Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();

                int orderId = Convert.ToInt32(cmd.Parameters["p_order_id"].Value.ToString());
                string result = cmd.Parameters["p_result"].Value.ToString();

                return (orderId, result);
            }
        }
    }

    public async Task<string> AddBookToOrder(int orderId, int bookId, int quantity)
    {
        using (var conn = new OracleConnection(ConnStr))
        {
            await conn.OpenAsync();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "pkg_lsh_books.add_book_to_order";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_order_id", OracleDbType.Int32).Value = orderId;
                cmd.Parameters.Add("p_book_id", OracleDbType.Int32).Value = bookId;
                cmd.Parameters.Add("p_quantity", OracleDbType.Int32).Value = quantity;
                cmd.Parameters.Add("p_result", OracleDbType.Varchar2, 100).Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();
                return cmd.Parameters["p_result"].Value.ToString();
            }
        }
    }

    public async Task<string> ConfirmOrder(int orderId)
    {
        using (var conn = new OracleConnection(ConnStr))
        {
            await conn.OpenAsync();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "pkg_lsh_books.confirm_order";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_order_id", OracleDbType.Int32).Value = orderId;
                cmd.Parameters.Add("p_result", OracleDbType.Varchar2, 100).Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();
                return cmd.Parameters["p_result"].Value.ToString();
            }
        }
    }

    public async Task<string> CancelOrder(int orderId)
    {
        using (var conn = new OracleConnection(ConnStr))
        {
            await conn.OpenAsync();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "pkg_lsh_books.cancel_order";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_order_id", OracleDbType.Int32).Value = orderId;
                cmd.Parameters.Add("p_result", OracleDbType.Varchar2, 100).Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();
                return cmd.Parameters["p_result"].Value.ToString();
            }
        }
    }

    public async Task<List<Order>> GetPendingOrders()
    {
        var orders = new Dictionary<int, Order>();

        using (var conn = new OracleConnection(ConnStr))
        {
            await conn.OpenAsync();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "pkg_lsh_books.get_orders_details";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new OracleParameter
                {
                    ParameterName = "p_orders_cursor",
                    OracleDbType = OracleDbType.RefCursor,
                    Direction = ParameterDirection.Output
                });

                cmd.Parameters.Add(new OracleParameter
                {
                    ParameterName = "p_result",
                    OracleDbType = OracleDbType.Varchar2,
                    Size = 1000,
                    Direction = ParameterDirection.Output
                });

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var orderId = reader.GetInt32(0);

                        if (!orders.ContainsKey(orderId))
                        {
                            orders[orderId] = new Order
                            {
                                OrderId = orderId,
                                CustomerName = reader.GetString(1),
                                TotalAmount = reader.GetDecimal(2),
                                Status = reader.GetString(3),
                                OrderDetails = new List<OrderDetail>()
                            };
                        }

                        if (!reader.IsDBNull(4))
                        {
                            orders[orderId].OrderDetails.Add(new OrderDetail
                            {
                                OrderId = orderId,
                                BookId = reader.GetInt32(4),
                                Title = reader.GetString(5),
                                Author = reader.GetString(6),
                                Quantity = reader.GetInt32(7),
                                Price = reader.GetDecimal(8)
                            });
                        }
                    }
                }
            }
        }

        return orders.Values.ToList();
    }

}
