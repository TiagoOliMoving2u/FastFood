using FastFoodAPI.DTOs;
using FastFoodAPI.Hubs;
using FastFoodAPI.Exceptions;
using FastFoodAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FastFoodAPI.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly IHubContext<OrdersHub> _hubContext;
        private readonly ILogger<OrdersController> _logger;
        public OrdersController(OrderService orderService,
            IHubContext<OrdersHub> hubContext, 
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _hubContext = hubContext;
            _logger = logger;
        }

        //--- Get: api/orders

        [HttpGet]
        public async Task<IActionResult> GetOrders(
            [FromQuery] int? areaId = null,
            [FromQuery] int? state = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var orders = await _orderService.GetOrdersAsyn(areaId, state, cancellationToken);
                return Ok(orders);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao processar pedidos.");

                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Não foi possível concluir a operação.");
            }
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderResponse>> GetOrderById(int id, CancellationToken cancellationToken)
        {
            try {
                var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);

                if (order is null)
                    return NotFound();

                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao processar pedidos.");

                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Não foi possível concluir a operação.");
            }
        }

        [HttpGet("StateHistory")]
        public async Task<IActionResult> GetStateHistory(
            [FromQuery] int? orderId = null,
            [FromQuery] int? areaId = null,
            [FromQuery] int? oldState = null,
            [FromQuery] int? newState = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var history = await _orderService.GetStateHistoryAsync(orderId, areaId, oldState, newState, cancellationToken);
                if (history == null) return NotFound();
                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao processar pedidos.");

                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Não foi possível concluir a operação.");
            }

        }

        //--- Post: api/orders

        [HttpPost]
        public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(request, cancellationToken);

                await NotifyAsync("OrdersChanged");

                await NotifyAsync("CreatedNewOrder");

                return CreatedAtAction(nameof(GetOrderById),
                    new { id = order.Id },
                    order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao processar pedidos.");

                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Não foi possível concluir a operação.");
            }
        }

        //--- Put: api/orders/{id}

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<OrderResponse>> UpdateOrder(int id, [FromQuery] int operation, [FromQuery] int currentState, CancellationToken cancellationToken)
        {
            try
            {
                var updatedOrder = await _orderService.UpdateOrderAsync(
                    id,
                    operation,
                    currentState,
                    User,
                    cancellationToken);

                await NotifyAsync("OrdersChanged");

                return Ok(updatedOrder);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (OrderConflictException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private async Task NotifyAsync(string eventName)
        {
            try
            {
                using var timeout = new CancellationTokenSource(
                    TimeSpan.FromSeconds(5));

                await _hubContext.Clients.All.SendAsync(
                    eventName,
                    timeout.Token);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Falha ao enviar o evento {EventName}.",
                    eventName);
            }
        }

    }
}
