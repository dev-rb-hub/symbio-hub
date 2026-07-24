using System.Collections.Generic;
using System.Threading.Tasks;
using Symbio.Core.Models;

namespace Symbio.Core.Repositories
{
    public interface ITalentDiscoveryRepository
    {
        Task<IEnumerable<TalentProfile>> SearchTalentProfilesAsync(string? query = null, string? skill = null, string? location = null, int limit = 12);
        Task UpsertTalentProfileAsync(TalentProfile profile);
    }
}