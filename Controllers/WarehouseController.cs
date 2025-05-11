using Microsoft.AspNetCore.Mvc;
using Tutorial9.Models;
using Tutorial9.Repositories;

namespace Tutorial9.Controllers
{
    [ApiController]
    [Route("api/warehouse")]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseRepository _repository;

        public WarehouseController(IWarehouseRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("manual")]
        public async Task<IActionResult> AddProductManual([FromBody] WarehouseRequestDTO request)
        {
            var result = await _repository.AddProductToWarehouse(request);
            if (result is null) return BadRequest("Validation failed or item already added.");
            return Created("", new { IdProductWarehouse = result });
        }

        [HttpPost("procedure")]
        public async Task<IActionResult> AddProductUsingProcedure([FromBody] WarehouseRequestDTO request)
        {
            var result = await _repository.AddProductToWarehouse_Proc(request);
            if (result is null) return BadRequest("Stored procedure failed or invalid input.");
            return Created("", new { IdProductWarehouse = result });
        }
    }
}