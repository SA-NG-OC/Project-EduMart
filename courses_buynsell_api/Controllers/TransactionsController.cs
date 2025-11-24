using courses_buynsell_api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace courses_buynsell_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _service;

        public TransactionsController(ITransactionService service)
        {
            _service = service;
        }

        // 🟩 GET /transactions?page=1&pageSize=10
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var data = await _service.GetAllAsync(page, pageSize);
            return Ok(data);
        }

        // 🟩 GET /transactions/{transactionCode}
        [HttpGet("{transactionCode}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDetail(string transactionCode)
        {
            var data = await _service.GetByCodeAsync(transactionCode);
            if (data == null) return NotFound();
            return Ok(data);
        }

        // 🟩 GET /transactions/stats/students?page=1&pageSize=10
        [HttpGet("stats/students")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStudentStats([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var data = await _service.GetStudentStatsAsync(page, pageSize);
            return Ok(data);
        }

        // 🟩 GET /transactions/stats/courses?page=1&pageSize=10
        [HttpGet("stats/courses")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCourseStats([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var data = await _service.GetCourseStatsAsync(page, pageSize);
            return Ok(data);
        }
    }
}
