using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Symbio.API.Data;
using Symbio.API.Hubs;
using Symbio.API.Models;
using Symbio.Core.Models;
using Symbio.Core.Repositories;

namespace Symbio.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IHubContext<MarketplaceHub> _marketplaceHubContext;
        private readonly SymbioDbContext _dbContext;

        public ProjectController(
            IProjectRepository projectRepository,
            IHubContext<MarketplaceHub> marketplaceHubContext,
            SymbioDbContext dbContext)
        {
            _projectRepository = projectRepository;
            _marketplaceHubContext = marketplaceHubContext;
            _dbContext = dbContext;
        }

        [HttpPost]
        [Authorize(Roles = "SME")]
        public async Task<IActionResult> PostProject([FromBody] ProjectScope project)
        {
            if (project == null || string.IsNullOrWhiteSpace(project.Title) || string.IsNullOrWhiteSpace(project.Description) || project.Budget <= 0)
            {
                return BadRequest(new { message = "Invalid project payload." });
            }

            project.PostedAt = DateTime.UtcNow;
            project.IsPublished = true;
            project.PaymentState = "AwaitingPayment";
            var savedProject = await _projectRepository.SaveProjectAsync(project);

            var paymentStateRecord = await _dbContext.ProjectPaymentStateRecords
                .FirstOrDefaultAsync(item => item.ProjectId == savedProject.Id);

            if (paymentStateRecord == null)
            {
                _dbContext.ProjectPaymentStateRecords.Add(new ProjectPaymentStateRecord
                {
                    ProjectId = savedProject.Id,
                    State = "AwaitingPayment",
                    GrossAmount = savedProject.Budget,
                    Currency = "AUD",
                    UpdatedAtUtc = DateTime.UtcNow
                });
                await _dbContext.SaveChangesAsync();
            }

            await _marketplaceHubContext.Clients.Group(MarketplaceHub.ExpertsGroupName).SendAsync("ProjectPublished", new
            {
                savedProject.Id,
                savedProject.Title,
                savedProject.Category,
                savedProject.Location,
                savedProject.Budget,
                savedProject.PostedAt
            });

            return CreatedAtAction(nameof(GetProject), new { id = savedProject.Id }, savedProject);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetProjects()
        {
            var projects = await _projectRepository.GetPublishedProjectsAsync();
            return Ok(projects);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProject(string id)
        {
            var projects = await _projectRepository.GetPublishedProjectsAsync();
            var project = projects.FirstOrDefault(p => p.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }
    }
}
