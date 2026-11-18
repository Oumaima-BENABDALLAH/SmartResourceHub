using Booking.Domain.Enums;
using Booking.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Infrastructure.Persistence.Repositories
{
    public  class BookingRepository : IBookingRepository 
    {
        private readonly BookingDbContext _bookingDbContext;
        public BookingRepository(BookingDbContext context)
        {
            _bookingDbContext = context;
        }

        public async  Task AddAsync(Domain.Entities.Booking booking, CancellationToken cancellationToken = default)
             => await _bookingDbContext.Bookings.AddAsync(booking, cancellationToken);

        public async Task<Domain.Entities.Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
             => await _bookingDbContext.Bookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        //AsNoTracking() : Désactive le suivi d'Entity Framework pour économiser de la mémoire et accélérer l'affichage fonctionne seulement sur read ( lire une liste)
        public async Task<IReadOnlyList<Domain.Entities.Booking>> GetByResourceIdAsync(Guid resourceId, CancellationToken cancellationToken = default)

            => await _bookingDbContext.Bookings.AsNoTracking()
                                               .Where(b => b.ResourceId == resourceId)
                                               .OrderByDescending(b => b.CreatedAt)
                                               .ToListAsync(cancellationToken);
        public async  Task<Domain.Entities.Booking?> GetBySagaCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default)
             => await _bookingDbContext.Bookings.FirstOrDefaultAsync(b=> b.SagaCorrelationId == correlationId , cancellationToken);

        public async Task<IReadOnlyList<Domain.Entities.Booking>> GetByStatusAsync(BookingStatus status, CancellationToken cancellationToken = default)
            => await _bookingDbContext.Bookings.AsNoTracking()
                                            .Where(b => b.Status == status)
                                            .OrderByDescending(b => b.CreatedAt)
                                            .ToListAsync(cancellationToken);

        public async  Task<IReadOnlyList<Domain.Entities.Booking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => await _bookingDbContext.Bookings.AsNoTracking()
                                               .Where(b=> b.UserId == userId)
                                               .OrderByDescending(b => b.CreatedAt)
                                               .ToListAsync(cancellationToken);


        // AnyAsync s'arrête dès le premier élément trouvé sans charger toute la table.
        public async  Task<bool> HasConflictAsync(Guid resourceId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
            => await _bookingDbContext.Bookings.Where(b => b.ResourceId == resourceId)
            .Where(b => b.Status == BookingStatus.Confiremed || b.Status == BookingStatus.Pending)
            .AnyAsync(b => startTime < b.EndTime && endTime > b.StartTime, cancellationToken);
    

        public  Task UpdateAsync(Domain.Entities.Booking booking, CancellationToken cancellationToken = default)
        {
            _bookingDbContext.Bookings.Update(booking);
            // Task.CompletedTask : retourne une tâche déjà terminée, utilisée pour respecter la signature d'une méthode asynchrone (Task)
            // lorsqu'aucun traitement asynchrone réel n'est nécessaire.
            return Task.CompletedTask;
        }
    }
}
