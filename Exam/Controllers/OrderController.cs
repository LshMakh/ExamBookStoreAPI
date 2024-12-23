using Exam.Dtos;
using Exam.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Exam.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IPKG_LSH_ORDERS _orderPackage;

        public OrderController(IPKG_LSH_ORDERS orderPackage)
        {
            _orderPackage = orderPackage;
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrder(OrderRequest request)
        {
            var (orderId, result) = await _orderPackage.CreateOrder(request.CustomerName);
            if (!result.Equals("SUCCESS"))
                return BadRequest(result);

            foreach (var item in request.Items)
            {
                var addResult = await _orderPackage.AddBookToOrder(orderId, item.BookId, item.Quantity);
                if (!addResult.Equals("SUCCESS"))
                {
                    await _orderPackage.CancelOrder(orderId);
                    return BadRequest($"Failed to add book {item.BookId}: {addResult}");
                }
            }

            return Ok(new { OrderId = orderId });
        }

        [HttpGet("pending")]
        public async Task<ActionResult<List<Order>>> GetPendingOrders()
        {
            var orders = await _orderPackage.GetPendingOrders();
            return Ok(orders);
        }

     

        [HttpPost("{orderId}/confirm")]
        public async Task<ActionResult> ConfirmOrder(int orderId)
        {
            var result = await _orderPackage.ConfirmOrder(orderId);
            if (result.Equals("SUCCESS"))
                return Ok();
            return BadRequest(result);
        }

        [HttpPost("{orderId}/cancel")]
        public async Task<ActionResult> CancelOrder(int orderId)
        {
            var result = await _orderPackage.CancelOrder(orderId);
            if (result.Equals("SUCCESS"))
                return Ok();
            return BadRequest(result);
        }
    
    }
}
