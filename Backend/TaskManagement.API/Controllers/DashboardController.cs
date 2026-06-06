using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.Models;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole("Admin");

        var query = _context.Tasks.AsQueryable();
        if (!isAdmin)
            query = query.Where(t => t.OwnerId == userId || t.AssignedToId == userId);

        var stats = new
        {
            Total = await query.CountAsync(),
            Pending = await query.CountAsync(t => t.Status == Models.TaskStatus.Pending),
            InProgress = await query.CountAsync(t => t.Status == Models.TaskStatus.InProgress),
            Completed = await query.CountAsync(t => t.Status == Models.TaskStatus.Completed)
        };

        return Ok(stats);
    }
}