using System.Collections.Generic;
using System.Threading.Tasks;
using Symbio.Core.Models;

namespace Symbio.Core.Repositories
{
    public interface IProjectRepository
    {
        Task<ProjectScope> SaveProjectAsync(ProjectScope project);
        Task<IEnumerable<ProjectScope>> GetPublishedProjectsAsync();
    }
}
