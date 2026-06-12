using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Domain.Repositories
{
    public interface  IResourceRepository
    {
        Task <Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task <IReadOnlyList<Resource>> GetAllAsync (CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Resource>> GetByTypeAsync(ResourceType type, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Resource>> GetAvailableAsync (CancellationToken cancellationToken = default);
        Task AddAsync(Resource resource, CancellationToken cancellationToken = default);
        Task UpdateAsync ( Resource resource, CancellationToken cancellationToken = default);
        Task DeleteAsync (Guid id , CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    }
}
