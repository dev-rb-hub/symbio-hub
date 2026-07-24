using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Symbio.API.Data;

namespace Symbio.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly SymbioDbContext _dbContext;

        public JobsController(SymbioDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public IActionResult GetPublicJobs()
        {
            var jobs = _dbContext.Jobs
                .Where(job => job.IsPublished)
                .OrderByDescending(job => job.PostedAt)
                .Select(job => new
                {
                    job.Id,
                    job.Title,
                    job.Description,
                    ClientName = job.ClientName,
                    ClientSurname = "***",
                    Budget = job.Budget >= 10000 ? "Confidential budget" : "Budget available on request",
                    ContactEmail = "Hidden until auth",
                    PostedAt = job.PostedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                })
                .ToList();

            return Ok(jobs);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult Get(int id)
        {
            var job = _dbContext.Jobs.Find(id);
            if (job == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                job.Id,
                job.Title,
                job.Description,
                ClientName = job.ClientName,
                ClientSurname = "***",
                Budget = job.Budget >= 10000 ? "Confidential budget" : "Budget available on request",
                ContactEmail = "Hidden until auth",
                PostedAt = job.PostedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            });
        }
    }
}
