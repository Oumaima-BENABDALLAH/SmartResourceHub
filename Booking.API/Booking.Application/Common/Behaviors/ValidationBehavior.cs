using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Common.Behaviors
{
    // Intercepteur (Pipeline Behavior) MediatR qui exécute automatiquement tous les validateurs FluentValidation et bloque la requête si une donnée est incorrecte.
    public sealed class ValidationBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;
        public async  Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any()) return await next();
            var _context = new ValidationContext<TRequest>(request);

            var errors = _validators.Select(v => v.Validate(_context))
                             .SelectMany(r => r.Errors)
                             .Where(f => f != null)
                             .ToList();

            if (errors.Count != 0) throw new ValidationException(errors);

            return await next();
        }
    }
}
