using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Common.Interfaces
{
    //regrouper plusieurs modifications(ajouts, modifications, suppressions) et de les envoyer toutes en même temps, dans une seule transaction, vers la base de données.

    public interface  IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    }
}
