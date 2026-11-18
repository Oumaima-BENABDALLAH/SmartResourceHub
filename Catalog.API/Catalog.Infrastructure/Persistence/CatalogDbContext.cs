using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Infrastructure.Persistence
{
    public class CatalogDbContext : DbContext, IUnitOfWork  //// IUnitOfWork : Regroupe plusieurs opérations de modification(ajout, mise à jour, suppression)
                                                            // dans une seule transaction pour garantir qu'elles réussissent ou échouent toutes ensemble.
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

        // Grace a ce ligne nous pourrions interroger , ajouter , modifier ou supprimer des ressources   , représente le table dans BD

        public DbSet<Resource> Resources => Set<Resource>();

        // OnModelCreating : configuration des entités 
        //// ModelBuilder : outil d'Entity Framework Core utilisé pour configurer la structure des tables, des relations et des contraintes de la base de données.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}