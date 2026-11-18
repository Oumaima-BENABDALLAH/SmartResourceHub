using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Domain.Exceptions
{
    //Exception spécifique levée lorsqu'une entité Resource (Réservation) , recherchée par son identifiant unique est introuvable dans le système.

    public class ResourceNotFoundException : DomainException
    {
        public ResourceNotFoundException(Guid id ) : base ($"Resource with id '{id}'was not found .") { }
    }
}
