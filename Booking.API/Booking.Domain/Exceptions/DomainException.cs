using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Domain.Exceptions
{
    //DomainException : Exception personnalisée au sein de la couche Domain. Elle est levée lorsqu'une action enfreint la logique métier.
    public class DomainException : Exception
    {
        public DomainException(string message ) : base(message) { }
    }
}
