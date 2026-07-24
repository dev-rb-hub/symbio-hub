using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Symbio.API.Data;
using Symbio.API.Hubs;
using Symbio.API.Models;

namespace Symbio.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Expert")]
    public class ExpertWorkbenchController : ControllerBase
    {
        private readonly SymbioDbContext _dbContext;
        private readonly IHubContext<DeliveryWorkbenchHub> _hubContext;

        public ExpertWorkbenchController(SymbioDbContext dbContext, IHubContext<DeliveryWorkbenchHub> hubContext)
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
        }

        public record WorkbenchLogRequest(int DeliveryAssignmentId, string Message, string? Level, int? ProgressPercent, string? Status);

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized();
            }

            var assignments = await _dbContext.DeliveryAssignments
                .Where(item => item.ExpertEmail == email && item.IsActive)
                .OrderBy(item => item.DueDate)
                .ToListAsync();

            var assignmentIds = assignments.Select(item => item.Id).ToList();
            var recentLogs = await _dbContext.DeliveryLogs
                .Where(log => assignmentIds.Contains(log.DeliveryAssignmentId))
                .OrderByDescending(log => log.CreatedAt)
                .Take(20)
                .ToListAsync();

            return Ok(new
            {
                expertEmail = email,
                assignments,
                recentLogs
            });
        }

        [HttpPost("logs")]
        public async Task<IActionResult> CreateLog([FromBody] WorkbenchLogRequest request)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized();
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { message = "A log message is required." });
            }

            var assignment = await _dbContext.DeliveryAssignments.FirstOrDefaultAsync(item => item.Id == request.DeliveryAssignmentId && item.ExpertEmail == email && item.IsActive);
            if (assignment == null)
            {
                return NotFound(new { message = "Delivery assignment not found." });
            }

            if (request.ProgressPercent.HasValue)
            {
                assignment.ProgressPercent = Math.Clamp(request.ProgressPercent.Value, 0, 100);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                assignment.Status = request.Status.Trim();
            }

            assignment.UpdatedAt = DateTime.UtcNow;

            var log = new DeliveryLogEntry
            {
                DeliveryAssignmentId = assignment.Id,
                ExpertEmail = email,
                CreatedByEmail = email,
                Level = string.IsNullOrWhiteSpace(request.Level) ? "info" : request.Level.Trim().ToLowerInvariant(),
                Message = request.Message.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.DeliveryLogs.Add(log);
            await _dbContext.SaveChangesAsync();

            var payload = new
            {
                log.Id,
                log.DeliveryAssignmentId,
                log.ExpertEmail,
                log.CreatedByEmail,
                log.Level,
                log.Message,
                log.CreatedAt,
                assignment.ProgressPercent,
                assignment.Status,
                assignment.CurrentMilestone,
                assignment.ProjectTitle
            };

            await _hubContext.Clients.Group(DeliveryWorkbenchHub.GetGroupName(email)).SendAsync("WorkbenchLogCreated", payload);

            return Ok(payload);
        }
    }
}