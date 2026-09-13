using Microsoft.AspNetCore.Mvc;
using Users.Application.Services;

namespace Users.Presentation.Controllers
{
    [ApiController]
    [Route("api/userOrderSummary")]
    public class UserOrderSummaryController : ControllerBase
    {
        private readonly IUserOrderSummaryService _userOrderSummaryService;

        public UserOrderSummaryController(IUserOrderSummaryService userOrderSummaryService)
        {
            _userOrderSummaryService = userOrderSummaryService;
        }

        [HttpGet("GetAllOrdersSummary")]
        public async Task<IActionResult> GetAllOrdersSummary(CancellationToken cancellation)
        {
            return Ok(await _userOrderSummaryService.GetAllOrdersSummary(cancellation));
        }
    }
}
