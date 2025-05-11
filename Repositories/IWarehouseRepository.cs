using Tutorial9.Models;

namespace Tutorial9.Repositories
{
    public interface IWarehouseRepository
    {
        Task<int?> AddProductToWarehouse(WarehouseRequestDTO request);
        Task<int?> AddProductToWarehouse_Proc(WarehouseRequestDTO request);
    }
}