using Booking.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Infrastructure.Persistence
{
    public  class BookingDbContext : DbContext, IUnitOfWork  // IUnitOfWork : Regroupe plusieurs opérations de modification(ajout, mise à jour, suppression)
                                                             // dans une seule transaction pour garantir qu'elles réussissent ou échouent toutes ensemble.
    {
        public BookingDbContext (DbContextOptions<BookingDbContext> options) : base(options) { }

        // Grace a ce ligne nous pourrions interroger , ajouter , modifier ou supprimer des bookings  , représente le table dans BD
        public DbSet<Domain.Entities.Booking> Bookings => Set<Domain.Entities.Booking>();

        // OnModelCreating : configuration des entités 
        //// ModelBuilder : outil d'Entity Framework Core utilisé pour configurer la structure des tables, des relations et des contraintes de la base de données.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }

 }
