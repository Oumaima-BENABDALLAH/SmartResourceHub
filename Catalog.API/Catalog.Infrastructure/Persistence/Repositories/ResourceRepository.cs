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


        //AsNoTracking() : Désactive le suivi d'Entity Framework pour économiser de la mémoire et accélérer l'affichage fonctionne seulement sur read ( lire une liste)

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

        //Elle vérifie si une ressource avec un identifiant (Guid id) spécifique existe déjà dans la base de données.
        public async  Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
         // AnyAsync : Vérifie efficacement l'existence d'un enregistrement en envoyant une requête SQL optimisée (ex: EXISTS) sans charger l'objet en mémoire.
         => await _context.Resources.AnyAsync(r => r.Id == id, cancellationToken);


        //Elle indique à Entity Framework qu'une ressource existante a été modifiée et que ses changements devront être enregistrés en base de données.
        public Task UpdateAsync(Resource resource, CancellationToken cancellationToken = default)
        {
            _context.Resources.Update(resource);
            // Task.CompletedTask : Retourne une tâche déjà terminée, utile ici car la méthode Update marque l'entité en mémoire (via le Change Tracker) de façon synchrone,
            // les modifications ne seront réellement envoyées à la base de données qu'au moment où le SaveChangesAsync() de l'unité de travail / UnitOfWork sera appelé.
            return Task.CompletedTask;
        }
    }
}
