using Microsoft.Data.SqlClient;
using Tutorial9.Models;

namespace Tutorial9.Repositories
{
    public class SqlWarehouseRepository : IWarehouseRepository
    {
        private readonly IConfiguration _config;

        public SqlWarehouseRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<int?> AddProductToWarehouse(WarehouseRequestDTO request)
        {
            using var conn = new SqlConnection(_config.GetConnectionString("Default"));
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();

            try
            {
                var cmd = new SqlCommand("SELECT Price FROM Product WHERE IdProduct = @id", conn, tran);
                cmd.Parameters.AddWithValue("@id", request.IdProduct);
                var priceObj = await cmd.ExecuteScalarAsync();
                if (priceObj is null) return null;
                var price = Convert.ToDecimal(priceObj);

                cmd = new SqlCommand("SELECT 1 FROM Warehouse WHERE IdWarehouse = @id", conn, tran);
                cmd.Parameters.AddWithValue("@id", request.IdWarehouse);
                if (await cmd.ExecuteScalarAsync() is null) return null;

                cmd = new SqlCommand(@"
                    SELECT IdOrder FROM [Order] 
                    WHERE IdProduct = @idProd AND Amount = @amount AND CreatedAt < @createdAt", conn, tran);
                cmd.Parameters.AddWithValue("@idProd", request.IdProduct);
                cmd.Parameters.AddWithValue("@amount", request.Amount);
                cmd.Parameters.AddWithValue("@createdAt", request.CreatedAt);
                var orderIdObj = await cmd.ExecuteScalarAsync();
                if (orderIdObj is null) return null;
                var orderId = Convert.ToInt32(orderIdObj);

                cmd = new SqlCommand("SELECT 1 FROM Product_Warehouse WHERE IdOrder = @id", conn, tran);
                cmd.Parameters.AddWithValue("@id", orderId);
                if (await cmd.ExecuteScalarAsync() is not null) return null;

                cmd = new SqlCommand("UPDATE [Order] SET FulfilledAt = GETDATE() WHERE IdOrder = @id", conn, tran);
                cmd.Parameters.AddWithValue("@id", orderId);
                await cmd.ExecuteNonQueryAsync();

                cmd = new SqlCommand(@"
                    INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                    VALUES (@wid, @pid, @oid, @amount, @price, GETDATE());
                    SELECT SCOPE_IDENTITY();", conn, tran);

                cmd.Parameters.AddWithValue("@wid", request.IdWarehouse);
                cmd.Parameters.AddWithValue("@pid", request.IdProduct);
                cmd.Parameters.AddWithValue("@oid", orderId);
                cmd.Parameters.AddWithValue("@amount", request.Amount);
                cmd.Parameters.AddWithValue("@price", price * request.Amount);

                var result = await cmd.ExecuteScalarAsync();
                await tran.CommitAsync();

                return Convert.ToInt32(result);
            }
            catch
            {
                await tran.RollbackAsync();
                return null;
            }
        }

        public async Task<int?> AddProductToWarehouse_Proc(WarehouseRequestDTO request)
        {
            using var conn = new SqlConnection(_config.GetConnectionString("Default"));
            await conn.OpenAsync();

            var cmd = new SqlCommand("AddProductToWarehouse", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            cmd.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
            cmd.Parameters.AddWithValue("@Amount", request.Amount);
            cmd.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

            var result = await cmd.ExecuteScalarAsync();
            return result is null ? null : Convert.ToInt32(result);
        }
    }
}