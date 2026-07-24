using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Symbio.Core.Models;
using Symbio.Core.Repositories;

namespace Symbio.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectController(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
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
            var savedProject = await _projectRepository.SaveProjectAsync(project);
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
