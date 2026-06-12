using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Domain.Exceptions
{
    public  class ResourceNotFoundException : DomainException
    {
        public ResourceNotFoundException(Guid id ) : base ($"Resource with id '{id}'was not found .") { }
    }
}
