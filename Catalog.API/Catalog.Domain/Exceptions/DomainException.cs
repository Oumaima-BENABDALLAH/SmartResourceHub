using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Domain.Exceptions
{
    public  class DomainException : Exception
    {
        public DomainException(String message) : base(message) { }
    }
}
