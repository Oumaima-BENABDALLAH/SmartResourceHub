using Booking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Booking.Domain.Entities;

namespace Booking.Domain.Repositories
{
    public interface  IBookingRepository
    {
        Task<Booking.Domain.Entities.Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Booking.Domain.Entities.Booking?> GetBySagaCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Booking.Domain.Entities.Booking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Booking.Domain.Entities.Booking>> GetByResourceIdAsync(Guid resourceId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Booking.Domain.Entities.Booking>> GetByStatusAsync(BookingStatus status, CancellationToken cancellationToken = default);
        Task<bool> HasConflictAsync(Guid resourceId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
        Task AddAsync(Booking.Domain.Entities.Booking booking, CancellationToken cancellationToken = default);
        Task UpdateAsync(Booking.Domain.Entities.Booking booking, CancellationToken cancellationToken = default);
    }
}
