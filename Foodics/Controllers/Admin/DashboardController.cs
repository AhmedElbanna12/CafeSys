using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers.Admin
{
    [Route("api/admin/dashboard")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("active-customers")]
        public async Task<IActionResult> GetActiveCustomers()
        {
            var fromDate = DateTime.UtcNow.AddDays(-30);

            var activeUsers = await _context.Orders
                .Where(o => o.CreatedAt >= fromDate)
                .Select(o => o.User)
                .Distinct()
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.PhoneNumber,
                    u.Email
                })
                .ToListAsync();

            return Ok(new
            {
                count = activeUsers.Count,
                users = activeUsers
            });
        }


        [HttpGet("recent-revenue")]
        public async Task<IActionResult> GetRecentRevenue()
        {
            var fromDate = DateTime.UtcNow.AddDays(-7);

            var revenue = await _context.Orders
                .Where(o => o.CreatedAt >= fromDate && o.OrderStatus == OrderStatus.Completed )
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            return Ok(new
            {
                revenue
            });
        }
    }
}
