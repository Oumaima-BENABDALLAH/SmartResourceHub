using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Catalog.Infrastructure.Persistence.Repositories
{
    public class ResourceRepository : IResourceRepository

    {
        private readonly CatalogDbContext _context;

        public ResourceRepository(CatalogDbContext context) {
            _context = context;
        }
        public async Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)

          => await _context.Resources.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);



        public async Task<IReadOnlyList<Resource>> GetByTypeAsync(ResourceType type, CancellationToken cancellationToken = default)
        => await _context.Resources
            .AsNoTracking()
            .Where(r => r.Type == type)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<Resource>> GetAllAsync(CancellationToken cancellationToken = default)
           => await _context.Resources
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        public async  Task<IReadOnlyList<Resource>> GetAvailableAsync(CancellationToken cancellationToken = default)
         => await _context.Resources
            .AsNoTracking()
             .Where(r => r.IsAvailable)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        public async Task AddAsync(Resource resource, CancellationToken cancellationToken = default)
             => await _context.Resources.AddAsync(resource, cancellationToken);

        public async  Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) 
        {
            var resource = await _context.Resources.FindAsync([id], cancellationToken);
            if ( resource is not null)
            {
                _context.Resources.Remove(resource);
            }
        }
          

        public async  Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
         => await _context.Resources.AnyAsync(r => r.Id == id, cancellationToken);


        public Task UpdateAsync(Resource resource, CancellationToken cancellationToken = default)
        {
            _context.Resources.Update(resource);
            return Task.CompletedTask;
        }
    }
}
