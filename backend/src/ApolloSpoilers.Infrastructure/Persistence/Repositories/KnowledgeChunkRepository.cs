using ApolloSpoilers.Domain.Entities;
using ApolloSpoilers.Domain.Interfaces;
using ApolloSpoilers.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApolloSpoilers.Infrastructure.Persistence.Repositories;

public class AiKnowledgeChunkRepository : IAiKnowledgeChunkRepository
{
    private readonly ApplicationDbContext _context;

    public AiKnowledgeChunkRepository(ApplicationDbContext context) => _context = context;

    public async Task<AiKnowledgeChunk?> GetByQdrantPointIdAsync(Guid pointId, CancellationToken ct = default)
        => await _context.AiKnowledgeChunks.FirstOrDefaultAsync(c => c.QdrantPointId == pointId, ct);

    public async Task<IReadOnlyList<AiKnowledgeChunk>> GetByProductIdAsync(Guid productId, CancellationToken ct = default)
        => await _context.AiKnowledgeChunks.Where(c => c.ProductId == productId).ToListAsync(ct);

    public async Task AddAsync(AiKnowledgeChunk chunk, CancellationToken ct = default)
        => await _context.AiKnowledgeChunks.AddAsync(chunk, ct);

    public void Remove(AiKnowledgeChunk chunk) => _context.AiKnowledgeChunks.Remove(chunk);
    public void RemoveRange(IEnumerable<AiKnowledgeChunk> chunks) => _context.AiKnowledgeChunks.RemoveRange(chunks);
}
