using Symbio.Core.Models;

namespace Symbio.Core.Repositories;

public interface ICompletionEvidenceRepository
{
    Task RecordAsync(CompletionEvidenceRecord record);
    Task<IReadOnlyList<CompletionEvidenceRecord>> GetByEpicAsync(string epicId);
    Task<CompletionEvidenceMatrix> GetMatrixAsync();
}