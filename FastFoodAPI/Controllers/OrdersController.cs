using FastFoodAPI.Data;
using FastFoodAPI.DTOs;
using FastFoodAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFoodAPI.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly FastFoodDbContext _context;

        public OrdersController(FastFoodDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if( request.AreaId <= 0)
            {
                return BadRequest("Invalid AreaId.");
            }

            await using var transacao =
            await _context.Database.BeginTransactionAsync();

            var area = await _context.Areas.FirstOrDefaultAsync(a => a.Id == request.AreaId);

            if(area == null)
            {
                return BadRequest("Area not found.");
            }

            //increment area's last order number
            area.LastOrderNumber++;

            //create the order code ex: E0001
            string orderCode = $"{area.Acronym}{area.LastOrderNumber:D4}";

            //create the new order
            var order = new Order
            {
                Code = orderCode,
                AreaId = area.Id,
                State = OrderState.AlredyOrdered,
                CreatedOn = DateTime.UtcNow
            };

            //add the order to the database
            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            await transacao.CommitAsync();

            var response = new OrderResponse
            {
                Id = order.Id,
                Code = order.Code,
                AreaId = area.Id,
                AreaName = area.Name,
                State = order.State,
                CreatedOn = order.CreatedOn
            };

            return CreatedAtAction(nameof(GetNameById),
                new { id = order.Id }, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .OrderBy(o => o.Id)
                .Select(o => new OrderResponse
                {
                    Id = o.Id,
                    Code = o.Code,
                    AreaId = o.AreaId,
                    AreaName = o.Area.Name,
                    State = o.State,
                    CreatedOn = o.CreatedOn
                })
                .ToListAsync();

            return Ok(orders);
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderResponse>> GetNameById(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == id)
                .Select(o => new OrderResponse
                {
                    Id = o.Id,
                    Code = o.Code,
                    AreaId = o.AreaId,
                    AreaName = o.Area.Name,
                    State = o.State,
                    CreatedOn = o.CreatedOn
                })
                .FirstOrDefaultAsync();

            if(order == null)
            {
                return NotFound();
            }

            return Ok(order);

        }
    }
}
